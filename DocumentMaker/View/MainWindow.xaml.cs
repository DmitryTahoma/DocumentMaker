using Dml.Controller.Validation;
using Dml.Model.Template;
using DocumentMaker.Controller;
using DocumentMaker.Controller.Controls;
using DocumentMaker.Model;
using DocumentMaker.Model.Files;
using DocumentMaker.Model.OfficeFiles.Human;
using DocumentMaker.View.Controls;
using DocumentMaker.View.Dialogs;
using MaterialDesignThemes.Wpf;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Markup;
using MessageBox = System.Windows.Forms.MessageBox;

namespace DocumentMaker
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public static readonly DependencyProperty TechnicalTaskDateTextProperty;
		public static readonly DependencyProperty ActDateTextProperty;
		public static readonly DependencyProperty AdditionNumTextProperty;
		public static readonly DependencyProperty ActSumProperty;
		public static readonly DependencyProperty ActSaldoProperty;

		static MainWindow()
		{
			TechnicalTaskDateTextProperty = DependencyProperty.Register("TechnicalTaskDateText", typeof(string), typeof(MainWindow));
			ActDateTextProperty = DependencyProperty.Register("ActDateText", typeof(string), typeof(MainWindow));
			AdditionNumTextProperty = DependencyProperty.Register("AdditionNumText", typeof(string), typeof(MainWindow));
			ActSumProperty = DependencyProperty.Register("ActSum", typeof(string), typeof(MainWindowController));
			ActSaldoProperty = DependencyProperty.Register("ActSaldo", typeof(string), typeof(MainWindowController));
		}

		private readonly MainWindowController controller;
		private readonly FolderBrowserDialog folderBrowserDialog;
		private readonly OpenFileDialog openFileDialog;
		private readonly InputingValidator inputingValidator;

		public MainWindow(string[] args)
		{
			controller = new MainWindowController(args);
			controller.Load();
			SetWindowSettingsFromController();

			InitializeComponent();
			TechnicalTaskDatePicker.Language = XmlLanguage.GetLanguage(System.Globalization.CultureInfo.CurrentCulture.IetfLanguageTag);
			ActDatePicker.Language = XmlLanguage.GetLanguage(System.Globalization.CultureInfo.CurrentCulture.IetfLanguageTag);
			DataHeader.HideWorkTypeLabel();

			DataFooter.SubscribeAddition((x) =>
			{
				x.Controller.IsRework = false;
				controller.BackDataControllers.Add(x.Controller);
				x.SetViewByTemplate(controller.TemplateType);
				UpdateActSum();

				DmxFile selectedFile = controller.GetSelectedFile();
				if (selectedFile != null)
				{
					selectedFile.AddBackModel(x.Controller.GetModel());
				}
			});
			DataFooter.SubscribeChangingSum(UpdateSaldo);

			ReworkDataFooter.SubscribeAddition((x) =>
			{
				x.Controller.IsRework = true;
				controller.BackDataControllers.Add(x.Controller);
				x.SetViewByTemplate(controller.TemplateType);
				x.SetWorkTypesList(controller.CurrentWorkTypesList);
				UpdateActSum();

				DmxFile selectedFile = controller.GetSelectedFile();
				if (selectedFile != null)
				{
					selectedFile.AddBackModel(x.Controller.GetModel());
				}
				x.UpdateInputStates();
			});
			ReworkDataFooter.SubscribeChangingSum(UpdateSaldo);

			folderBrowserDialog = new FolderBrowserDialog();
			openFileDialog = new OpenFileDialog() { Multiselect = true, Filter = "DocumentMaker files (*" + DmxFile.Extension + ")|*" + DmxFile.Extension };
			inputingValidator = new InputingValidator();

			ActSumInput.CommandBindings.Add(new System.Windows.Input.CommandBinding(ApplicationCommands.Paste, inputingValidator.BlockingCommand));
		}

		public IList<DmxFile> OpenedFilesList => controller.OpenedFilesList;

		public IList<FullDocumentTemplate> DocumentTemplatesList => controller.DocumentTemplatesList;

		public string TechnicalTaskDateText
		{
			get => (string)GetValue(TechnicalTaskDateTextProperty);
			set => SetValue(TechnicalTaskDateTextProperty, value);
		}

		public string ActDateText
		{
			get => (string)GetValue(ActDateTextProperty);
			set => SetValue(ActDateTextProperty, value);
		}

		public string AdditionNumText
		{
			get => (string)GetValue(AdditionNumTextProperty);
			set => SetValue(AdditionNumTextProperty, value);
		}

		public string ActSum
		{
			get => (string)GetValue(ActSumProperty);
			set
			{
				SetValue(ActSumProperty, value);
				controller.ActSum = value;

				if (OpenedFilesComboBox != null
					 && OpenedFilesComboBox.SelectedItem is DmxFile selectedFile)
				{
					selectedFile.ActSum = value;
				}
			}
		}

		public string ActSaldo
		{
			get => (string)GetValue(ActSaldoProperty);
			set => SetValue(ActSaldoProperty, value);
		}

		public double IconSize { get; } = 24;

		private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			SetDataToController();
			controller.WindowTop = Top;
			controller.WindowLeft = Left;
			controller.WindowHeight = Height;
			controller.WindowWidth = Width;
			controller.WindowState = WindowState == WindowState.Minimized ? WindowState.Normal : WindowState;

			controller.Save();
		}

		private void WindowLoaded(object sender, RoutedEventArgs e)
		{
			CheckFiles();
			if (controller != null)
			{
				SetDataFromController();
				AddLoadedBackData();
				UpdateViewBackData();
				string[] files = controller.GetOpenLaterFiles();
				OpenFiles(files);
				LoadFiles();
				if (files != null && files.Length > 0)
				{
					SetSelectedFile(files.Last());
				}
				UpdateActSum();

#if INCLUDED_UPDATER_API
				try
				{
					bool _ = false;
					UpdateInformer informer = new UpdateInformer();
					informer.Notify(ref _);
				}
				catch
				{
					MessageBox.Show("Невозможно подключиться к шаре!", "ActCreator Update", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
				}
#endif
			}
		}

		private void ChangedDocumentTemplate(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			if (controller != null
				&& sender is System.Windows.Controls.ComboBox comboBox
				&& comboBox.SelectedItem is DocumentTemplate documentTemplate)
			{
				controller.TemplateType = documentTemplate.Type;
				UpdateViewBackData();

				if (OpenedFilesComboBox != null
					&& OpenedFilesComboBox.SelectedItem is DmxFile selectedFile)
				{
					selectedFile.TemplateType = documentTemplate.Type;
				}
			}
		}

		private void ChangedHuman(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			if (controller != null
				&& sender is System.Windows.Controls.ComboBox comboBox
				&& comboBox.SelectedItem is HumanData humanData)
			{
				controller.SetHuman(humanData);
				SetDataFromController();

				if (OpenedFilesComboBox != null
					&& OpenedFilesComboBox.SelectedItem is DmxFile selectedFile)
				{
					selectedFile.SelectedHuman = humanData.Name;
				}
			}
		}

		private void ExportBtnClick(object sender, RoutedEventArgs e)
		{
			SetDataToController();
			if (controller.Validate(out string errorText))
			{
				if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					bool isShowResult = true;
					controller.Export(folderBrowserDialog.SelectedPath);

					if (controller.HasNoMovedFiles)
					{
						if (MessageBox.Show("Файли за заданними путями вже існують.\n\n" + controller.GetInfoNoMovedFiles() + "\nЗамінити?",
											"DocumentMaker | Export",
											MessageBoxButtons.YesNo,
											MessageBoxIcon.Question)
												== System.Windows.Forms.DialogResult.Yes)
						{
							controller.ReplaceCreatedFiles();

							if (controller.HasNoMovedFiles)
							{
								MessageBox.Show("Не вдалось перемістити наступні файли. Можливо вони відкриті в іншій програмі.\n\n" + controller.GetInfoNoMovedFiles(),
												"DocumentMaker | Export",
												MessageBoxButtons.OK,
												MessageBoxIcon.Warning);

								controller.RemoveTemplates();
								isShowResult = false;
							}
						}
						else
						{
							controller.RemoveTemplates();
						}
					}

					if (isShowResult && MessageBox.Show("Файли збережені.\nВідкрити папку з файлами?",
										"DocumentMaker | Export",
										MessageBoxButtons.YesNo,
										MessageBoxIcon.Information,
										MessageBoxDefaultButton.Button2)
						== System.Windows.Forms.DialogResult.Yes)
					{
						Process.Start("explorer", folderBrowserDialog.SelectedPath);
					}
				}
			}
			else
			{
				MessageBox.Show(errorText, "DocumentMaker | Validation", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void OpenFileClick(object sender, RoutedEventArgs e)
		{
			if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				OpenFiles(openFileDialog.FileNames);
				LoadFiles();
				SetSelectedFile(openFileDialog.FileNames.Last());
			}
		}

		private void CloseFileClick(object sender, RoutedEventArgs e)
		{
			int index = OpenedFilesComboBox.SelectedIndex;
			if (index != -1 && OpenedFilesComboBox.SelectedItem is DmxFile file)
			{
				controller.CloseFile(file);
				OpenedFilesComboBox.SelectedIndex = OpenedFilesComboBox.Items.Count <= index ? index - 1 : index;
			}
		}

		private async void InfoBtnClick(object sender, RoutedEventArgs e)
		{
			if (OpenedFilesComboBox.SelectedItem is DmxFile selectedFile && selectedFile.Loaded)
			{
				await DialogHost.Show(new HumanInformationDialog(controller.GetSelectedHuman()));
			}
			else
				MessageBox.Show("Спочатку необхідно відкрити файл.",
								"DocumentMaker | Картка людини",
								MessageBoxButtons.OK,
								MessageBoxIcon.Information);
		}

		private void CorrectSaldoClick(object sender, RoutedEventArgs e)
		{
			if (uint.TryParse(ActSum, out uint actSum) && actSum != 0)
			{
				SetDataToController();
				controller.CorrectSaldo();

				SetDataFromControllerBackDatas();
			}
			else
			{
				MessageBox.Show("Сума для корегування не може бути нульовою.",
					"Корегування | Помилка",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error);
			}
		}

		private async void CorrectDevelopClick(object sender, RoutedEventArgs e)
		{
			if (uint.TryParse(ActSum, out uint actSum) && actSum != 0)
			{
				CorrectDevelopmentDialog dialog = new CorrectDevelopmentDialog
				{
					NumberText = controller.CorrectDevelopmentWindow_NumberText,
					TakeSumFromSupport = controller.CorrectDevelopmentWindow_TakeSumFromSupport,
				};
				await DialogHost.Show(dialog);

				controller.CorrectDevelopmentWindow_NumberText = dialog.NumberText;
				controller.CorrectDevelopmentWindow_TakeSumFromSupport = dialog.TakeSumFromSupport;

				if (dialog.IsCorrection && int.TryParse(dialog.NumberText, out int sum))
				{
					controller.CorrectDevelopment(sum, dialog.TakeSumFromSupport);
					SetDataFromControllerBackDatas();
				}
			}
			else
			{
				MessageBox.Show("Сума для корегування не може бути нульовою.",
					"Корегування | Помилка",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error);
			}
		}

		private async void CorrectSupportClick(object sender, RoutedEventArgs e)
		{
			if (uint.TryParse(ActSum, out uint actSum) && actSum != 0)
			{
				CorrectSupportDialog dialog = new CorrectSupportDialog
				{
					NumberText = controller.CorrectSupportWindow_NumberText,
					TakeSumFromDevelopment = controller.CorrectSupportWindow_TakeSumFromDevelopment
				};
				await DialogHost.Show(dialog);

				controller.CorrectSupportWindow_NumberText = dialog.NumberText;
				controller.CorrectSupportWindow_TakeSumFromDevelopment = dialog.TakeSumFromDevelopment;

				if (dialog.IsCorrection && int.TryParse(dialog.NumberText, out int sum))
				{
					controller.CorrectSupport(sum, dialog.TakeSumFromDevelopment);
					SetDataFromControllerBackDatas();
				}
			}
			else
			{
				MessageBox.Show("Сума для корегування не може бути нульовою.",
					"Корегування | Помилка",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error);
			}
		}

		private void SetDataFromControllerBackDatas()
		{
			SetDataFromControllerBackDatas(BacksData);
			SetDataFromControllerBackDatas(ReworkBacksData);
		}

		private void SetDataFromControllerBackDatas(StackPanel stackPanel)
		{
			if (stackPanel != null)
			{
				foreach (UIElement elem in stackPanel.Children)
				{
					if (elem is FullBackData backData)
					{
						backData.SetDataFromController();
					}
				}
			}
		}

		private void UpdateViewBackData()
		{
			foreach (UIElement control in BacksData.Children)
			{
				if (control is FullBackData backData)
				{
					backData.SetViewByTemplate(controller.TemplateType);
				}
			}
			foreach (UIElement control in ReworkBacksData.Children)
			{
				if (control is FullBackData backData)
				{
					backData.SetViewByTemplate(controller.TemplateType);
					backData.SetWorkTypesList(controller.CurrentWorkTypesList);
				}
			}
			DataHeader.SetViewByTemplate(controller.TemplateType);
			ReworkDataHeader.SetViewByTemplate(controller.TemplateType);
			DataFooter.SetViewByTemplate(controller.TemplateType);
			ReworkDataFooter.SetViewByTemplate(controller.TemplateType);
		}

		private void SetDataFromController()
		{
			DocumentTemplateComboBox.SelectedIndex = (int)controller.TemplateType;
			TechnicalTaskDatePicker.Text = controller.TechnicalTaskDateText;
			ActDatePicker.Text = controller.ActDateText;
			AdditionNumTextInput.Text = controller.AdditionNumText;
			ActSumInput.Text = controller.ActSum;
			ActSaldoInput.Text = controller.ActSaldo;
		}

		private void SetDataToController()
		{
			controller.TechnicalTaskDateText = TechnicalTaskDateText;
			controller.ActDateText = ActDateText;
			controller.AdditionNumText = AdditionNumText;
			controller.ActSum = ActSum;
			controller.ActSaldo = ActSaldo;
		}

		private void OpenFiles(string[] filenames)
		{
			controller.OpenFiles(filenames, out string skippedFiles);

			if (!string.IsNullOrEmpty(skippedFiles))
			{
				MessageBox.Show("Формат файлів не підтримується:\n\n" + skippedFiles,
					"DocumentMaker | Open Files",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error);
			}
		}

		private void LoadFiles()
		{
			controller.LoadFiles();
			SetSelectedFile(controller.GetSelectedFile()?.FullName);
		}

		private void WindowPreviewDrop(object sender, System.Windows.DragEventArgs e)
		{
			if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop, true))
			{
				string[] filenames = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop, true);
				OpenFiles(filenames);
				LoadFiles();
				SetSelectedFile(filenames.Last(filename => filename.EndsWith(DmxFile.Extension)));
				e.Handled = true;
			}
		}

		private void WindowDragEnter(object sender, System.Windows.DragEventArgs e)
		{
			bool isCorrect = true;

			if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop, true))
			{
				string[] filenames = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop, true);
				foreach (string filename in filenames)
				{
					if (!File.Exists(filename))
					{
						isCorrect = false;
						break;
					}
					FileInfo info = new FileInfo(filename);
					if (info.Extension != DmxFile.Extension)
					{
						isCorrect = false;
						break;
					}
				}
			}
			if (isCorrect == true)
				e.Effects = System.Windows.DragDropEffects.All;
			else
				e.Effects = System.Windows.DragDropEffects.None;
			e.Handled = true;
		}

		private void OpenedFilesSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			if (sender is System.Windows.Controls.ComboBox comboBox && comboBox.SelectedItem is DmxFile selectedFile && selectedFile.Loaded)
			{
				controller.SetDataFromFile(selectedFile);
				SetDataFromController();
				AddLoadedBackData();
				controller.SetSelectedFile(selectedFile);
				UpdateViewBackData();
				UpdateActSum();
			}
		}

		private void ActSumTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
		{
			UpdateActSum();
		}

		private void UIntValidating(object sender, System.Windows.Input.TextCompositionEventArgs e)
		{
			inputingValidator.UIntInputing_PreviewTextInput(sender, e);
		}

		private void AddLoadedBackData()
		{
			foreach (FullBackDataController backDataController in controller.BackDataControllers)
			{
				if (backDataController.IsRework)
				{
					ReworkDataFooter.AddLoadedBackData(backDataController);
				}
				else
				{
					DataFooter.AddLoadedBackData(backDataController);
				}
			}
			DataFooter.UpdateBackDataIds();
			ReworkDataFooter.UpdateBackDataIds();
		}

		private void SetSelectedFile(string filename)
		{
			foreach (DmxFile file in OpenedFilesList)
			{
				if (file.FullName == filename || file.Name == filename)
				{
					OpenedFilesComboBox.SelectedItem = file;
					break;
				}
			}
		}

		private void SetWindowSettingsFromController()
		{
			Top = controller.WindowTop;
			Left = controller.WindowLeft;
			Height = controller.WindowHeight;
			Width = controller.WindowWidth;
			WindowState = controller.WindowState;
			WindowValidator.MoveToValidPosition(this);
		}

		private void UpdateActSum()
		{
			if (uint.TryParse(ActSum, out uint sum))
			{
				UpdateActSumBackDataPanel(BacksData, sum);
				UpdateActSumBackDataPanel(ReworkBacksData, sum);
				DataFooter?.SetActSum(sum);
				ReworkDataFooter?.SetActSum(sum);
			}
			UpdateSaldo();
		}

		private void UpdateActSumBackDataPanel(StackPanel stackPanel, uint sum)
		{
			if (stackPanel != null)
			{
				foreach (UIElement elem in stackPanel.Children)
				{
					if (elem is FullBackData backData)
					{
						backData.SetActSum(sum);
					}
				}
			}
		}

		private void UpdateSaldo()
		{
			if (uint.TryParse(ActSum, out uint sum) && uint.TryParse(DataFooter.AllSum, out uint curSum) && uint.TryParse(ReworkDataFooter.AllSum, out uint curSumRework))
			{
				ActSaldo = ((int)sum - (int)(curSum + curSumRework)).ToString();
				ActSaldoInput.Text = ActSaldo;
			}
		}

		private void CheckFiles()
		{
			List<string> files = new List<string>()
			{
				"HumanData.xlsx",
				"projectnames.xml",
				"SupportTypes.xlsx",
			};

			if (!ProgramValidator.ValidateExistsFiles(files))
			{
				string notFindedFiles = "";
				foreach (string file in files)
				{
					notFindedFiles += '\n' + file;
				}

				MessageBox.Show("Не знайдені необхідні файли (програма може працювати з помилками):\n" + notFindedFiles,
					"DocumentMaker | Відсутні файли",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error);
			}
		}
	}
}
