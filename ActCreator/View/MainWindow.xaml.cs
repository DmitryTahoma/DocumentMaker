using ActCreator.Controller;
using ActCreator.Controller.Controls;
using ActCreator.View.Controls;
using Dml.Controller.Validation;
using Dml.Model.Files;
using Dml.Model.Template;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.Forms.MessageBox;

#if INCLUDED_UPDATER_API
using UpdaterAPI;
using UpdaterAPI.Resources;
using System.Threading.Tasks;
#endif

namespace ActCreator
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public static readonly DependencyProperty SelectedHumanProperty;
		public static readonly DependencyProperty DataTableVisibilityProperty;
		public static readonly DependencyProperty FileContentVisibilityProperty;
		public static readonly DependencyProperty CreateFileButtonVisibilityProperty;

		static MainWindow()
		{
			SelectedHumanProperty = DependencyProperty.Register("SelectedHuman", typeof(string), typeof(MainWindowController));
			DataTableVisibilityProperty = DependencyProperty.Register("DataTableVisibility", typeof(Visibility), typeof(MainWindow));
			FileContentVisibilityProperty = DependencyProperty.Register("FileContentVisibility", typeof(Visibility), typeof(MainWindow));
			CreateFileButtonVisibilityProperty = DependencyProperty.Register("CreateFileButtonVisibility", typeof(Visibility), typeof(MainWindow));
		}

		private readonly MainWindowController controller;
		private readonly SaveFileDialog saveFileDialog;
		private readonly OpenFileDialog openFileDialog;

		public MainWindow(string[] args)
		{
			controller = new MainWindowController(args);
			controller.Load();
			SetWindowSettingsFromController();

			openFileDialog = new OpenFileDialog { Filter= "Файли акту (*" + BaseDmxFile.Extension + ")|*" + BaseDmxFile.Extension };

			InitializeComponent();

			DataFooter.SubscribeAddition((x) =>
			{
				HaveUnsavedChanges = true;
				controller.BackDataControllers.Add(x.Controller);
				x.SetViewByTemplate(controller.TemplateType);
				x.SetGameNameList(controller.GameNameList);
				x.PropertyChanged += OnBackDataPropertyChanged;
			});
			DataFooter.SubscribeRemoving((x) =>
			{
				HaveUnsavedChanges = true;
				controller.BackDataControllers.Remove(x.Controller);
				DataFooter.UpdateBackDataIds();
			});
			DataFooter.SubscribeClearing(() =>
			{
				HaveUnsavedChanges = true;
				controller.BackDataControllers.Clear();
			});

			saveFileDialog = new SaveFileDialog
			{
				DefaultExt = Dml.Model.Files.BaseDmxFile.Extension,
				Filter = "Файл акту (*" + Dml.Model.Files.BaseDmxFile.Extension + ")|*" + Dml.Model.Files.BaseDmxFile.Extension
			};

#if INCLUDED_UPDATER_API
			AssemblyLoader.LoadWinScp();
#endif
		}

		public IList<DocumentTemplate> DocumentTemplatesList => controller.DocumentTemplatesList;

		public string SelectedHuman
		{
			get => (string)GetValue(SelectedHumanProperty);
			set
			{
				SetValue(SelectedHumanProperty, value);
				controller.SelectedHuman = value;
				UpdateDataTableVisibility();
				HaveUnsavedChanges = true;
			}
		}

		public Visibility DataTableVisibility
		{
			get => (Visibility)GetValue(DataTableVisibilityProperty); 
			set => SetValue(DataTableVisibilityProperty, value); 
		}

		public Visibility FileContentVisibility
		{
			get => (Visibility)GetValue(FileContentVisibilityProperty); 
			set => SetValue(FileContentVisibilityProperty, value); 
		}

		public Visibility CreateFileButtonVisibility
		{
			get => (Visibility)GetValue(CreateFileButtonVisibilityProperty); 
			set => SetValue(CreateFileButtonVisibilityProperty, value); 
		}

		public IList<string> HumanFullNameList => controller.HumanFullNameList;

		public bool HaveUnsavedChanges 
		{
			get => controller.HaveUnsavedChanges;
			set 
			{
				controller.HaveUnsavedChanges = value;
				UpdateTitle();
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

#if INCLUDED_UPDATER_API
		private async void WindowLoaded(object sender, RoutedEventArgs e)
#else
		private void WindowLoaded(object sender, RoutedEventArgs e)
#endif
		{
			WindowState = controller.WindowState;
			if (controller != null)
			{
				if(controller.HaveOpenLaterFiles)
				{
					OpenFile(controller.GetOpenLaterFile());
				}
				controller.IsLoadingLastSession = true;
				SetDataFromController();
				controller.IsLoadingLastSession = false;
				ResetHaveUnsavedChanges();
				UpdateTitle();
				UpdateFileContentVisibility();

#if INCLUDED_UPDATER_API
				await Task.Run(() =>
				{
					try
					{
						bool _ = false;
						UpdateInformer informer = new UpdateInformer();
						informer.Notify(ref _, isHidden: true);
					}
					catch
					{
						MessageBox.Show("Невозможно подключиться к шаре!", "ActCreator Update", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
					}
				});
#endif
			}
			ResetHaveUnsavedChanges();
		}

		private void ChangedDocumentTemplate(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			if (controller != null
				&& sender is System.Windows.Controls.ComboBox comboBox
				&& comboBox.SelectedItem is DocumentTemplate documentTemplate)
			{
				HaveUnsavedChanges = true;
				controller.TemplateType = documentTemplate.Type;
				UpdateViewBackData();
				UpdateDataTableVisibility();
			}
		}

		private void ExportBtnClick(object sender, RoutedEventArgs e)
		{
			if (controller.Validate(out string errorText))
			{
				saveFileDialog.FileName = controller.GetDmxFileName();
				if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					controller.ExportDmx(saveFileDialog.FileName);

					if (MessageBox.Show("Файл збережений.\nВідкрити папку з файлом?",
										"ActCreator | Export",
										MessageBoxButtons.YesNo,
										MessageBoxIcon.Information,
										MessageBoxDefaultButton.Button2)
						== System.Windows.Forms.DialogResult.Yes)
					{
						Process.Start("explorer", Path.GetDirectoryName(saveFileDialog.FileName));
					}
				}
			}
			else
			{
				MessageBox.Show(errorText, "ActCreator | Validation", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void OpenFileClick(object sender, RoutedEventArgs e)
		{
			if(openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				OpenFile(openFileDialog.FileName);
			}
		}

		private void CloseFileClick(object sender, RoutedEventArgs e)
		{
			if (controller.IsOpenedFile)
			{
				CheckNeedSaveBeforeClosing(out DialogResult res);
				if (res == System.Windows.Forms.DialogResult.Cancel)
				{
					return;
				}

				controller.CloseFile();
				DataFooter.ClearBackData();
				ResetHaveUnsavedChanges();
				UpdateTitle();
				UpdateFileContentVisibility();
			}
			else
			{
				MessageBox.Show("Спочатку необхідно відкрити файл.",
								"ActCreator | Закриття файлу",
								MessageBoxButtons.OK,
								MessageBoxIcon.Information);
			}
		}

		private void CreateFileClick(object sender, RoutedEventArgs e)
		{
			controller.CreateFile(); 
			UpdateDataTableVisibility();
			ResetHaveUnsavedChanges();
			UpdateTitle();
			UpdateFileContentVisibility();
		}

		private void WindowPreviewDrop(object sender, System.Windows.DragEventArgs e)
		{
			if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop, true))
			{
				string[] filenames = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop, true);
				if(filenames.Length > 0 && filenames[0] != null)
				{
					OpenFile(filenames[0]);
					e.Handled = true;
				}
			}
		}

		private void WindowDragEnter(object sender, System.Windows.DragEventArgs e)
		{
			bool isCorrect = true;

			if(e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop, true))
			{
				string[] filenames = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop, true);
				if(filenames.Length != 1 || !File.Exists(filenames[0]) || new FileInfo(filenames[0]).Extension != BaseDmxFile.Extension)
				{
					isCorrect = false;
				}
			}

			e.Effects = isCorrect ? System.Windows.DragDropEffects.All : System.Windows.DragDropEffects.None;
			e.Handled = true;
		}

		private void OnBackDataPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(!controller.IsOpeningFile && !controller.IsLoadingLastSession)
			{
				HaveUnsavedChanges = true;
				UpdateTitle();
			}
		}

		private void UpdateViewBackData()
		{
			foreach (UIElement control in BacksData.Children)
			{
				if (control is ShortBackData backData)
				{
					backData.SetViewByTemplate(controller.TemplateType);
					backData.SetGameNameList(controller.GameNameList);
				}
			}
			DataFooter.SetViewByTemplate(controller.TemplateType);
			DataHeader.SetViewByTemplate(controller.TemplateType);
		}

		private void SetWindowSettingsFromController()
		{
			Top = controller.WindowTop;
			Left = controller.WindowLeft;
			Height = controller.WindowHeight;
			Width = controller.WindowWidth;
			WindowValidator.MoveToValidPosition(this);
		}

		private void SetDataFromController()
		{
			DocumentTemplateComboBox.SelectedIndex = (int)controller.TemplateType;
			HumanFullNameComboBox.Text = controller.SelectedHuman;
		}

		private void AddLoadedBackData()
		{
			DataFooter.ClearData();
			foreach (ShortBackDataController backDataController in controller.BackDataControllers)
			{
				ShortBackData backData = DataFooter.AddLoadedBackData(backDataController);
				backData.PropertyChanged += OnBackDataPropertyChanged;
			}
			DataFooter.UpdateBackDataIds();
		}

		private void OpenFile(string filename)
		{
			if(!controller.OpenFile(filename))
			{
				MessageBox.Show("Не вдалось відкрити файл:\n\n" + filename, 
					"ActCreator | Open Files",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error);
			}
			else
			{
				controller.IsOpeningFile = true;
				SetDataFromController();
				AddLoadedBackData();
				UpdateViewBackData();
				controller.IsOpeningFile = false;
				ResetHaveUnsavedChanges();
				UpdateTitle();
				UpdateFileContentVisibility();
			}
		}

		private void ResetHaveUnsavedChanges()
		{
			controller.ResetHaveUnsavedChanges();
		}

		private void UpdateDataTableVisibility()
		{
			DataTableVisibility = (controller.TemplateType == DocumentTemplateType.Empty || string.IsNullOrEmpty(SelectedHuman)) ? Visibility.Collapsed : Visibility.Visible;
		}

		private void UpdateTitle()
		{
			if (controller.IsOpeningFile || controller.IsLoadingLastSession) return;

			string fileStr = string.Empty;
			if(controller.IsOpenedFile)
			{
				if (controller.HaveUnsavedChangesAtAll())
					fileStr += '*';
				fileStr += controller.OpenedFile + " | ";
			}
			Title = fileStr + "ActCreator";
		}

		private void UpdateFileContentVisibility()
		{
			FileContentVisibility =		  controller.IsOpenedFile ? Visibility.Visible : Visibility.Collapsed;
			CreateFileButtonVisibility = !controller.IsOpenedFile ? Visibility.Visible : Visibility.Collapsed;
		}

		private void CheckNeedSaveBeforeClosing(out DialogResult dialogResult)
		{
			dialogResult = System.Windows.Forms.DialogResult.None;
			if (controller.HaveUnsavedChangesAtAll())
			{
				dialogResult = MessageBox.Show("Файл має незбережені зміни. Зберегти файл перед закриттям?",
					"Закриття файлу",
					MessageBoxButtons.YesNoCancel,
					MessageBoxIcon.Question,
					MessageBoxDefaultButton.Button1);

				if (dialogResult == System.Windows.Forms.DialogResult.Yes)
				{
					saveFileDialog.FileName = controller.GetDmxFileName();
					dialogResult = saveFileDialog.ShowDialog();
					if (dialogResult == System.Windows.Forms.DialogResult.OK)
					{
						controller.ExportDmx(saveFileDialog.FileName);

						MessageBox.Show("Файл збережений.",
							"ActCreator | Export dcmk",
							MessageBoxButtons.OK,
							MessageBoxIcon.Information);
					}
				}
			}
		}
	}
}
