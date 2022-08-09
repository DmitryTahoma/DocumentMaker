using Dml.Controller;
using Dml.Controller.Validation;
using Dml.Controls;
using Dml.Model.Files;
using Dml.Model.Template;
using DocumentMaker.Controller;
using DocumentMaker.Model.OfficeFiles.Human;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.Forms.MessageBox;

namespace DocumentMaker
{
	public delegate void ActionWithBackData(BackData backData);

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public static readonly DependencyProperty TechnicalTaskDateTextProperty;
		public static readonly DependencyProperty ActDateTextProperty;
		public static readonly DependencyProperty AdditionNumTextProperty;
		public static readonly DependencyProperty SelectedHumanProperty;
		public static readonly DependencyProperty HumanIdTextProperty;
		public static readonly DependencyProperty AddressTextProperty;
		public static readonly DependencyProperty PaymentAccountTextProperty;
		public static readonly DependencyProperty BankNameProperty;
		public static readonly DependencyProperty MfoTextProperty;
		public static readonly DependencyProperty ContractNumberTextProperty;
		public static readonly DependencyProperty ContractDateTextProperty;

		static MainWindow()
		{
			TechnicalTaskDateTextProperty = DependencyProperty.Register("TechnicalTaskDateText", typeof(string), typeof(InputWithText));
			ActDateTextProperty = DependencyProperty.Register("ActDateText", typeof(string), typeof(InputWithText));
			AdditionNumTextProperty = DependencyProperty.Register("AdditionNumText", typeof(string), typeof(InputWithText));
			SelectedHumanProperty = DependencyProperty.Register("FullHumanName", typeof(string), typeof(InputWithText));
			HumanIdTextProperty = DependencyProperty.Register("HumanIdText", typeof(string), typeof(InputWithText));
			AddressTextProperty = DependencyProperty.Register("AddressText", typeof(string), typeof(InputWithText));
			PaymentAccountTextProperty = DependencyProperty.Register("PaymentAccountText", typeof(string), typeof(InputWithText));
			BankNameProperty = DependencyProperty.Register("BankName", typeof(string), typeof(InputWithText));
			MfoTextProperty = DependencyProperty.Register("MfoText", typeof(string), typeof(InputWithText));
			ContractNumberTextProperty = DependencyProperty.Register("ContractNumberText", typeof(string), typeof(InputWithText));
			ContractDateTextProperty = DependencyProperty.Register("ContractDateText", typeof(string), typeof(InputWithText));
		}

		private readonly MainWindowController controller;
		private readonly FolderBrowserDialog folderBrowserDialog;
		private readonly OpenFileDialog openFileDialog;

		public MainWindow(string[] args)
		{
			controller = new MainWindowController(args);
			controller.Load();
			SetWindowSettingsFromController();

			InitializeComponent();

			DataFooter.SubscribeAddition((x) =>
			{
				controller.BackDataControllers.Add(x.Controller);
				x.SetViewByTemplate(controller.TemplateType);
			});
			DataFooter.SubscribeRemoving((x) =>
			{
				controller.BackDataControllers.Remove(x.Controller);
			});
			DataFooter.SubscribeClearing(() =>
			{
				controller.BackDataControllers.Clear();
			});

			folderBrowserDialog = new FolderBrowserDialog();
			openFileDialog = new OpenFileDialog() { Multiselect = true, Filter = "DocumentMaker files (*" + DmxFile.Extension + ")|*" + DmxFile.Extension };
		}

		public IList<DmxFile> OpenedFilesList => controller.OpenedFilesList;

		public IList<DocumentTemplate> DocumentTemplatesList => controller.DocumentTemplatesList;

		public string TechnicalTaskDateText
		{
			get => (string)GetValue(TechnicalTaskDateTextProperty);
			set
			{
				SetValue(TechnicalTaskDateTextProperty, value);
				controller.TechnicalTaskDateText = value;
			}
		}

		public string ActDateText
		{
			get => (string)GetValue(ActDateTextProperty);
			set
			{
				SetValue(ActDateTextProperty, value);
				controller.ActDateText = value;
			}
		}

		public string AdditionNumText
		{
			get => (string)GetValue(AdditionNumTextProperty);
			set
			{
				SetValue(AdditionNumTextProperty, value);
				controller.AdditionNumText = value;
			}
		}

		public string SelectedHuman
		{
			get => (string)GetValue(SelectedHumanProperty);
			set
			{
				SetValue(SelectedHumanProperty, value);
				controller.SelectedHuman = value;
			}
		}

		public IList<HumanData> HumanFullNameList => controller.HumanFullNameList;

		public string HumanIdText
		{
			get => (string)GetValue(HumanIdTextProperty);
			set
			{
				SetValue(HumanIdTextProperty, value);
				controller.HumanIdText = value;
			}
		}

		public string AddressText
		{
			get => (string)GetValue(AddressTextProperty);
			set
			{
				SetValue(AddressTextProperty, value);
				controller.AddressText = value;
			}
		}

		public string PaymentAccountText
		{
			get => (string)GetValue(PaymentAccountTextProperty);
			set
			{
				SetValue(PaymentAccountTextProperty, value);
				controller.PaymentAccountText = value;
			}
		}

		public string BankName
		{
			get => (string)GetValue(BankNameProperty);
			set
			{
				SetValue(BankNameProperty, value);
				controller.BankName = value;
			}
		}

		public string MfoText
		{
			get => (string)GetValue(MfoTextProperty);
			set
			{
				SetValue(MfoTextProperty, value);
				controller.MfoText = value;
			}
		}

		public string ContractNumberText
		{
			get => (string)GetValue(ContractNumberTextProperty);
			set
			{
				SetValue(ContractNumberTextProperty, value);
				controller.ContractNumberText = value;
			}
		}

		public string ContractDateText
		{
			get => (string)GetValue(ContractDateTextProperty);
			set
			{
				SetValue(ContractDateTextProperty, value);
				controller.ContractDateText = value;
			}
		}

		private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			controller.WindowTop = Top;
			controller.WindowLeft = Left;
			controller.WindowHeight = Height;
			controller.WindowWidth = Width;
			controller.WindowState = WindowState == WindowState.Minimized ? WindowState.Normal : WindowState;

			controller.Save();
		}

		private void WindowLoaded(object sender, RoutedEventArgs e)
		{
			if (controller != null)
			{
				SetDataFromController();
				AddLoadedBackData();
				UpdateViewBackData();
				string[] files = controller.GetOpenLaterFiles();
				OpenFiles(files);
				LoadFiles();
				if(files != null && files.Length > 0)
				{
					SetSelectedFile(files.Last());
				}
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

				if(OpenedFilesComboBox != null
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
			if(openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
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

		private void UpdateViewBackData()
		{
			foreach (UIElement control in BacksData.Children)
			{
				if (control is BackData backData)
				{
					backData.SetViewByTemplate(controller.TemplateType);
				}
			}
			DataFooter.SetViewByTemplate(controller.TemplateType);
			DataHeader.SetViewByTemplate(controller.TemplateType);
		}

		private void SetDataFromController()
		{
			DocumentTemplateComboBox.SelectedIndex = (int)controller.TemplateType;
			TechnicalTaskDateTextInput.InputText = controller.TechnicalTaskDateText;
			ActDateTextInput.InputText = controller.ActDateText;
			AdditionNumTextInput.InputText = controller.AdditionNumText;
			HumanFullNameComboBox.Text = controller.SelectedHuman;
			HumanIdTextInput.InputText = controller.HumanIdText;
			AddressTextInput.InputText = controller.AddressText;
			PaymentAccountTextInput.InputText = controller.PaymentAccountText;
			BankNameInput.InputText = controller.BankName;
			MfoTextInput.InputText = controller.MfoText;
			ContractNumberTextInput.InputText = controller.ContractNumberText;
			ContractDateTextInput.InputText = controller.ContractDateText;
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
			SetSelectedFile(controller.GetSelectedFileName());
		}

		private void WindowPreviewDrop(object sender, System.Windows.DragEventArgs e)
		{
			string[] filenames = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop, true);
			OpenFiles(filenames);
			LoadFiles();
			SetSelectedFile(filenames.Last(filename => filename.EndsWith(DmxFile.Extension)));
			e.Handled = true;
		}

		private void WindowDragEnter(object sender, System.Windows.DragEventArgs e)
		{
			bool isCorrect = true;

			if(e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop, true))
			{
				string[] filenames = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop, true);
				foreach(string filename in filenames)
				{
					if(!File.Exists(filename))
					{
						isCorrect = false;
						break;
					}
					FileInfo info = new FileInfo(filename);
					if(info.Extension != DmxFile.Extension)
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
			if(sender is System.Windows.Controls.ComboBox comboBox && comboBox.SelectedItem is DmxFile selectedFile && selectedFile.Loaded)
			{
				DataFooter.ClearBackData();
				controller.SetDataFromFile(selectedFile);
				SetDataFromController();
				AddLoadedBackData();
				controller.SetSelectedFile(selectedFile); 
				UpdateViewBackData();
			}
		}

		private void AddLoadedBackData()
		{
			foreach (BackDataController backDataController in controller.BackDataControllers)
			{
				DataFooter.AddLoadedBackData(backDataController);
			}
		}

		private void SetSelectedFile(string filename)
		{
			foreach(DmxFile file in OpenedFilesList)
			{
				if(file.FullName == filename || file.Name == filename)
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
	}
}
