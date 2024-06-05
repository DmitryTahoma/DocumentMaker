using Dml.Controller.Validation;
using Dml.Model.Files;
using Dml.Model.Template;
using Dml.UndoRedo;
using DocumentMaker.Controller;
using DocumentMaker.Controller.Controls;
using DocumentMakerModelLibrary;
using DocumentMakerModelLibrary.Back;
using DocumentMakerModelLibrary.Files;
using DocumentMakerModelLibrary.OfficeFiles.Human;
using DocumentMaker.View.Controls;
using DocumentMaker.View.Dialogs;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Markup;
using MessageBox = System.Windows.Forms.MessageBox;
using UpdaterAPI;
using UpdaterAPI.View;
using System.Threading.Tasks;

namespace DocumentMaker
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		#region Dependency Properties

		public static readonly DependencyProperty TechnicalTaskDateTextProperty;
		public static readonly DependencyProperty ActDateTextProperty;
		public static readonly DependencyProperty TechnicalTaskNumTextProperty;
		public static readonly DependencyProperty ActSumProperty;
		public static readonly DependencyProperty ActSumSelectedProperty;
		public static readonly DependencyProperty ActSaldoProperty;
		public static readonly DependencyProperty ContentVisibilityProperty;
		public static readonly DependencyProperty ButtonOpenContentVisibilityProperty;
		public static readonly DependencyProperty CanRedoProperty;
		public static readonly DependencyProperty CanUndoProperty;

		static MainWindow()
		{
			TechnicalTaskDateTextProperty = DependencyProperty.Register("TechnicalTaskDateText", typeof(string), typeof(MainWindowController));
			ActDateTextProperty = DependencyProperty.Register("ActDateText", typeof(string), typeof(MainWindowController));
			TechnicalTaskNumTextProperty = DependencyProperty.Register("TechnicalTaskNumText", typeof(string), typeof(MainWindowController));
			ActSumProperty = DependencyProperty.Register("ActSum", typeof(string), typeof(MainWindowController));
			ActSumSelectedProperty = DependencyProperty.Register("ActSumSelected", typeof(string), typeof(MainWindowController));
			ActSaldoProperty = DependencyProperty.Register("ActSaldo", typeof(string), typeof(MainWindowController));
			ContentVisibilityProperty = DependencyProperty.Register("ContentVisibility", typeof(Visibility), typeof(MainWindow));
			ButtonOpenContentVisibilityProperty = DependencyProperty.Register("ButtonOpenContentVisibility", typeof(Visibility), typeof(MainWindow));
			CanRedoProperty = DependencyProperty.Register("CanRedo", typeof(bool), typeof(MainWindow));
			CanUndoProperty = DependencyProperty.Register("CanUndo", typeof(bool), typeof(MainWindow));
		}

		#endregion

		private readonly MainWindowController controller;
		private readonly FolderBrowserDialog folderBrowserDialog;
		private readonly OpenFileDialog openFileDialog;
		private readonly SaveFileDialog saveFileDialog;
		private readonly InputingValidator inputingValidator;

		private bool cancelOpenedFilesSelectionChanged;
		private Updater updater = new Updater();
		private readonly long telegramGroupId = -1002081343445;

		public MainWindow(string[] args)
		{
			controller = new MainWindowController(args);
			controller.Load();

			folderBrowserDialog = new FolderBrowserDialog();
			openFileDialog = new OpenFileDialog
			{
				Multiselect = true,
				Filter = "Всі файли акту (*" + BaseDmxFile.Extension + ";*" + DcmkFile.Extension + ")|*" + BaseDmxFile.Extension + ";*" + DcmkFile.Extension
					+ "|Файли акту (*" + BaseDmxFile.Extension + ")|*" + BaseDmxFile.Extension
					+ "|Файли повного акту (*" + DcmkFile.Extension + ")|*" + DcmkFile.Extension
			};
			saveFileDialog = new SaveFileDialog
			{
				DefaultExt = DcmkFile.Extension,
				Filter = "Файл повного акту (*" + DcmkFile.Extension + ")|*" + DcmkFile.Extension
			};

			inputingValidator = new InputingValidator();

			ContentVisibility = Visibility.Hidden;
			ButtonOpenContentVisibility = Visibility.Visible;

			InitializeComponent();
			InitializeComponentFromCode();
			updater.LoadLocalVersions();
			Title = $"Five-BN DocumentMaker {updater.GetCurrentVersion()}";

			System.Windows.Application.Current.DispatcherUnhandledException += (s, e1) =>
			{
				SendExceptionTG(e1.Exception);
				e1.Handled = true;
			};

			AppDomain.CurrentDomain.UnhandledException += (s, e1) =>
			{
				Exception exc = e1.ExceptionObject as Exception;
				if (exc != null)
				{
					SendExceptionTG(exc);
				}
			};
		}

		private void InitializeComponentFromCode()
		{
			TechnicalTaskDatePicker.Language = XmlLanguage.GetLanguage(System.Globalization.CultureInfo.CurrentCulture.IetfLanguageTag);
			ActDatePicker.Language = XmlLanguage.GetLanguage(System.Globalization.CultureInfo.CurrentCulture.IetfLanguageTag);

			DataHeader.SubscribeSelectionChanged((b) => OnSelectionChanged(b, false, false));
			DataFooter.SubscribeAddition((x) => OnAdded(x, false, false));
			DataFooter.SubscribeChangingSum(OnSumChanged);
			DataFooter.ActionsStack = controller.GetActionsStack();

			ReworkDataHeader.SubscribeSelectionChanged((b) => OnSelectionChanged(b, true, false));
			ReworkDataFooter.SubscribeAddition((x) => OnAdded(x, true, false));
			ReworkDataFooter.SubscribeChangingSum(OnSumChanged);
			ReworkDataFooter.ActionsStack = controller.GetActionsStack();

			OtherDataHeader.HideWorkTypeLabel();
			OtherDataHeader.SubscribeSelectionChanged((b) => OnSelectionChanged(b, false, true));
			OtherDataFooter.SubscribeAddition((x) => OnAdded(x, false, true));
			OtherDataFooter.SubscribeChangingSum(OnSumChanged);
			OtherDataFooter.ActionsStack = controller.GetActionsStack();

			ActSumInput.CommandBindings.Add(new CommandBinding(ApplicationCommands.Paste, inputingValidator.BlockingCommand));
		}

		#region Properties

		public IList<DmxFile> OpenedFilesList => controller.OpenedFilesList;

		public IList<FullDocumentTemplate> DocumentTemplatesList => controller.DocumentTemplatesList;

		public IList<string> ContractFilesList => controller.ContractFilesList;

		public string TechnicalTaskDateText
		{
			get => (string)GetValue(TechnicalTaskDateTextProperty);
			set
			{
				SetValue(TechnicalTaskDateTextProperty, value);
				controller.TechnicalTaskDateText = value;

				if (OpenedFilesComboBox != null && OpenedFilesComboBox.SelectedItem is DmxFile selectedFile)
				{
					selectedFile.TechnicalTaskDateText = value;
				}
			}
		}

		public string ActDateText
		{
			get => (string)GetValue(ActDateTextProperty);
			set
			{
				SetValue(ActDateTextProperty, value);
				controller.ActDateText = value;

				if (OpenedFilesComboBox != null && OpenedFilesComboBox.SelectedItem is DmxFile selectedFile)
				{
					selectedFile.ActDateText = value;
				}
			}
		}

		public string TechnicalTaskNumText
		{
			get => (string)GetValue(TechnicalTaskNumTextProperty);
			set
			{
				SetValue(TechnicalTaskNumTextProperty, value);
				controller.TechnicalTaskNumText = value;

				if (OpenedFilesComboBox != null && OpenedFilesComboBox.SelectedItem is DmxFile selectedFile)
				{
					selectedFile.TechnicalTaskNumText = value;
				}
			}
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


		public string ActSumSelected
		{
			get => (string)GetValue(ActSumSelectedProperty);
			set
			{
				SetValue(ActSumSelectedProperty, value);
				//controller.ActSum = value;
			}
		}

		public string ActSaldo
		{
			get => (string)GetValue(ActSaldoProperty);
			set => SetValue(ActSaldoProperty, value);
		}

		public double IconSize { get; } = 24;

		public Visibility ContentVisibility { get => (Visibility)GetValue(ContentVisibilityProperty); set => SetValue(ContentVisibilityProperty, value); }

		public Visibility ButtonOpenContentVisibility { get => (Visibility)GetValue(ButtonOpenContentVisibilityProperty); set => SetValue(ButtonOpenContentVisibilityProperty, value); }

		private bool CanUndoNeedUpdateSum { get; set; } = true;

		public bool CanRedo
		{
			get => (bool)GetValue(CanRedoProperty);
			set => SetValue(CanRedoProperty, value);
		}

		public bool CanUndo
		{
			get => (bool)GetValue(CanUndoProperty);
			set => SetValue(CanUndoProperty, value);
		}

		public bool HaveUnsavedChanges { get => controller.HaveUnsavedChanges; set => controller.HaveUnsavedChanges = value; }

		#endregion

		#region Event handlers

		private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			SetDataToController();
			if (WindowState != WindowState.Normal)
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
				CheckFiles();
				SetWindowSettingsFromController();

				if (controller != null)
				{
					SetDataFromController();
					string[] files = controller.GetOpenLaterFiles();
					OpenFiles(files);
					ResetHaveUnsavedChanges();
					LoadFiles();
					if (files != null && files.Length > 0)
					{
						SetSelectedFile(files.Last());
					}
					controller.ChangeOpenedFilesExtension();
					UpdateActSum();
				}

				controller.EnableActionsStacking();
				controller.SubscribeActionPushed((action) => { UpdateUndoRedoState(); });
				ResetHaveUnsavedChanges();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}

			try
			{
				if (Environment.GetCommandLineArgs().Length > 2 && Environment.GetCommandLineArgs()[1] == "/afterUpdate")
				{
					Version prevVersion = Version.Parse(Environment.GetCommandLineArgs()[2]);
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
					changeLogForm.Initialize($"Доступно обновление {lastAvailableVersion}! Обновиться?", changeLogDictionary, MessageBoxButtons.YesNo);

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

				if (OpenedFilesComboBox != null
					&& OpenedFilesComboBox.SelectedItem is DmxFile selectedFile)
				{
					selectedFile.TemplateType = documentTemplate.Type;
				}
			}
		}

		private void ChangedContractFiles(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			if (controller != null && sender is System.Windows.Controls.ComboBox comboBox
				&& comboBox.SelectedItem != null && !string.IsNullOrEmpty(comboBox.SelectedItem.ToString()))
			{
				controller.LoadHumans(comboBox.SelectedItem.ToString());
				controller.SelectedContractFile = comboBox.SelectedItem.ToString();

				if (OpenedFilesComboBox != null
					&& OpenedFilesComboBox.SelectedItem is DmxFile selectedFile)
				{
					selectedFile.SelectedContractFile = comboBox.SelectedItem.ToString();
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
			try
			{
				if (!(OpenedFilesComboBox.SelectedItem is DmxFile selectedFile && selectedFile.Loaded))
				{
					MessageBox.Show("Спочатку необхідно відкрити файл.",
									"DocumentMaker | Експорт актів",
									MessageBoxButtons.OK,
									MessageBoxIcon.Information);
					return;
				}

				SetDataToController();
				controller.TrimAllStrings();
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
									isShowResult = false;
								}
							}
							else
							{
								isShowResult = false;
							}
						}

						controller.RemoveTemplates();
						if (isShowResult)
						{
							MessageBox.Show("Файли збережені.",
											"DocumentMaker | Export",
											MessageBoxButtons.OK,
											MessageBoxIcon.Information);
						}
					}
				}
				else
				{
					MessageBox.Show(errorText, "DocumentMaker | Validation", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
			catch (Exception exc)
			{
				MessageBox.Show("Виникла непередбачена помилка під час експорту! Надішліть, будь ласка, скріншот помилки розробнику.\n" + exc.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void ExportAllBtnClick(object sender, RoutedEventArgs e)
		{
			try
			{
				if (!(OpenedFilesComboBox.SelectedItem is DmxFile selectedFile && selectedFile.Loaded))
				{
					MessageBox.Show("Спочатку необхідно відкрити файл.",
									"DocumentMaker | Експорт актів",
									MessageBoxButtons.OK,
									MessageBoxIcon.Information);
					return;
				}

				if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					bool isShowResult = false;
					foreach (DmxFile file in OpenedFilesList)
					{
						SetSelectedFile(file.FullName);
						SetDataToController();
						controller.TrimAllStrings();

						if (!controller.Validate(out string errorText))
						{
							if (MessageBox.Show(errorText,
								"DocumentMaker | Validation | " + file.Name,
								MessageBoxButtons.OKCancel,
								MessageBoxIcon.Error,
								MessageBoxDefaultButton.Button1)
									== System.Windows.Forms.DialogResult.OK)
							{
								continue;
							}
							else
							{
								break;
							}
						}

						controller.Export(folderBrowserDialog.SelectedPath);
						isShowResult = true;
					}

					if (controller.HasNoMovedFiles)
					{
						string startInfoNoMoved = controller.GetInfoNoMovedFiles();
						if (MessageBox.Show("Файли за заданними путями вже існують.\n\n" + startInfoNoMoved + "\nЗамінити?",
											"DocumentMaker | Export",
											MessageBoxButtons.YesNo,
											MessageBoxIcon.Question)
												== System.Windows.Forms.DialogResult.Yes)
						{
							controller.ReplaceCreatedFiles();

							if (controller.HasNoMovedFiles)
							{
								string infoNoMoved = controller.GetInfoNoMovedFiles();
								MessageBox.Show("Не вдалось перемістити наступні файли. Можливо вони відкриті в іншій програмі.\n\n" + infoNoMoved,
												"DocumentMaker | Export",
												MessageBoxButtons.OK,
												MessageBoxIcon.Warning);

								isShowResult = startInfoNoMoved != infoNoMoved;
							}
						}
						else
						{
							isShowResult = false;
						}
					}

					controller.RemoveTemplates();
					if (isShowResult)
					{
						MessageBox.Show("Файли збережені.",
										"DocumentMaker | Export",
										MessageBoxButtons.OK,
										MessageBoxIcon.Information);
					}
				}
			}
			catch (Exception exc)
			{
				MessageBox.Show("Виникла непередбачена помилка під час експорту! Надішліть, будь ласка, скріншот помилки розробнику.\n" + exc.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private async void ExportGameSum(object sender, RoutedEventArgs e)
		{
			try
			{
				if (!(OpenedFilesComboBox.SelectedItem is DmxFile selectedFile && selectedFile.Loaded))
				{
					MessageBox.Show("Спочатку необхідно відкрити файл.",
									"DocumentMaker | Експорт сум iгор",
									MessageBoxButtons.OK,
									MessageBoxIcon.Information);
					return;
				}


				if (selectedFile != null)
				{
					CreateGamePriceDialog dialog = new CreateGamePriceDialog(OpenedFilesList, controller.GameNameList, controller.ActDateText);

					await DialogHost.Show(dialog);
				}
			}
			catch (Exception exc)
			{
				MessageBox.Show("Виникла непередбачена помилка під час експорту! Надішліть, будь ласка, скріншот помилки розробнику.\n" + exc.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void OpenFileClick(object sender, RoutedEventArgs e)
		{
			if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				OpenFiles(openFileDialog.FileNames);
				LoadFiles();
				SetSelectedFile(openFileDialog.FileNames.Last());
				controller.ChangeOpenedFilesExtension();
			}
		}

		private void CloseFileClick(object sender, RoutedEventArgs e)
		{
			if (OpenedFilesComboBox.SelectedItem is DmxFile selectedFile && selectedFile.Loaded)
			{
				CheckNeedSaveBeforeClosing(out DialogResult res);
				if (res == System.Windows.Forms.DialogResult.Cancel)
				{
					return;
				}

				int index = OpenedFilesComboBox.SelectedIndex;
				if (index != -1)
				{
					ResetHaveUnsavedChanges();
					controller.CloseFile(selectedFile);
					int newIndex = OpenedFilesComboBox.Items.Count <= index ? index - 1 : index;
					if (newIndex < 0)
					{
						controller.ClearUndoRedo();
						UpdateUndoRedoState();
					}
					ResetHaveUnsavedChanges();
					OpenedFilesComboBox.SelectedIndex = newIndex;
				}
			}
			else
				MessageBox.Show("Спочатку необхідно відкрити файл.",
								"DocumentMaker | Закриття файлу",
								MessageBoxButtons.OK,
								MessageBoxIcon.Information);
		}

		private void CloseAllFilesClick(object sender, RoutedEventArgs e)
		{
			if (OpenedFilesList.Count > 0)
			{
				CheckNeedSaveBeforeClosing(out DialogResult res);
				if (res == System.Windows.Forms.DialogResult.Cancel)
				{
					return;
				}

				while (OpenedFilesList.Count > 0)
				{
					ResetHaveUnsavedChanges();
					controller.CloseFile(OpenedFilesList[0]);
				}
				OpenedFilesComboBox.SelectedIndex = -1;
				controller.ClearUndoRedo();
				UpdateUndoRedoState();
			}
			else
				MessageBox.Show("Спочатку необхідно відкрити файл.",
								"DocumentMaker | Закриття файлу",
								MessageBoxButtons.OK,
								MessageBoxIcon.Information);
		}

		private void ShowInstruction(object sender, RoutedEventArgs e)
		{
			System.Diagnostics.Process.Start("https://docs.google.com/document/d/1zyoLFhsY6qsM4e9WHDbU-I-P7sIQVZ5czkKlLs9ef40/edit?usp=sharing");
		}

		private void ShowBackNames(object sender, RoutedEventArgs e)
		{
			System.Diagnostics.Process.Start("https://docs.google.com/spreadsheets/d/1DkCd50ZGVRITZXW0g0yjM2vgH0XpvLCgPEWKmOZl0mU/edit#gid=367254280");
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
			bool isNeedUpdateSum = controller.NeedUpdateSum;
			DisableUpdatingSum();
			SetDataToController();

			List<FullBackData> selectedBackDatas = new List<FullBackData>(GetSelectedBackDatas());
			IEnumerable<int> resultSums = controller.CorrectSaldo(selectedBackDatas.Select(x => x.Controller));

			IEnumerator<FullBackData> selectedBackDatasEnum = selectedBackDatas.GetEnumerator();
			IEnumerator<int> resultSumsEnum = resultSums.GetEnumerator();
			bool pushedFirst = false;
			while (selectedBackDatasEnum.MoveNext() && resultSumsEnum.MoveNext())
			{
				if (selectedBackDatasEnum.Current.Controller.SumText == resultSumsEnum.Current.ToString())
					continue;

				if (pushedFirst)
				{
					selectedBackDatasEnum.Current.SetSumTextChangesWithLink(resultSumsEnum.Current.ToString());
				}
				selectedBackDatasEnum.Current.SumTextInput.Text = resultSumsEnum.Current.ToString();
				if (!pushedFirst)
				{
					pushedFirst = true;
					controller.DisableActionsStacking();
					AddUndoRedoLinkNeedUpdateSum(isNeedUpdateSum);
				}
			}
			controller.EnableActionsStacking();

			if (pushedFirst)
			{
				HaveUnsavedChanges = true;
			}
			else if (isNeedUpdateSum)
			{
				EnableUpdatingSum();
			}

			if (DataHeader != null) DataHeader.IsChecked = false;
			if (ReworkDataHeader != null) ReworkDataHeader.IsChecked = false;
			if (OtherDataHeader != null) OtherDataHeader.IsChecked = false;

			SetDataFromControllerBackDatas();
			DataFooter?.UpdateAllSum();
			ReworkDataFooter?.UpdateAllSum();
			OtherDataFooter?.UpdateAllSum();
		}

		private async void CorrectDevelopClick(object sender, RoutedEventArgs e)
		{
			CorrectDevelopmentDialog dialog = new CorrectDevelopmentDialog
			{
				NumberText = controller.CorrectDevelopmentWindow_NumberText,
				TakeSumFromSupport = controller.CorrectDevelopmentWindow_TakeSumFromSupport,
				IsRemoveIdenticalNumbers = controller.CorrectDevelopmentDialog_IsRemoveIdenticalNumbers,
			};
			await DialogHost.Show(dialog);

			controller.CorrectDevelopmentWindow_NumberText = dialog.NumberText;
			controller.CorrectDevelopmentWindow_TakeSumFromSupport = dialog.TakeSumFromSupport;
			controller.CorrectDevelopmentDialog_IsRemoveIdenticalNumbers = dialog.IsRemoveIdenticalNumbers;

			if (dialog.IsCorrection && int.TryParse(dialog.NumberText, out int sum))
			{
				bool isNeedUpdateSum = controller.NeedUpdateSum;
				DisableUpdatingSum();
				IEnumerable<int> resultSums = controller.CorrectDevelopment(sum, dialog.TakeSumFromSupport, dialog.IsRemoveIdenticalNumbers);
				IEnumerator<FullBackDataController> backDataControllersEnum = controller.BackDataControllers.GetEnumerator();
				IEnumerator<int> resultSumsEnum = resultSums.GetEnumerator();
				List<FullBackData> allFullBackDatas = new List<FullBackData>(GetAllFullBacksData());
				bool pushedFirst = false;
				controller.DisableActionsStacking();
				while (backDataControllersEnum.MoveNext() && resultSumsEnum.MoveNext())
				{
					FullBackData current = allFullBackDatas.FirstOrDefault(x => x.Controller == backDataControllersEnum.Current);
					if (current != null && current.SumTextInput.Text != resultSumsEnum.Current.ToString())
					{
						if (pushedFirst)
						{
							current.SetSumTextChangesWithLink(resultSumsEnum.Current.ToString());
						}
						else
						{
							pushedFirst = true;
							current.SetSumTextChangesWithAction(resultSumsEnum.Current.ToString());
							controller.AddUndoRedoLink(new UndoRedoLink(() =>
							{
								current.SumTextInput.Text = current.Controller.SumText;
							}));
							AddUndoRedoLinkNeedUpdateSum(isNeedUpdateSum);
						}
						current.SumTextInput.Text = resultSumsEnum.Current.ToString();
					}
				}
				controller.EnableActionsStacking();

				if (pushedFirst)
				{
					HaveUnsavedChanges = true;
				}
				else if (isNeedUpdateSum)
				{
					EnableUpdatingSum();
				}

				if (DataHeader != null) DataHeader.IsChecked = false;

				SetDataFromControllerBackDatas();
			}
		}

		private async void CorrectSupportClick(object sender, RoutedEventArgs e)
		{
			CorrectSupportDialog dialog = new CorrectSupportDialog
			{
				NumberText = controller.CorrectSupportWindow_NumberText,
				TakeSumFromDevelopment = controller.CorrectSupportWindow_TakeSumFromDevelopment,
				IsCreateNewWorks = controller.CorrectSupportDialog_IsCreateNewWorks,
				IsRemoveIdenticalNumbers = controller.CorrectSupportDialog_IsRemoveIdenticalNumbers,
			};
			await DialogHost.Show(dialog);

			controller.CorrectSupportWindow_NumberText = dialog.NumberText;
			controller.CorrectSupportWindow_TakeSumFromDevelopment = dialog.TakeSumFromDevelopment;
			controller.CorrectSupportDialog_IsCreateNewWorks = dialog.IsCreateNewWorks;
			controller.CorrectSupportDialog_IsRemoveIdenticalNumbers = dialog.IsRemoveIdenticalNumbers;

			if (dialog.IsCorrection && int.TryParse(dialog.NumberText, out int sum))
			{
				bool isNeedUpdateSum = controller.NeedUpdateSum;
				DisableUpdatingSum();
				IEnumerable<int> resultSums = controller.CorrectSupport(sum, dialog.TakeSumFromDevelopment, dialog.IsCreateNewWorks, dialog.IsRemoveIdenticalNumbers, out List<KeyValuePair<FullBackDataController, int>> newControllers);
				IEnumerator<FullBackDataController> backDataControllersEnum = controller.BackDataControllers.GetEnumerator();
				IEnumerator<int> resultSumsEnum = resultSums.GetEnumerator();
				List<FullBackData> allFullBackDatas = new List<FullBackData>(GetAllFullBacksData());
				bool pushedFirst = false;
				controller.DisableActionsStacking();
				while (backDataControllersEnum.MoveNext() && resultSumsEnum.MoveNext())
				{
					FullBackData current = allFullBackDatas.FirstOrDefault(x => x.Controller == backDataControllersEnum.Current);
					if (current != null && current.SumTextInput.Text != resultSumsEnum.Current.ToString())
					{
						if (pushedFirst)
						{
							current.SetSumTextChangesWithLink(resultSumsEnum.Current.ToString());
						}
						else
						{
							pushedFirst = true;
							current.SetSumTextChangesWithAction(resultSumsEnum.Current.ToString());
							controller.AddUndoRedoLink(new UndoRedoLink(() =>
							{
								current.SumTextInput.Text = current.Controller.SumText;
							}));
							AddUndoRedoLinkNeedUpdateSum(isNeedUpdateSum);
						}
						current.SumTextInput.Text = resultSumsEnum.Current.ToString();
					}
				}
				if (newControllers.Count > 0)
				{
					IEnumerable<FullBackData> added = AddNewSupport(newControllers.Select(x => x.Key));
					IEnumerator<KeyValuePair<FullBackDataController, int>> newControllersEnum = newControllers.GetEnumerator();
					IEnumerator<FullBackData> addedEnum = added.GetEnumerator();
					while (addedEnum.MoveNext() && newControllersEnum.MoveNext())
					{
						addedEnum.Current.SetSumTextChangesWithLink(newControllersEnum.Current.Value.ToString());
					}
					UpdateViewBackData();
				}
				controller.EnableActionsStacking();

				if (pushedFirst)
				{
					HaveUnsavedChanges = true;
				}
				else if (isNeedUpdateSum)
				{
					EnableUpdatingSum();
				}

				if (ReworkDataHeader != null) ReworkDataHeader.IsChecked = false;

				SetDataFromControllerBackDatas();
			}
		}

		public void DeleteSelectedDevelopment(object sender, RoutedEventArgs e)
		{
			if ((!DataHeader.IsChecked.HasValue || DataHeader.IsChecked.Value) &&
				MessageBox.Show("Ви впевнені, що хочете видалити обрані елементи?",
				"DocumentMaker | Видалення | Розробка",
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Question,
				MessageBoxDefaultButton.Button1)
					== System.Windows.Forms.DialogResult.Yes)
			{
				HaveUnsavedChanges = true;
				DisableUpdatingSum();
				CanUndoNeedUpdateSum = false;
				IEnumerable<FullBackData> removedElems = DeleteSelectedBackData(BacksData);
				controller.RemoveFromActionsStack(removedElems.Select(x => x.Controller));
				DataHeader.UpdateIsCheckedState();
				DataFooter.UpdateBackDataIds();
				DataFooter.UpdateAllSum();
			}
		}

		public void DeleteSelectedSupport(object sender, RoutedEventArgs e)
		{
			if ((!ReworkDataHeader.IsChecked.HasValue || ReworkDataHeader.IsChecked.Value) &&
				MessageBox.Show("Ви впевнені, що хочете видалити обрані елементи?",
				"DocumentMaker | Видалення | Підтримка",
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Question,
				MessageBoxDefaultButton.Button1)
					== System.Windows.Forms.DialogResult.Yes)
			{
				HaveUnsavedChanges = true;
				DisableUpdatingSum();
				CanUndoNeedUpdateSum = false;
				IEnumerable<FullBackData> removedElems = DeleteSelectedBackData(ReworkBacksData);
				controller.RemoveFromActionsStack(removedElems.Select(x => x.Controller));
				ReworkDataHeader.UpdateIsCheckedState();
				ReworkDataFooter.UpdateBackDataIds();
				ReworkDataFooter.UpdateAllSum();
			}
		}

		public void DeleteSelectedOther(object sender, RoutedEventArgs e)
		{
			if ((!OtherDataHeader.IsChecked.HasValue || OtherDataHeader.IsChecked.Value) &&
				MessageBox.Show("Ви впевнені, що хочете видалити обрані елементи?",
				"DocumentMaker | Видалення | Інше",
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Question,
				MessageBoxDefaultButton.Button1)
					== System.Windows.Forms.DialogResult.Yes)
			{
				HaveUnsavedChanges = true;
				DisableUpdatingSum();
				CanUndoNeedUpdateSum = false;
				IEnumerable<FullBackData> removedElems = DeleteSelectedBackData(OtherBacksData);
				controller.RemoveFromActionsStack(removedElems.Select(x => x.Controller));
				OtherDataHeader.UpdateIsCheckedState();
				OtherDataFooter.UpdateBackDataIds();
				OtherDataFooter.UpdateAllSum();
			}
		}

		public void MoveFromDevelopment(object sender, RoutedEventArgs e)
		{
			MoveBackData(DataHeader, BacksData, DataFooter, ReworkDataHeader, ReworkDataFooter);
			HaveUnsavedChanges = true;
		}

		public void MoveFromSupport(object sender, RoutedEventArgs e)
		{
			MoveBackData(ReworkDataHeader, ReworkBacksData, ReworkDataFooter, DataHeader, DataFooter);
			HaveUnsavedChanges = true;
		}

		public void MoveFromOtherToDevelopment(object sender, RoutedEventArgs e)
		{
			MoveBackData(OtherDataHeader, OtherBacksData, OtherDataFooter, DataHeader, DataFooter);
			HaveUnsavedChanges = true;
		}

		public void MoveFromOtherToSupport(object sender, RoutedEventArgs e)
		{
			MoveBackData(OtherDataHeader, OtherBacksData, OtherDataFooter, ReworkDataHeader, ReworkDataFooter);
			HaveUnsavedChanges = true;
		}

		private void WindowPreviewDrop(object sender, System.Windows.DragEventArgs e)
		{
			if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop, true))
			{
				string[] filenames = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop, true);
				OpenFiles(filenames);
				LoadFiles();
				SetSelectedFile(filenames.Last(filename => filename.EndsWith(BaseDmxFile.Extension) || filename.EndsWith(DcmkFile.Extension)));
				controller.ChangeOpenedFilesExtension();
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
					if (info.Extension != BaseDmxFile.Extension
						&& info.Extension != DcmkFile.Extension)
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

		private void OpenedFilesSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!IsInitialized)
			{
				Initialized += FileChangedAction;
			}
			else
			{
				FileChangedAction(sender, e);
			}
		}

		private void FileChangedAction(object sender, EventArgs e)
		{
			FileChangedAction();
			Initialized -= FileChangedAction;
		}

		private void FileChangedAction()
		{
			if (cancelOpenedFilesSelectionChanged)
			{
				cancelOpenedFilesSelectionChanged = false;
				return;
			}

			if (OpenedFilesComboBox != null && OpenedFilesComboBox.SelectedItem is DmxFile selectedFile && selectedFile.Loaded)
			{
				if (HaveUnsavedChangesAtAll())
				{
					DialogResult res = MessageBox.Show("Відкритий файл має незбережені зміни. Зберегти файл перед змінною?",
						"Зміна файлу",
						MessageBoxButtons.YesNoCancel,
						MessageBoxIcon.Question,
						MessageBoxDefaultButton.Button1);

					if (res == System.Windows.Forms.DialogResult.Yes)
					{
						if (string.IsNullOrEmpty(selectedFile.Name))
							saveFileDialog.FileName = controller.GetDcmkFileName();
						else
							saveFileDialog.FileName = controller.GetSelectedFile().Name;

						res = saveFileDialog.ShowDialog();
						if (res == System.Windows.Forms.DialogResult.OK)
						{
							controller.ExportDcmk(saveFileDialog.FileName);

							MessageBox.Show("Файл збережений.",
								"DocumentMaker | Export dcmk",
								MessageBoxButtons.OK,
								MessageBoxIcon.Information);
						}
					}

					if (res == System.Windows.Forms.DialogResult.Cancel)
					{
						cancelOpenedFilesSelectionChanged = true;
						OpenedFilesComboBox.SelectedItem = controller.GetSelectedFile();
						return;
					}
				}

				controller.ClearUndoRedo();
				UpdateUndoRedoState();

				ContentVisibility = Visibility.Visible;
				ButtonOpenContentVisibility = Visibility.Hidden;

				SetDataToController();
				controller.SetDataFromFile(selectedFile);
				AddLoadedBackData();
				DataFooter?.UpdateAllSum();
				ReworkDataFooter?.UpdateAllSum();
				OtherDataFooter?.UpdateAllSum();
				SetDataFromController();
				controller.SetSelectedFile(selectedFile);
				UpdateViewBackData();
				UpdateSaldo();

				ResetHaveUnsavedChanges();
			}
			else
			{
				ContentVisibility = Visibility.Hidden;
				ButtonOpenContentVisibility = Visibility.Visible;
			}
		}

		private void ActSumTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
		{
			HaveUnsavedChanges = true;
			if (controller.IsActionsStackingEnabled && sender is System.Windows.Controls.TextBox textBox)
			{
				controller.AddUndoRedoLink(new UndoRedoLink(() =>
				{
					textBox.Text = controller.ActSum;
					textBox.Focus();
					textBox.SelectionStart = textBox.Text.Length;
					textBox.SelectionLength = 0;
				}));
			}
			UpdateActSum();
		}

		private void UIntValidating(object sender, System.Windows.Input.TextCompositionEventArgs e)
		{
			inputingValidator.UIntInputing_PreviewTextInput(sender, e);
		}

		private void RandomizeWorkTypes(object sender, RoutedEventArgs e)
		{
			HaveUnsavedChanges = true;
			controller.TrimAllStrings();
			controller.RandomizeWorkTypes(GetSelectedBackDatas(BacksData).Select(x => x.Controller));
			if (DataHeader != null) DataHeader.IsChecked = false;
			SetDataFromController();
			SetDataFromControllerBackDatas();
		}

		private void RandomizeReworkWorkTypes(object sender, RoutedEventArgs e)
		{
			HaveUnsavedChanges = true;
			controller.TrimAllStrings();
			controller.RandomizeReworkWorkTypes(GetSelectedBackDatas(ReworkBacksData).Select(x => x.Controller));
			if (ReworkDataHeader != null) ReworkDataHeader.IsChecked = false;
			SetDataFromController();
			SetDataFromControllerBackDatas();
		}

		private void ExportDcmkClick(object sender, RoutedEventArgs e)
		{
			if (OpenedFilesComboBox.SelectedItem is DmxFile selectedFile && selectedFile.Loaded)
			{
				if (string.IsNullOrEmpty(selectedFile.Name))
					saveFileDialog.FileName = controller.GetDcmkFileName();
				else
					saveFileDialog.FileName = controller.GetSelectedFile().Name;

				if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					controller.ExportDcmk(saveFileDialog.FileName);

					MessageBox.Show("Файл збережений.",
						"DocumentMaker | Export dcmk",
						MessageBoxButtons.OK,
						MessageBoxIcon.Information);
				}
			}
			else
				MessageBox.Show("Спочатку необхідно відкрити файл.",
								"DocumentMaker | Збереження файлу",
								MessageBoxButtons.OK,
								MessageBoxIcon.Information);
		}

		private void ExportAllDcmkClick(object sender, RoutedEventArgs e)
		{
			if (OpenedFilesList.Count > 0)
			{
				string savedFiles = string.Empty;
				bool IsManySaveFiles = OpenedFilesList.Count > 1;
				bool IsSaveFile = !IsManySaveFiles;
				string filesSavePath = "";

				if (IsManySaveFiles)
				{
					DialogResult dialog = MessageBox.Show("Зберегти всі файли відразу?",
										"DocumentMaker | Збереження файлiв",
										MessageBoxButtons.YesNoCancel,
										MessageBoxIcon.Question);
					if (dialog == System.Windows.Forms.DialogResult.Yes)
					{
						if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
						{
							IsManySaveFiles = true;
							IsSaveFile = true;
							filesSavePath = folderBrowserDialog.SelectedPath + "\\";
						}
						else IsSaveFile = false;
					}
					else if (dialog == System.Windows.Forms.DialogResult.No)
					{
						IsManySaveFiles = false;
						IsSaveFile = true;
					}
					else if (dialog == System.Windows.Forms.DialogResult.Cancel)
						IsManySaveFiles = false;
				}

				if (IsSaveFile)
				{
					foreach (DmxFile file in OpenedFilesList)
					{
						SetSelectedFile(file.FullName);

						string fileName = file.Name;
						if (string.IsNullOrEmpty(file.Name))
							fileName = controller.GetDcmkFileName();

						if (!IsManySaveFiles || string.IsNullOrEmpty(filesSavePath))
						{
							saveFileDialog.FileName = fileName;

							if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
							{
								controller.ExportDcmk(saveFileDialog.FileName);
								savedFiles += "\n" + saveFileDialog.FileName;
							}
						}
						else
						{
							fileName = filesSavePath + fileName;
							controller.ExportDcmk(fileName);
							savedFiles += "\n" + fileName;
						}
					}
				}

				if (savedFiles != string.Empty)
				{
					MessageBox.Show("Файли збережені:" + savedFiles,
						"DocumentMaker | Export dcmk",
						MessageBoxButtons.OK,
						MessageBoxIcon.Information);
				}
			}
			else
				MessageBox.Show("Спочатку необхідно відкрити файл.",
								"DocumentMaker | Збереження файлу",
								MessageBoxButtons.OK,
								MessageBoxIcon.Information);
		}

		private void UnfocusOnEnter(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				Keyboard.ClearFocus();
			}
		}

		private void RedoClick(object sender, RoutedEventArgs e)
		{
			Redo();
		}

		private void UndoClick(object sender, RoutedEventArgs e)
		{
			Undo();
		}

		private void WindowKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if ((Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && Keyboard.IsKeyDown(Key.Y))
				|| (Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) && Keyboard.IsKeyDown(Key.Z)))
			{
				Redo();
				e.Handled = true;
			}
			else if ((Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && Keyboard.IsKeyDown(Key.Z))
				|| (Keyboard.Modifiers.HasFlag(ModifierKeys.Alt) && Keyboard.IsKeyDown(Key.Back)))
			{
				Undo();
				e.Handled = true;
			}
		}

		private async void ChangeAllDates(object sender, RoutedEventArgs e)
		{
			ChangeDatesDialog dialog = new ChangeDatesDialog
			{
				TechnicalTaskDateText = TechnicalTaskDateText,
				ActDateText = ActDateText
			};
			await DialogHost.Show(dialog);
			if (dialog.IsChanging)
			{
				bool changed = controller.ChangeTechnicalTaskDateAtAllFiles(dialog.TechnicalTaskDateText);
				changed = controller.ChangeActDateAtAllFiles(dialog.ActDateText) || changed;
				if (changed)
				{
					DmxFile selectedFile = controller.GetSelectedFile();
					if (selectedFile != null)
					{
						controller.TechnicalTaskDateText = selectedFile.TechnicalTaskDateText;
						controller.ActDateText = selectedFile.ActDateText;
						SetDataFromController();
					}
				}
			}
		}

		private async void ChangeAllContractFiles(object sender, RoutedEventArgs e)
		{
			ChangeContractFilesDialog dialog = new ChangeContractFilesDialog(ContractFilesComboBox);

			await DialogHost.Show(dialog);
			if (dialog.IsChanging)
			{
				bool changed = controller.ChangeContractFileAtAllFiles(dialog.ContractFilesComboBox.Text);
				if (changed)
				{
					DmxFile selectedFile = controller.GetSelectedFile();
					if (selectedFile != null)
					{
						controller.SelectedContractFile = selectedFile.SelectedContractFile;

						if (ContractFilesComboBox.Items.Contains(controller.SelectedContractFile))
							ContractFilesComboBox.SelectedItem = controller.SelectedContractFile;
						else
							ContractFilesComboBox.SelectedItem = null;
					}
				}
			}
		}

		private async void CalculateGamePrice(object sender, RoutedEventArgs e)
		{
			DmxFile selectedFile = controller.GetSelectedFile();
			if (selectedFile != null)
			{
				GamePriceDialog dialog = new GamePriceDialog(selectedFile.BackDataModels, controller.GameNameList);

				await DialogHost.Show(dialog);
			}
		}

		private void AboutUpdate(object sender, RoutedEventArgs e)
		{
			ChangeLog changeLog = new ChangeLog();
			Version version = updater.GetCurrentVersion();
			Dictionary<Version, string> versionInfo = new Dictionary<Version, string> { { version, updater.GetCurrentVersionText() } };
			changeLog.Initialize($"О версии программы {version}", versionInfo, MessageBoxButtons.OK);
			changeLog.ShowDialog();
		}

		private void CombineDcmkBtnClick(object sender, RoutedEventArgs e)
		{
			try
			{
				if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					string savedFiles = string.Empty;
					string filesSavePath;

					filesSavePath = folderBrowserDialog.SelectedPath + "\\";

					List<string> NameUser = new List<string>();
					List<DmxFile> UserDmx = new List<DmxFile>();

					foreach (DmxFile file in OpenedFilesList)
					{
						if (!NameUser.Contains(file.SelectedHuman))
							NameUser.Add(file.SelectedHuman);
					}

					NameUser.Sort(new Dml.NaturalStringComparer());

					foreach (string name in NameUser)
					{
						DmxFile fileDmx = null;
						DateTime maxDateAct = new DateTime();
						DateTime maxDateTeh = new DateTime();
						int actSum = 0;
						foreach (DmxFile file in OpenedFilesList)
						{
							if (name == file.SelectedHuman)
							{
								if (fileDmx == null)
									fileDmx = new DmxFile(Path.Combine(filesSavePath, file.SelectedHuman + DcmkFile.Extension));

								fileDmx.TemplateType = file.TemplateType;
								fileDmx.SelectedHuman = file.SelectedHuman;
								fileDmx.SelectedContractFile = file.SelectedContractFile;
								actSum += int.Parse(file.ActSum);
								DateTime tempDateAct = DateTime.Parse(file.ActDateText);
								if (maxDateAct.Date < tempDateAct.Date)
									maxDateAct = tempDateAct;

								DateTime tempDateTeh = DateTime.Parse(file.TechnicalTaskDateText);
								if (maxDateTeh.Date < tempDateTeh.Date)
									maxDateTeh = tempDateTeh;

								if (fileDmx.BackDataModels == null)
									fileDmx.SetLoadedBackData(file.BackDataModels);
								else
									fileDmx.AddRangeBackModel(file.BackDataModels);
							}
						}

						if (fileDmx != null)
						{
							fileDmx.ActSum = actSum.ToString();
							fileDmx.ActDateText = maxDateAct.ToString("dd.MM.yyyy");
							fileDmx.TechnicalTaskDateText = maxDateTeh.ToString("dd.MM.yyyy");
							fileDmx.ChangePath(Path.Combine(filesSavePath, fileDmx.SelectedHuman + " " + fileDmx.ActDateText + " " + fileDmx.ActSum + DcmkFile.Extension));
							UserDmx.Add(fileDmx);
						}
					}

					foreach (DmxFile file in UserDmx)
					{
						OpenedFilesList.Add(file);
						SetSelectedFile(file.FullName);
						controller.ExportDcmk(file.FullName);
						controller.CloseFile(file);
						savedFiles += "\n" + file.FullName;
					}

					if (savedFiles != string.Empty)
					{
						MessageBox.Show("Файли збережені:" + savedFiles,
							"DocumentMaker | Export dcmk",
							MessageBoxButtons.OK,
							MessageBoxIcon.Information);
					}
				}
			}
			catch(Exception exc)
			{
				SendExceptionTG(exc);
			}
		}

		#endregion

		#region Methods

		private IEnumerable<FullBackData> DeleteSelectedBackData(StackPanel stackPanel)
		{
			DmxFile selectedFile = controller.GetSelectedFile();
			List<UIElement> elems = new List<UIElement>();
			foreach (UIElement elem in stackPanel.Children)
			{
				if (elem is FullBackData backData)
				{
					if (backData.IsChecked.HasValue && backData.IsChecked.Value)
					{
						elems.Add(elem);
						backData.UnsubscribeAllEvents();
						controller.BackDataControllers.Remove(backData.Controller);
						selectedFile?.BackDataModels.Remove(backData.Controller.GetModel());
					}
				}
			}

			List<FullBackData> removed = new List<FullBackData>();
			foreach (UIElement elem in elems)
			{
				stackPanel.Children.Remove(elem);
				removed.Add((FullBackData)elem);
			}
			return removed;
		}

		private void MoveBackData(FullBackDataHeader headerFrom, StackPanel dataFrom, FullBackDataFooter footerFrom, FullBackDataHeader headerTo, FullBackDataFooter footerTo)
		{
			bool isNeedUpdateSum = controller.NeedUpdateSum;
			IEnumerable<FullBackData> removed = DeleteSelectedBackData(dataFrom);
			headerFrom.UpdateIsCheckedState();
			footerFrom.UpdateBackDataIds();
			footerFrom.UpdateAllSum();
			footerTo.AddMovedBackData(removed);
			headerTo.UpdateIsCheckedState();
			footerTo.UpdateBackDataIds();
			if (isNeedUpdateSum) EnableUpdatingSum();
		}

		private void SetDataFromControllerBackDatas()
		{
			bool actionsStackingEnable = controller.IsActionsStackingEnabled;
			controller.DisableActionsStacking();
			SetDataFromControllerBackDatas(BacksData);
			SetDataFromControllerBackDatas(ReworkBacksData);
			SetDataFromControllerBackDatas(OtherBacksData);
			if (actionsStackingEnable) controller.EnableActionsStacking();
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
			IList<WorkObject> currentWorkTypesList = controller.CurrentWorkTypesList,
				currentReworkWorkTypesList = controller.CurrentReworkWorkTypesList;

			foreach (UIElement control in BacksData.Children)
			{
				if (control is FullBackData backData)
				{
					backData.SetViewByTemplate(controller.TemplateType);
					backData.SetWorkTypesList(currentWorkTypesList);
					backData.SetGameNameList(controller.GameNameList);
					backData.SetBackDataTypesList(controller.CurrentBackDataTypesList);
				}
			}
			foreach (UIElement control in ReworkBacksData.Children)
			{
				if (control is FullBackData backData)
				{
					backData.SetViewByTemplate(controller.TemplateType);
					backData.SetWorkTypesList(currentReworkWorkTypesList);
					backData.SetGameNameList(controller.GameNameList);
					backData.SetBackDataTypesList(controller.CurrentBackDataTypesList);
				}
			}
			foreach (UIElement control in OtherBacksData.Children)
			{
				if (control is FullBackData backData)
				{
					backData.SetViewByTemplate(controller.TemplateType);
					backData.SetGameNameList(controller.GameNameList);
					backData.SetBackDataTypesList(controller.CurrentBackDataTypesList);
				}
			}

			DataHeader.SetViewByTemplate(controller.TemplateType);
			ReworkDataHeader.SetViewByTemplate(controller.TemplateType);
			OtherDataHeader.SetViewByTemplate(controller.TemplateType);

			DataFooter.SetViewByTemplate(controller.TemplateType);
			ReworkDataFooter.SetViewByTemplate(controller.TemplateType);
			OtherDataFooter.SetViewByTemplate(controller.TemplateType);
		}

		private void SetDataFromController()
		{
			bool actionsStackingEnable = controller.IsActionsStackingEnabled;
			controller.DisableActionsStacking();
			DocumentTemplateComboBox.SelectedIndex = (int)controller.TemplateType;

			if (ContractFilesComboBox.Items.Contains(controller.SelectedContractFile))
				ContractFilesComboBox.SelectedItem = controller.SelectedContractFile;
			else
				ContractFilesComboBox.SelectedIndex = 0;

			TechnicalTaskDatePicker.Text = controller.TechnicalTaskDateText;
			ActDatePicker.Text = controller.ActDateText;
			TechnicalTaskNumTextInput.Text = controller.TechnicalTaskNumText;
			ActSumInput.Text = controller.ActSum;
			ActSaldoInput.Text = controller.ActSaldo;
			if (actionsStackingEnable) controller.EnableActionsStacking();
		}

		private void SetDataToController()
		{
			bool actionsStackingEnable = controller.IsActionsStackingEnabled;
			controller.DisableActionsStacking();
			controller.TechnicalTaskDateText = TechnicalTaskDateText;
			controller.SelectedContractFile = ContractFilesComboBox.SelectedItem?.ToString();
			controller.ActDateText = ActDateText;
			controller.TechnicalTaskNumText = TechnicalTaskNumText;
			controller.ActSum = ActSum;
			controller.ActSaldo = ActSaldo;
			if (actionsStackingEnable) controller.EnableActionsStacking();
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

		private void AddLoadedBackData()
		{
			bool actionsStackingEnable = controller.IsActionsStackingEnabled;
			controller.DisableActionsStacking();

			DataFooter.ClearData();
			ReworkDataFooter.ClearData();
			OtherDataFooter.ClearData();
			foreach (FullBackDataController backDataController in controller.BackDataControllers)
			{
				backDataController.SetActionsStack(controller.GetActionsStack());
				if (backDataController.IsOtherType)
				{
					FullBackData backData = OtherDataFooter.AddLoadedBackData(backDataController);
					if (backData != null)
					{
						backData.SubscribeSelectionChanged(() =>
						{
							OtherDataHeader.UpdateIsCheckedState();
							UpdateActSumSelected();
						});
					}
				}
				else if (backDataController.IsRework)
				{
					FullBackData backData = ReworkDataFooter.AddLoadedBackData(backDataController);
					if (backData != null)
					{
						backData.SubscribeSelectionChanged(() =>
						{
							ReworkDataHeader.UpdateIsCheckedState();
							UpdateActSumSelected();
						});
					}
				}
				else
				{
					FullBackData backData = DataFooter.AddLoadedBackData(backDataController);
					if (backData != null)
					{
						backData.SubscribeSelectionChanged(() =>
						{
							DataHeader.UpdateIsCheckedState();
							UpdateActSumSelected();
						});
					}
				}
			}
			DataFooter.UpdateBackDataIds();
			ReworkDataFooter.UpdateBackDataIds();
			OtherDataFooter.UpdateBackDataIds();

			if (actionsStackingEnable) controller.EnableActionsStacking();
		}

		private IEnumerable<FullBackData> AddNewSupport(IEnumerable<FullBackDataController> controllers)
		{
			bool actionsStackingEnable = controller.IsActionsStackingEnabled;
			controller.DisableActionsStacking();

			DmxFile selectedFile = controller.GetSelectedFile();
			if (selectedFile != null)
			{
				selectedFile.AddRangeBackModel(controllers.Select(x => x.GetModel()));
			}

			List<FullBackData> addedNewSupport = new List<FullBackData>();
			foreach (FullBackDataController backDataController in controllers)
			{
				backDataController.SetActionsStack(controller.GetActionsStack());
				if (!backDataController.IsOtherType && backDataController.IsRework)
				{
					controller.BackDataControllers.Add(backDataController);
					FullBackData backData = ReworkDataFooter.AddLoadedBackData(backDataController);
					if (backData != null)
					{
						backData.SubscribeSelectionChanged(() =>
						{
							ReworkDataHeader.UpdateIsCheckedState();
							UpdateActSumSelected();
						});
						addedNewSupport.Add(backData);
					}
				}
			}
			ReworkDataFooter.UpdateBackDataIds();

			if (actionsStackingEnable) controller.EnableActionsStacking();

			return addedNewSupport;
		}

		private void SetSelectedFile(string filename)
		{
			foreach (DmxFile file in OpenedFilesList)
			{
				if (Path.ChangeExtension(file.FullName, null) == Path.ChangeExtension(filename, null) || Path.ChangeExtension(file.Name, null) == Path.ChangeExtension(filename, null))
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
			WindowValidator.MoveToValidPosition(this);
			WindowState = controller.WindowState;
		}

		private void UpdateActSum()
		{
			bool actionsStackingEnable = controller.IsActionsStackingEnabled;
			if (controller.NeedUpdateSum) controller.DisableActionsStacking();

			uint sum = uint.TryParse(ActSum, out uint s) ? s : 0;
			UpdateActSumBackDataPanel(BacksData, sum);
			UpdateActSumBackDataPanel(ReworkBacksData, sum);
			UpdateActSumBackDataPanel(OtherBacksData, sum);
			UpdateSaldo();

			if (controller.NeedUpdateSum)
			{
				DropSaldoToLast();

				if (actionsStackingEnable) controller.EnableActionsStacking();
			}
		}

		private void DropSaldoToLast()
		{
			if (!int.TryParse(ActSaldo, out int currentSaldo)) return;

			FullBackDataController last = controller.BackDataControllers.LastOrDefault();
			if (last != null && int.TryParse(last.SumText, out int lastSum))
			{
				last.SumText = (lastSum + currentSaldo).ToString();

				foreach (UIElement elem in BacksData.Children)
				{
					if (elem is FullBackData backData && backData.Controller == last)
					{
						backData.SetDataFromController();
						return;
					}
				}
				foreach (UIElement elem in ReworkBacksData.Children)
				{
					if (elem is FullBackData backData && backData.Controller == last)
					{
						backData.SetDataFromController();
						return;
					}
				}
				foreach (UIElement elem in OtherBacksData.Children)
				{
					if (elem is FullBackData backData && backData.Controller == last)
					{
						backData.SetDataFromController();
						return;
					}
				}
			}
		}

		private void UpdateActSumBackDataPanel(StackPanel stackPanel, uint sum)
		{
			if (stackPanel != null)
			{
				foreach (UIElement elem in stackPanel.Children)
				{
					if (elem is FullBackData backData)
					{
						backData.SetActSum(sum, controller.NeedUpdateSum);
					}
				}
			}
		}

		private void UpdateSaldo()
		{
			UpdateActSumSelected();
			uint sum = 0;
			if (uint.TryParse(ActSum, out uint s))
			{
				sum = s;
			}

			if (uint.TryParse(DataFooter.AllSum, out uint curSum)
				&& uint.TryParse(ReworkDataFooter.AllSum, out uint curSumRework)
				&& uint.TryParse(OtherDataFooter.AllSum, out uint curSumOther))
			{
				ActSaldo = ((int)sum - (int)(curSum + curSumRework + curSumOther)).ToString();
				ActSaldoInput.Text = ActSaldo;
			}
		}

		private void CheckFiles()
		{
			List<string> files = new List<string>()
			{
				"projectnames.xml",
				"DevelopmentTypes.xlsx",
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

			List<string> notLoadedFilesList = controller.GetNotLoadedFilesList();
			if (notLoadedFilesList != null && notLoadedFilesList.Count > 0)
			{
				string notLoadedFiles = "";
				foreach (string file in notLoadedFilesList)
				{
					notLoadedFiles += '\n' + file;
				}

				MessageBox.Show("Не вдалось загрузити необхідні файли - можливо вони відкриті в іншій програмі. (програма може працювати з помилками):\n" + notLoadedFiles,
					"DocumentMaker | Відсутні файли",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error);
			}
		}

		private void EnableUpdatingSum()
		{
			SetNeedUpdateSumState(true);
			if (CanUndoNeedUpdateSum)
			{
				controller.ResetWeights();
			}
		}

		private void DisableUpdatingSum()
		{
			SetNeedUpdateSumState(false);
		}

		private void SetNeedUpdateSumState(bool state)
		{
			controller.NeedUpdateSum = state;
			DmxFile selectedFile = controller.GetSelectedFile();
			if (selectedFile != null)
			{
				selectedFile.NeedUpdateSum = state;
			}
		}

		private void UpdateActSumSelected()
		{
			ActSumSelectedInput.Text = GetSelectedBackDatas().Sum(x => int.TryParse(x.SumText, out int s) ? s : 0).ToString();
		}

		private IEnumerable<FullBackData> GetSelectedBackDatas()
		{
			return GetSelectedBackDatas(BacksData)
				.Union(GetSelectedBackDatas(ReworkBacksData))
				.Union(GetSelectedBackDatas(OtherBacksData));
		}

		private IEnumerable<FullBackData> GetSelectedBackDatas(StackPanel stackPanel)
		{
			foreach (UIElement elem in stackPanel.Children)
			{
				if (elem is FullBackData backData && backData.IsChecked.HasValue && backData.IsChecked.Value)
				{
					yield return backData;
				}
			}
		}

		private void AddUndoRedoLinkNeedUpdateSum(bool isNeedUpdateSum)
		{
			controller.AddUndoRedoLink(new UndoRedoLink(
					redo: (data) =>
					{
						if ((bool)data && CanUndoNeedUpdateSum)
							DisableUpdatingSum();
					},
					undo: (data) =>
					{
						if ((bool)data && CanUndoNeedUpdateSum)
							EnableUpdatingSum();
					}
					)
			{ Data = isNeedUpdateSum });
		}

		private void AddUndoRedoLinkNeedUpdateSumWithCheck(bool isNeedUpdateSum)
		{
			if (controller.IsActionsStackingEnabled)
			{
				AddUndoRedoLinkNeedUpdateSum(isNeedUpdateSum);
			}
		}

		private IEnumerable<FullBackData> GetAllFullBacksData()
		{
			return GetAllFullBacksData(BacksData)
				.Union(GetAllFullBacksData(ReworkBacksData))
				.Union(GetAllFullBacksData(OtherBacksData));
		}

		private IEnumerable<FullBackData> GetAllFullBacksData(StackPanel stackPanel)
		{
			foreach (UIElement elem in stackPanel.Children)
			{
				if (elem is FullBackData backData)
				{
					yield return backData;
				}
			}
		}

		private void Redo()
		{
			controller.DisableActionsStacking();
			controller.Redo();
			controller.EnableActionsStacking();
			UpdateUndoRedoState();
		}

		private void Undo()
		{
			controller.DisableActionsStacking();
			controller.Undo();
			controller.EnableActionsStacking();
			UpdateUndoRedoState();
		}

		private void UpdateUndoRedoState()
		{
			CanUndo = controller.CanUndo;
			CanRedo = controller.CanRedo;
		}

		private bool HaveUnsavedChangesAtAll()
		{
			return controller.HaveUnsavedChangesAtAll();
		}

		private void ResetHaveUnsavedChanges()
		{
			controller.ResetHaveUnsavedChanges();
		}

		private void CheckNeedSaveBeforeClosing(out DialogResult dialogResult)
		{
			dialogResult = System.Windows.Forms.DialogResult.None;
			if (HaveUnsavedChangesAtAll())
			{
				dialogResult = MessageBox.Show("Файл має незбережені зміни. Зберегти файл перед закриттям?",
					"Закриття файлу",
					MessageBoxButtons.YesNoCancel,
					MessageBoxIcon.Question,
					MessageBoxDefaultButton.Button1);

				if (dialogResult == System.Windows.Forms.DialogResult.Yes)
				{
					saveFileDialog.FileName = controller.GetDcmkFileName();
					dialogResult = saveFileDialog.ShowDialog();
					if (dialogResult == System.Windows.Forms.DialogResult.OK)
					{
						controller.ExportDcmk(saveFileDialog.FileName);

						MessageBox.Show("Файл збережений.",
							"DocumentMaker | Export dcmk",
							MessageBoxButtons.OK,
							MessageBoxIcon.Information);
					}
				}
			}
		}

		private void OnAdded(FullBackData backData, bool isRework, bool isOtherType)
		{
			HaveUnsavedChanges = true;
			DisableUpdatingSum();
			CanUndoNeedUpdateSum = false;
			backData.Controller.IsRework = isRework;
			backData.Controller.IsOtherType = isOtherType;
			backData.Controller.SetActionsStack(controller.GetActionsStack());
			controller.BackDataControllers.Add(backData.Controller);
			backData.SetViewByTemplate(controller.TemplateType);
			if (!isOtherType)
			{
				backData.SetWorkTypesList(isRework ? controller.CurrentReworkWorkTypesList : controller.CurrentWorkTypesList);
			}
			backData.SetBackDataTypesList(controller.CurrentBackDataTypesList);
			backData.SetGameNameList(controller.GameNameList);
			FullBackDataHeader header = isOtherType ? OtherDataHeader : (isRework ? ReworkDataHeader : DataHeader);
			backData.SubscribeSelectionChanged(() =>
			{
				header.UpdateIsCheckedState();
				UpdateActSumSelected();
			});
			UpdateActSum();
			header.UpdateIsCheckedState();

			DmxFile selectedFile = controller.GetSelectedFile();
			if (selectedFile != null)
			{
				selectedFile.AddBackModel(backData.Controller.GetModel());
			}
			backData.UpdateInputStates();
		}

		private void OnSelectionChanged(bool? isSelected, bool isRework, bool isOtherType)
		{
			UIElementCollection backsData = (isOtherType ? OtherBacksData : (isRework ? ReworkBacksData : BacksData))?.Children;
			if (backsData != null && isSelected.HasValue)
			{
				foreach (UIElement elem in backsData)
				{
					if (elem is FullBackData backData)
					{
						backData.SetIsCheckedWithoutCallback(isSelected.Value);
					}
				}
			}
			UpdateActSumSelected();
		}

		private void OnSumChanged(bool changedWeight)
		{
			if (changedWeight)
			{
				AddUndoRedoLinkNeedUpdateSumWithCheck(controller.NeedUpdateSum);
				DisableUpdatingSum();
			}
			UpdateSaldo();
		}

		private void SendExceptionTG(Exception exc)
		{
			SendException.ExceptionDialog dialog = new SendException.ExceptionDialog(exc, telegramGroupId, updater.GetCurrentVersion());
			dialog.ShowDialog();
		}

		#endregion
	}
}
