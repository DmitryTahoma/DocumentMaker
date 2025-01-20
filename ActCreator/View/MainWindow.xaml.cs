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
using System.Windows.Input;
using MessageBox = System.Windows.Forms.MessageBox;
using UpdaterAPI;
using UpdaterAPI.View;
using System;
using System.Threading.Tasks;
using SendException;
using ActCreator.Settings;

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
		private Updater updater = null;

		public MainWindow(string[] args)
		{
			controller = new MainWindowController(args);
			controller.Load();

			openFileDialog = new OpenFileDialog { Filter = "Файли акту (*" + BaseDmxFile.Extension + ")|*" + BaseDmxFile.Extension };

			InitializeComponent();

			DataFooter.SubscribeAddition((x) =>
			{
				HaveUnsavedChanges = true;
				controller.BackDataControllers.Add(x.Controller);
				x.SetViewByTemplate(controller.TemplateType);
				x.SetBackDataTypesList(controller.CurrentBackDataTypesList);
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

			updater = new Updater(ProgramSettings.DirectoryPath, (ConnectionType)controller.LastTypeConnection);
			updater.LoadLocalVersions();
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
			CheckNeedSaveBeforeClosing(out DialogResult res);
			if (res == System.Windows.Forms.DialogResult.Cancel)
			{
				e.Cancel = true;
				return;
			}

			if(WindowState != WindowState.Normal)
			{
				controller.WindowTop = RestoreBounds.Y;
				controller.WindowLeft = RestoreBounds.X;
				controller.WindowHeight = RestoreBounds.Height;
				controller.WindowWidth = RestoreBounds.Width;
			}
			else
			{
				controller.WindowTop = Top;
				controller.WindowLeft = Left;
				controller.WindowHeight = Height;
				controller.WindowWidth = Width;
			}

			controller.WindowState = WindowState == WindowState.Minimized ? WindowState.Normal : WindowState;

			controller.Save();
		}

		private async void WindowLoaded(object sender, RoutedEventArgs e)
		{
			try
			{
				SetWindowSettingsFromController();
				if (controller != null)
				{
					if (controller.HaveOpenLaterFiles)
					{
						OpenFile(controller.GetOpenLaterFile());
					}
					controller.IsLoadingLastSession = true;
					SetDataFromController();
					controller.IsLoadingLastSession = false;
					ResetHaveUnsavedChanges();
					UpdateTitle();
					UpdateFileContentVisibility();
				}

				ResetHaveUnsavedChanges();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}

			await SendExceptionProcessor.TrySendReportsAsync();

			try
			{
				if (Environment.GetCommandLineArgs().Length > 1 && Environment.GetCommandLineArgs()[1] == "/afterUpdate")
				{
					Version prevVersion = new Version("0.0.0.0");
					ChangeLog changeLog = new ChangeLog();
					changeLog.Initialize($"Обновление {updater.GetCurrentVersion()} успешно установлено! Приятной работы!", updater.GetChangeLog(prevVersion), MessageBoxButtons.OK);
					changeLog.ShowDialog();
				}
			}
			catch (Exception exception)
			{
				UpdateLog.WriteLine(exception.ToString(), "red");
			}

			try
			{
				const string updateLogStringColor = "#30bf1d";
				Version lastAvailableVersion = null;
				IReadOnlyDictionary<Version, string> changeLogDictionary = null;
				await Task.Run(() =>
				{
					UpdateLog.WriteLine("Удаление старых версий...", updateLogStringColor);
					updater.RemoveOldVersions();

					UpdateLog.WriteLine("Подключение...", updateLogStringColor);
					if (updater.TryConnect())
					{
						UpdateLog.WriteLine("Подключено", updateLogStringColor);
						controller.LastTypeConnection = (int)updater.GetLastTypeConnection();
						UpdateLog.WriteLine("Проверка есть ли обновление API...", updateLogStringColor);
						if (updater.HaveUpdateApi())
						{
							UpdateLog.WriteLine("Скачивание обновления API...", updateLogStringColor);
							updater.DownloadApiUpdates();
							UpdateLog.WriteLine("Обновление API скачано", updateLogStringColor);
							updater.UpdateUpdater();
							UpdateLog.WriteLine("Updater.exe обновлен", updateLogStringColor);
						}

						UpdateLog.WriteLine("Загрузка доступных версий на сервере...", updateLogStringColor);
						updater.LoadRemoteVersions();
						UpdateLog.WriteLine("Проверка есть ли новые версии для сервере...", updateLogStringColor);
						if (updater.HaveUpdateRemote())
						{
							UpdateLog.WriteLine("Скачивание обновления...", updateLogStringColor);
							updater.DownloadUpdates();
							UpdateLog.WriteLine("Обновление скачано", updateLogStringColor);
						}

						UpdateLog.WriteLine("Проверка есть ли скачанное обновление...", updateLogStringColor);
						if (updater.HaveUpdateLocal())
						{
							UpdateLog.WriteLine("Найдено обновление для установки", updateLogStringColor);
							UpdateLog.WriteLine("Определение версии для установки...", updateLogStringColor);
							lastAvailableVersion = updater.GetLastAvailableVersion();
							UpdateLog.WriteLine("Формирование словаря с изменениями...", updateLogStringColor);
							changeLogDictionary = updater.GetChangeLog(updater.GetCurrentVersion());
						}

						UpdateLog.WriteLine("Отключение...", updateLogStringColor);
						updater.Disconnect();
						UpdateLog.WriteLine("Отключено", updateLogStringColor);
					}
					else
					{
						UpdateLog.WriteLine("Не удалось подключиться", updateLogStringColor);
					}
				});

				if (lastAvailableVersion != null)
				{
					UpdateLog.WriteLine("Создание формы ChangeLog...", updateLogStringColor);
					ChangeLog changeLogForm = new ChangeLog();
					UpdateLog.WriteLine("Инициализация формы ChangeLog...", updateLogStringColor);
					changeLogForm.Initialize($"Доступно обновление {lastAvailableVersion}! Обновиться?", changeLogDictionary,MessageBoxButtons.YesNo);

					UpdateLog.WriteLine("Отображение формы ChangeLog...", updateLogStringColor);
					if (changeLogForm.ShowDialog() == System.Windows.Forms.DialogResult.Yes)
					{
						UpdateLog.WriteLine("Запуск Updater.exe...", updateLogStringColor);
						updater.Update(lastAvailableVersion);
					}
				}
			}
			catch (Exception exception)
			{
				UpdateLog.WriteLine(exception.ToString(), "red");
			}
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

		private void SaveBtnClick(object sender, RoutedEventArgs e)
		{
			OnSave();
		}

		private void SaveAsBtnClick(object sender, RoutedEventArgs e)
		{
			OnSaveAs();
		}

		private void OpenFileClick(object sender, RoutedEventArgs e)
		{
			OnOpenFile();
		}

		private void CloseFileClick(object sender, RoutedEventArgs e)
		{
			OnCloseFile();
		}

		private void AddWorkFileBtnClick(object sender, RoutedEventArgs e)
		{
			if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				if (controller.IsOpenedFile)
				{
					MainWindowController controllerAct = new MainWindowController(controller);

					if (!controllerAct.OpenFile(openFileDialog.FileName))
					{
						MessageBox.Show("Не вдалось відкрити файл:\n\n" + openFileDialog.FileName,
							"ActCreator | Open Files",
							MessageBoxButtons.OK,
							MessageBoxIcon.Error);
					}
					else
					{
						if (controllerAct.BackDataControllers.Count > 0)
						{
							controller.IsOpeningFile = true;
							controller.BackDataControllers.AddRange(controllerAct.BackDataControllers);
							controller.HaveUnsavedChanges = true;

							AddLoadedBackData();
							UpdateViewBackData();
							controller.IsOpeningFile = false;
						}
					}
				}
				else
				{
					OpenFile(openFileDialog.FileName);
				}
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
				if (filenames.Length > 0 && filenames[0] != null)
				{
					OpenFile(filenames[0]);
					e.Handled = true;
				}
			}
		}

		private void WindowDragEnter(object sender, System.Windows.DragEventArgs e)
		{
			bool isCorrect = true;

			if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop, true))
			{
				string[] filenames = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop, true);
				if (filenames.Length != 1 || !File.Exists(filenames[0]) || new FileInfo(filenames[0]).Extension != BaseDmxFile.Extension)
				{
					isCorrect = false;
				}
			}

			e.Effects = isCorrect ? System.Windows.DragDropEffects.All : System.Windows.DragDropEffects.None;
			e.Handled = true;
		}

		private void OnBackDataPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (!controller.IsOpeningFile && !controller.IsLoadingLastSession)
			{
				HaveUnsavedChanges = true;
			}
		}

		private void RemoveAllClick(object sender, RoutedEventArgs e)
		{
			OnRemoveAll();
		}

		private void WindowKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			bool control = Keyboard.Modifiers.HasFlag(ModifierKeys.Control);
			bool alt = Keyboard.Modifiers.HasFlag(ModifierKeys.Alt);

			if (control && Keyboard.IsKeyDown(Key.O))
			{
				OnOpenFile();
				e.Handled = true;
			}
			else if (control && Keyboard.IsKeyDown(Key.W))
			{
				OnCloseFile();
				e.Handled = true;
			}
			else if (control && alt && Keyboard.IsKeyDown(Key.S))
			{
				OnSaveAs();
				e.Handled = true;
			}
			else if (control && Keyboard.IsKeyDown(Key.S))
			{
				OnSave();
				e.Handled = true;
			}
			else if (alt && Keyboard.IsKeyDown(Key.Delete))
			{
				OnRemoveAll();
				e.Handled = true;
			}
		}

		private void UnfocusOnEnter(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				Keyboard.ClearFocus();
			}
		}

		private void ShowInstruction(object sender, RoutedEventArgs e)
		{
			Process.Start("https://docs.google.com/document/d/1zyoLFhsY6qsM4e9WHDbU-I-P7sIQVZ5czkKlLs9ef40/edit?usp=sharing");
		}

		private void ShowBackNames(object sender, RoutedEventArgs e)
		{
			Process.Start("https://docs.google.com/spreadsheets/d/1DkCd50ZGVRITZXW0g0yjM2vgH0XpvLCgPEWKmOZl0mU/edit#gid=367254280");
		}

		private void AboutUpdate(object sender, RoutedEventArgs e)
		{
			ChangeLog changeLog = new ChangeLog();
			System.Version version = updater.GetCurrentVersion();
			Dictionary<System.Version, string> versionInfo = new Dictionary<System.Version, string> { { version, updater.GetCurrentVersionText() } };
			changeLog.Initialize($"О версии программы {version}", versionInfo, MessageBoxButtons.OK);
			changeLog.ShowDialog();
		}

		private void UpdateViewBackData()
		{
			foreach (UIElement control in BacksData.Children)
			{
				if (control is ShortBackData backData)
				{
					backData.SetViewByTemplate(controller.TemplateType);
					backData.SetBackDataTypesList(controller.CurrentBackDataTypesList);
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
			WindowState = controller.WindowState;
		}

		private void SetDataFromController()
		{
			DocumentTemplateComboBox.SelectedIndex = (int)controller.TemplateType;
			HumanFullNameComboBox.Text = controller.SelectedHuman;
			if (HumanFullNameComboBox.SelectedItem == null)
			{
				HumanFullNameComboBox.Text = null;
			}
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
			CheckNeedSaveBeforeClosing(out DialogResult res);
			if (res == System.Windows.Forms.DialogResult.Cancel)
			{
				return;
			}

			if (!controller.OpenFile(filename))
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
			if (controller.IsOpenedFile)
			{
				if (controller.HaveUnsavedChangesAtAll())
					fileStr += '*';
				fileStr += controller.OpenedFile + " | ";
			}
			Title = fileStr + "Five-BN ActCreator " + updater.GetCurrentVersion();
		}

		private void UpdateFileContentVisibility()
		{
			FileContentVisibility = controller.IsOpenedFile ? Visibility.Visible : Visibility.Collapsed;
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
						if (controller.Validate(out string errorText))
						{
							if (controller.HaveWarnings(out string warningText))
							{
								if (MessageBox.Show(warningText, "ActCreator | Validation", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == System.Windows.Forms.DialogResult.Cancel)
								{
									dialogResult = System.Windows.Forms.DialogResult.Cancel;
									return;
								}
							}

							controller.ExportDmx(saveFileDialog.FileName);

							MessageBox.Show("Файл збережений.",
								"ActCreator | Export dcmk",
								MessageBoxButtons.OK,
								MessageBoxIcon.Information);
						}
						else
						{
							dialogResult = System.Windows.Forms.DialogResult.Cancel;
							MessageBox.Show("Неможливо зберегти файл!\n" + errorText, "ActCreator | Validation", MessageBoxButtons.OK, MessageBoxIcon.Error);
						}
					}
				}
			}
		}

		private void OnOpenFile()
		{
			if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				OpenFile(openFileDialog.FileName);
			}
		}

		private void OnCloseFile()
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

		private void OnSaveAs()
		{
			if (controller.Validate(out string errorText))
			{
				if (controller.HaveWarnings(out string warningText))
				{
					if (MessageBox.Show(warningText, "ActCreator | Validation", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == System.Windows.Forms.DialogResult.Cancel)
					{
						return;
					}
				}

				saveFileDialog.FileName = controller.GetDmxFileName();
				if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					controller.ExportDmx(saveFileDialog.FileName);
					ResetHaveUnsavedChanges();
					UpdateTitle();

					if (MessageBox.Show("Файл збережений.\nВідкрити папку з файлом?",
										"ActCreator | Export",
										MessageBoxButtons.YesNo,
										MessageBoxIcon.Information,
										MessageBoxDefaultButton.Button2)
						== System.Windows.Forms.DialogResult.Yes)
					{
						Process.Start("explorer", "/n, /select, " + saveFileDialog.FileName);
					}
				}
			}
			else
			{
				MessageBox.Show(errorText, "ActCreator | Validation", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void OnSave()
		{
			if (controller.IsNewFile)
			{
				OnSaveAs();
				return;
			}

			if (controller.Validate(out string errorText))
			{
				if (controller.HaveWarnings(out string warningText))
				{
					if (MessageBox.Show(warningText, "ActCreator | Validation", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == System.Windows.Forms.DialogResult.Cancel)
					{
						return;
					}
				}

				controller.ExportDmx(controller.OpenedFile);
				ResetHaveUnsavedChanges();
				UpdateTitle();
			}
			else
			{
				MessageBox.Show(errorText, "ActCreator | Validation", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void OnRemoveAll()
		{
			DataFooter.ClearBackData();
			HaveUnsavedChanges = true;
		}
	}
}
