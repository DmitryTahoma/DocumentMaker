using Dml.Controller.Validation;
using Dml.Model.Files;
using Dml.Model.Template;
using Dml.UndoRedo;
using DocumentMaker.Controller;
using DocumentMaker.Controller.Controls;
using DocumentMaker.Model;
using DocumentMaker.Model.Back;
using DocumentMaker.Model.Files;
using DocumentMaker.Model.OfficeFiles.Human;
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

#if INCLUDED_UPDATER_API
using UpdaterAPI;
using UpdaterAPI.Resources;
using System.Threading.Tasks;
#endif

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

		static MainWindow()
		{
			TechnicalTaskDateTextProperty = DependencyProperty.Register("TechnicalTaskDateText", typeof(string), typeof(MainWindow));
			ActDateTextProperty = DependencyProperty.Register("ActDateText", typeof(string), typeof(MainWindow));
			TechnicalTaskNumTextProperty = DependencyProperty.Register("TechnicalTaskNumText", typeof(string), typeof(MainWindow));
			ActSumProperty = DependencyProperty.Register("ActSum", typeof(string), typeof(MainWindowController));
			ActSumSelectedProperty = DependencyProperty.Register("ActSumSelected", typeof(string), typeof(MainWindowController));
			ActSaldoProperty = DependencyProperty.Register("ActSaldo", typeof(string), typeof(MainWindowController));
			ContentVisibilityProperty = DependencyProperty.Register("ContentVisibility", typeof(Visibility), typeof(MainWindow));
			ButtonOpenContentVisibilityProperty = DependencyProperty.Register("ButtonOpenContentVisibility", typeof(Visibility), typeof(MainWindow));
		}

		#endregion

		private readonly MainWindowController controller;
		private readonly FolderBrowserDialog folderBrowserDialog;
		private readonly OpenFileDialog openFileDialog;
		private readonly SaveFileDialog saveFileDialog;
		private readonly InputingValidator inputingValidator;

		public MainWindow(string[] args)
		{
			controller = new MainWindowController(args);
			controller.Load();
			SetWindowSettingsFromController();

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

#if INCLUDED_UPDATER_API
			AssemblyLoader.LoadWinScp();
#endif
		}

		private void InitializeComponentFromCode()
		{
			TechnicalTaskDatePicker.Language = XmlLanguage.GetLanguage(System.Globalization.CultureInfo.CurrentCulture.IetfLanguageTag);
			ActDatePicker.Language = XmlLanguage.GetLanguage(System.Globalization.CultureInfo.CurrentCulture.IetfLanguageTag);

			DataHeader.SubscribeSelectionChanged((b) =>
			{
				if (b.HasValue)
				{
					foreach (UIElement elem in BacksData.Children)
					{
						if (elem is FullBackData backData)
						{
							backData.SetIsCheckedWithoutCallback(b.Value);
						}
					}
				}
				UpdateActSumSelected();
			});
			DataFooter.SubscribeAddition((x) =>
			{
				DisableUpdatingSum();
				CanUndoNeedUpdateSum = false;
				x.Controller.IsRework = false;
				x.Controller.IsOtherType = false;
				x.Controller.SetActionsStack(controller.GetActionsStack());
				controller.BackDataControllers.Add(x.Controller);
				x.SetViewByTemplate(controller.TemplateType);
				x.SetWorkTypesList(controller.CurrentWorkTypesList);
				x.SetGameNameList(controller.GameNameList);
				x.SubscribeSelectionChanged(() =>
				{
					DataHeader.UpdateIsCheckedState();
					UpdateActSumSelected();
				});
				UpdateActSum();
				DataHeader.UpdateIsCheckedState();

				DmxFile selectedFile = controller.GetSelectedFile();
				if (selectedFile != null)
				{
					selectedFile.AddBackModel(x.Controller.GetModel());
				}
			});
			DataFooter.SubscribeChangingSum((changedWeight) =>
			{
				if (changedWeight)
				{
					AddUndoRedoLinkNeedUpdateSum(controller.NeedUpdateSum);
					DisableUpdatingSum();
				}
				UpdateSaldo();
			});

			ReworkDataHeader.SubscribeSelectionChanged((b) =>
			{
				if (b.HasValue)
				{
					foreach (UIElement elem in ReworkBacksData.Children)
					{
						if (elem is FullBackData backData)
						{
							backData.SetIsCheckedWithoutCallback(b.Value);
						}
					}
				}
				UpdateActSumSelected();
			});
			ReworkDataFooter.SubscribeAddition((x) =>
			{
				DisableUpdatingSum();
				CanUndoNeedUpdateSum = false;
				x.Controller.IsRework = true;
				x.Controller.IsOtherType = false;
				x.Controller.SetActionsStack(controller.GetActionsStack());
				controller.BackDataControllers.Add(x.Controller);
				x.SetViewByTemplate(controller.TemplateType);
				x.SetWorkTypesList(controller.CurrentReworkWorkTypesList);
				x.SetGameNameList(controller.GameNameList);
				x.SubscribeSelectionChanged(() =>
				{
					ReworkDataHeader.UpdateIsCheckedState();
					UpdateActSumSelected();
				});
				UpdateActSum();
				ReworkDataHeader.UpdateIsCheckedState();

				DmxFile selectedFile = controller.GetSelectedFile();
				if (selectedFile != null)
				{
					selectedFile.AddBackModel(x.Controller.GetModel());
				}
				x.UpdateInputStates();
			});
			ReworkDataFooter.SubscribeChangingSum((changedWeight) =>
			{
				if (changedWeight)
				{
					AddUndoRedoLinkNeedUpdateSum(controller.NeedUpdateSum);
					DisableUpdatingSum();
				}
				UpdateSaldo();
			});

			OtherDataHeader.HideWorkTypeLabel();
			OtherDataHeader.SubscribeSelectionChanged((b) =>
			{
				if (b.HasValue)
				{
					foreach (UIElement elem in OtherBacksData.Children)
					{
						if (elem is FullBackData backData)
						{
							backData.SetIsCheckedWithoutCallback(b.Value);
						}
					}
				}
				UpdateActSumSelected();
			});
			OtherDataFooter.SubscribeAddition((x) =>
			{
				DisableUpdatingSum();
				CanUndoNeedUpdateSum = false;
				x.Controller.IsOtherType = true;
				x.Controller.SetActionsStack(controller.GetActionsStack());
				controller.BackDataControllers.Add(x.Controller);
				x.SetViewByTemplate(controller.TemplateType);
				x.SetGameNameList(controller.GameNameList);
				x.SubscribeSelectionChanged(() =>
				{
					OtherDataHeader.UpdateIsCheckedState();
					UpdateActSumSelected();
				});
				UpdateActSum();
				OtherDataHeader.UpdateIsCheckedState();

				DmxFile selectedFile = controller.GetSelectedFile();
				if (selectedFile != null)
				{
					selectedFile.AddBackModel(x.Controller.GetModel());
				}
				x.UpdateInputStates();
			});
			OtherDataFooter.SubscribeChangingSum((changedWeight) =>
			{
				if (changedWeight)
				{
					AddUndoRedoLinkNeedUpdateSum(controller.NeedUpdateSum);
					DisableUpdatingSum();
				}
				UpdateSaldo();
			});

			ActSumInput.CommandBindings.Add(new System.Windows.Input.CommandBinding(ApplicationCommands.Paste, inputingValidator.BlockingCommand));
		}

		#region Properties

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

		public string TechnicalTaskNumText
		{
			get => (string)GetValue(TechnicalTaskNumTextProperty);
			set => SetValue(TechnicalTaskNumTextProperty, value);
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

		#endregion

		#region Event handlers

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

#if INCLUDED_UPDATER_API
		private async void WindowLoaded(object sender, RoutedEventArgs e)
#else
		private void WindowLoaded(object sender, RoutedEventArgs e)
#endif
		{
			CheckFiles();
			WindowState = controller.WindowState;
			if (controller != null)
			{
				SetDataFromController();
				string[] files = controller.GetOpenLaterFiles();
				OpenFiles(files);
				LoadFiles();
				if (files != null && files.Length > 0)
				{
					SetSelectedFile(files.Last());
				}
				UpdateActSum();

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
			controller.EnableActionsStacking();
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
			if (OpenedFilesComboBox.SelectedItem is DmxFile selectedFile && selectedFile.Loaded)
			{
				int index = OpenedFilesComboBox.SelectedIndex;
				if (index != -1)
				{
					controller.CloseFile(selectedFile);
					OpenedFilesComboBox.SelectedIndex = OpenedFilesComboBox.Items.Count <= index ? index - 1 : index;
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
				while (OpenedFilesList.Count > 0)
				{
					controller.CloseFile(OpenedFilesList[0]);
				}
				OpenedFilesComboBox.SelectedIndex = -1;
			}
			else
				MessageBox.Show("Спочатку необхідно відкрити файл.",
								"DocumentMaker | Закриття файлу",
								MessageBoxButtons.OK,
								MessageBoxIcon.Information);
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
			IEnumerable<int> resultSums = controller.CorrectSaldo(selectedBackDatas.Select(x=>x.Controller));

			IEnumerator<FullBackData> selectedBackDatasEnum = selectedBackDatas.GetEnumerator();
			IEnumerator<int> resultSumsEnum = resultSums.GetEnumerator();
			bool pushedFirst = false;
			while(selectedBackDatasEnum.MoveNext() && resultSumsEnum.MoveNext())
			{
				selectedBackDatasEnum.Current.SumTextInput.Text = resultSumsEnum.Current.ToString();
				if(!pushedFirst)
				{
					pushedFirst = true;
					AddUndoRedoLinkNeedUpdateSum(isNeedUpdateSum);
				}
			}

			SetDataFromControllerBackDatas();
			DataFooter?.UpdateAllSum();
			ReworkDataFooter?.UpdateAllSum();
			OtherDataFooter?.UpdateAllSum();
		}

		private async void CorrectDevelopClick(object sender, RoutedEventArgs e)
		{
			bool isNeedUpdateSum = controller.NeedUpdateSum;
			DisableUpdatingSum();
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
				IEnumerable<int> resultSums = controller.CorrectDevelopment(sum, dialog.TakeSumFromSupport);
				IEnumerator<FullBackDataController> backDataControllersEnum = controller.BackDataControllers.GetEnumerator();
				IEnumerator<int> resultSumsEnum = resultSums.GetEnumerator();
				List<FullBackData> allFullBackDatas = new List<FullBackData>(GetAllFullBacksData());
				bool pushedFirst = false;
				while(backDataControllersEnum.MoveNext() && resultSumsEnum.MoveNext())
				{
					FullBackData current = allFullBackDatas.FirstOrDefault(x => x.Controller == backDataControllersEnum.Current);
					if(current != null && current.SumTextInput.Text != resultSumsEnum.Current.ToString())
					{
						current.SumTextInput.Text = resultSumsEnum.Current.ToString();
						if(!pushedFirst)
						{
							pushedFirst = true;
							AddUndoRedoLinkNeedUpdateSum(isNeedUpdateSum);
						}
					}
				}

				SetDataFromControllerBackDatas();
			}
		}

		private async void CorrectSupportClick(object sender, RoutedEventArgs e)
		{
			bool isNeedUpdateSum = controller.NeedUpdateSum;
			DisableUpdatingSum();
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
				IEnumerable<int> resultSums = controller.CorrectSupport(sum, dialog.TakeSumFromDevelopment);
				IEnumerator<FullBackDataController> backDataControllersEnum = controller.BackDataControllers.GetEnumerator();
				IEnumerator<int> resultSumsEnum = resultSums.GetEnumerator();
				List<FullBackData> allFullBackDatas = new List<FullBackData>(GetAllFullBacksData());
				bool pushedFirst = false;
				while (backDataControllersEnum.MoveNext() && resultSumsEnum.MoveNext())
				{
					FullBackData current = allFullBackDatas.FirstOrDefault(x => x.Controller == backDataControllersEnum.Current);
					if (current != null && current.SumTextInput.Text != resultSumsEnum.Current.ToString())
					{
						current.SumTextInput.Text = resultSumsEnum.Current.ToString();
						if (!pushedFirst)
						{
							pushedFirst = true;
							AddUndoRedoLinkNeedUpdateSum(isNeedUpdateSum);
						}
					}
				}

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
		}

		public void MoveFromSupport(object sender, RoutedEventArgs e)
		{
			MoveBackData(ReworkDataHeader, ReworkBacksData, ReworkDataFooter, DataHeader, DataFooter);
		}

		public void MoveFromOtherToDevelopment(object sender, RoutedEventArgs e)
		{
			MoveBackData(OtherDataHeader, OtherBacksData, OtherDataFooter, DataHeader, DataFooter);
		}

		public void MoveFromOtherToSupport(object sender, RoutedEventArgs e)
		{
			MoveBackData(OtherDataHeader, OtherBacksData, OtherDataFooter, ReworkDataHeader, ReworkDataFooter);
		}

		private void WindowPreviewDrop(object sender, System.Windows.DragEventArgs e)
		{
			if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop, true))
			{
				string[] filenames = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop, true);
				OpenFiles(filenames);
				LoadFiles();
				SetSelectedFile(filenames.Last(filename => filename.EndsWith(BaseDmxFile.Extension) || filename.EndsWith(DcmkFile.Extension)));
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

		private void OpenedFilesSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			if (sender is System.Windows.Controls.ComboBox comboBox && comboBox.SelectedItem is DmxFile selectedFile && selectedFile.Loaded)
			{
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
			}
			else
			{
				ContentVisibility = Visibility.Hidden;
				ButtonOpenContentVisibility = Visibility.Visible;
			}
		}

		private void ActSumTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
		{
			if(controller.IsActionsStackingEnabled && sender is System.Windows.Controls.TextBox textBox)
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
			controller.TrimAllStrings();
			controller.RandomizeWorkTypes(GetSelectedBackDatas(BacksData).Select(x=>x.Controller));
			SetDataFromController();
			SetDataFromControllerBackDatas();
		}

		private void RandomizeReworkWorkTypes(object sender, RoutedEventArgs e)
		{
			controller.TrimAllStrings();
			controller.RandomizeReworkWorkTypes(GetSelectedBackDatas(ReworkBacksData).Select(x => x.Controller));
			SetDataFromController();
			SetDataFromControllerBackDatas();
		}

		private void ExportDcmkClick(object sender, RoutedEventArgs e)
		{
			if (OpenedFilesComboBox.SelectedItem is DmxFile selectedFile && selectedFile.Loaded)
			{
				saveFileDialog.FileName = controller.GetDcmkFileName();
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

				foreach (DmxFile file in OpenedFilesList)
				{
					SetSelectedFile(file.FullName);
					saveFileDialog.FileName = controller.GetDcmkFileName();
					if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
					{
						controller.ExportDcmk(saveFileDialog.FileName);

						savedFiles += "\n" + saveFileDialog.FileName;
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
			controller.DisableActionsStacking();
			controller.Redo();
			controller.EnableActionsStacking();
		}

		private void UndoClick(object sender, RoutedEventArgs e)
		{
			controller.DisableActionsStacking();
			controller.Undo();
			controller.EnableActionsStacking();
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
			IEnumerable<FullBackData> removed = DeleteSelectedBackData(dataFrom);
			headerFrom.UpdateIsCheckedState();
			footerFrom.UpdateBackDataIds();
			footerFrom.UpdateAllSum();
			footerTo.AddMovedBackData(removed);
			headerTo.UpdateIsCheckedState();
			footerTo.UpdateBackDataIds();
		}

		private void SetDataFromControllerBackDatas()
		{
			bool actionsStackingEnable = controller.IsActionsStackingEnabled;
			controller.DisableActionsStacking();
			SetDataFromControllerBackDatas(BacksData);
			SetDataFromControllerBackDatas(ReworkBacksData);
			SetDataFromControllerBackDatas(OtherBacksData);
			if(actionsStackingEnable) controller.EnableActionsStacking();
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
				}
			}
			foreach (UIElement control in ReworkBacksData.Children)
			{
				if (control is FullBackData backData)
				{
					backData.SetViewByTemplate(controller.TemplateType);
					backData.SetWorkTypesList(currentReworkWorkTypesList);
					backData.SetGameNameList(controller.GameNameList);
				}
			}
			foreach (UIElement control in OtherBacksData.Children)
			{
				if (control is FullBackData backData)
				{
					backData.SetViewByTemplate(controller.TemplateType);
					backData.SetGameNameList(controller.GameNameList);
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
			controller.ActDateText = ActDateText;
			controller.TechnicalTaskNumText = TechnicalTaskNumText;
			controller.ActSum = ActSum;
			controller.ActSaldo = ActSaldo;
			if(actionsStackingEnable) controller.EnableActionsStacking();
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
						backData.SubscribeSelectionChanged(() => {
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
						backData.SubscribeSelectionChanged(() => {
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
						backData.SubscribeSelectionChanged(() => {
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
			WindowValidator.MoveToValidPosition(this);
		}

		private void UpdateActSum()
		{
			bool actionsStackingEnable = controller.IsActionsStackingEnabled;
			if(controller.NeedUpdateSum) controller.DisableActionsStacking();

			if (uint.TryParse(ActSum, out uint sum))
			{
				UpdateActSumBackDataPanel(BacksData, sum);
				UpdateActSumBackDataPanel(ReworkBacksData, sum);
				UpdateActSumBackDataPanel(OtherBacksData, sum);
			}
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
				"HumanData.xlsx",
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
			foreach(UIElement elem in stackPanel.Children)
			{
				if(elem is FullBackData backData && backData.IsChecked.HasValue && backData.IsChecked.Value)
				{
					yield return backData;
				}
			}
		}

		private void AddUndoRedoLinkNeedUpdateSum(bool isNeedUpdateSum)
		{
			if (controller.IsActionsStackingEnabled)
			{
				controller.AddUndoRedoLink(new UndoRedoLink(
					redo: (data) => { if ((bool)data && CanUndoNeedUpdateSum) DisableUpdatingSum(); },
					undo: (data) => { if ((bool)data && CanUndoNeedUpdateSum) EnableUpdatingSum(); }
					)
				{ Data = isNeedUpdateSum });
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

		#endregion
	}
}
