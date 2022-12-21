using Dml.Controller.Validation;
using Dml.Model.Files;
using Dml.Model.Template;
using Dml.UndoRedo;
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
		private readonly FolderBrowserDialog folderBrowserDialog;
		private readonly OpenFileDialog openFileDialog;
		private readonly SaveFileDialog saveFileDialog;
		private readonly InputingValidator inputingValidator;

		private bool cancelOpenedFilesSelectionChanged;

		public MainWindow(string[] args)
		{
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

			InitializeComponent();
			InitializeComponentFromCode();

			ViewModel.ApplicationArgs = args;
			ViewModel.Load();
			SetWindowSettingsFromController();

#if INCLUDED_UPDATER_API
			AssemblyLoader.LoadWinScp();
#endif
		}

		private void InitializeComponentFromCode()
		{
			TechnicalTaskDatePicker.Language = XmlLanguage.GetLanguage(System.Globalization.CultureInfo.CurrentCulture.IetfLanguageTag);
			ActDatePicker.Language = XmlLanguage.GetLanguage(System.Globalization.CultureInfo.CurrentCulture.IetfLanguageTag);

			DataHeader.SubscribeSelectionChanged((b) => OnSelectionChanged(b, false, false));
			DataFooter.SubscribeAddition((x) => OnAdded(x, false, false));
			DataFooter.SubscribeChangingSum(OnSumChanged);
			DataFooter.ActionsStack = ViewModel.GetActionsStack();

			ReworkDataHeader.SubscribeSelectionChanged((b) => OnSelectionChanged(b, true, false));
			ReworkDataFooter.SubscribeAddition((x) => OnAdded(x, true, false));
			ReworkDataFooter.SubscribeChangingSum(OnSumChanged);
			ReworkDataFooter.ActionsStack = ViewModel.GetActionsStack();

			OtherDataHeader.HideWorkTypeLabel();
			OtherDataHeader.SubscribeSelectionChanged((b) => OnSelectionChanged(b, false, true));
			OtherDataFooter.SubscribeAddition((x) => OnAdded(x, false, true));
			OtherDataFooter.SubscribeChangingSum(OnSumChanged);
			OtherDataFooter.ActionsStack = ViewModel.GetActionsStack();

			ActSumInput.CommandBindings.Add(new CommandBinding(ApplicationCommands.Paste, inputingValidator.BlockingCommand));
		}

		#region Properties

		public IList<DmxFile> OpenedFilesList => ViewModel.OpenedFilesList;

		public IList<FullDocumentTemplate> DocumentTemplatesList => ViewModel.DocumentTemplatesList;

		public double IconSize { get; } = 24;

		private bool CanUndoNeedUpdateSum { get; set; } = true;

		public bool HaveUnsavedChanges { get => ViewModel.HaveUnsavedChanges; set => ViewModel.HaveUnsavedChanges = value; }

		#endregion

		#region Event handlers

		private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			SetDataToController();
			ViewModel.WindowTop = Top;
			ViewModel.WindowLeft = Left;
			ViewModel.WindowHeight = Height;
			ViewModel.WindowWidth = Width;
			ViewModel.WindowState = WindowState == WindowState.Minimized ? WindowState.Normal : WindowState;

			ViewModel.Save();
		}

#if INCLUDED_UPDATER_API
		private async void WindowLoaded(object sender, RoutedEventArgs e)
#else
		private void WindowLoaded(object sender, RoutedEventArgs e)
#endif
		{
			CheckFiles();
			WindowState = ViewModel.WindowState;
			if (ViewModel != null)
			{
				SetDataFromController();
				ViewModel.OpenFiles(ViewModel.ApplicationArgs);
				ResetHaveUnsavedChanges();
				LoadFiles();
				if (ViewModel.ApplicationArgs != null && ViewModel.ApplicationArgs.Length > 0)
				{
					SetSelectedFile(ViewModel.ApplicationArgs.Last());
				}
				ViewModel.ChangeOpenedFilesExtension();
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
					catch (Exception exc)
					{
						UpdateLog.WriteLine("DocumentMaker: Невозможно подключиться к шаре!\n" + exc.ToString(), "red");
					}
				});
#endif
			}
			ViewModel.EnableActionsStacking();
			ViewModel.SubscribeActionPushed((action) => { UpdateUndoRedoState(); });
			ResetHaveUnsavedChanges();
		}

		private void ChangedDocumentTemplate(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			if (ViewModel != null
				&& sender is System.Windows.Controls.ComboBox comboBox
				&& comboBox.SelectedItem is DocumentTemplate documentTemplate)
			{
				HaveUnsavedChanges = true;
				ViewModel.TemplateType = documentTemplate.Type;
				UpdateViewBackData();

				if (OpenedFilesComboBox != null
					&& ViewModel.SelectedOpenedFile is DmxFile selectedFile)
				{
					selectedFile.TemplateType = documentTemplate.Type;
				}
			}
		}

		private void ChangedHuman(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			if (ViewModel != null
				&& sender is System.Windows.Controls.ComboBox comboBox
				&& comboBox.SelectedItem is HumanData humanData)
			{
				ViewModel.SetHuman(humanData);
				SetDataFromController();

				if (OpenedFilesComboBox != null
					&& ViewModel.SelectedOpenedFile is DmxFile selectedFile)
				{
					selectedFile.SelectedHuman = humanData.Name;
				}
			}
		}

		private void ExportBtnClick(object sender, RoutedEventArgs e)
		{
			try
			{
				if (!(ViewModel.SelectedOpenedFile is DmxFile selectedFile && selectedFile.Loaded))
				{
					MessageBox.Show("Спочатку необхідно відкрити файл.",
									"DocumentMaker | Експорт актів",
									MessageBoxButtons.OK,
									MessageBoxIcon.Information);
					return;
				}

				SetDataToController();
				ViewModel.TrimAllStrings();
				if (ViewModel.Validate(out string errorText))
				{
					if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
					{
						bool isShowResult = true;
						ViewModel.Export(folderBrowserDialog.SelectedPath);

						if (ViewModel.HasNoMovedFiles)
						{
							if (MessageBox.Show("Файли за заданними путями вже існують.\n\n" + ViewModel.GetInfoNoMovedFiles() + "\nЗамінити?",
												"DocumentMaker | Export",
												MessageBoxButtons.YesNo,
												MessageBoxIcon.Question)
													== System.Windows.Forms.DialogResult.Yes)
							{
								ViewModel.ReplaceCreatedFiles();

								if (ViewModel.HasNoMovedFiles)
								{
									MessageBox.Show("Не вдалось перемістити наступні файли. Можливо вони відкриті в іншій програмі.\n\n" + ViewModel.GetInfoNoMovedFiles(),
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

						ViewModel.RemoveTemplates();
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
				if (!(ViewModel.SelectedOpenedFile is DmxFile selectedFile && selectedFile.Loaded))
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
						ViewModel.TrimAllStrings();

						if (!ViewModel.Validate(out string errorText))
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

						ViewModel.Export(folderBrowserDialog.SelectedPath);
						isShowResult = true;
					}

					if (ViewModel.HasNoMovedFiles)
					{
						string startInfoNoMoved = ViewModel.GetInfoNoMovedFiles();
						if (MessageBox.Show("Файли за заданними путями вже існують.\n\n" + startInfoNoMoved + "\nЗамінити?",
											"DocumentMaker | Export",
											MessageBoxButtons.YesNo,
											MessageBoxIcon.Question)
												== System.Windows.Forms.DialogResult.Yes)
						{
							ViewModel.ReplaceCreatedFiles();

							if (ViewModel.HasNoMovedFiles)
							{
								string infoNoMoved = ViewModel.GetInfoNoMovedFiles();
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

					ViewModel.RemoveTemplates();
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
				ViewModel.OpenFiles(openFileDialog.FileNames);
				LoadFiles();
				SetSelectedFile(openFileDialog.FileNames.Last());
				ViewModel.ChangeOpenedFilesExtension();
			}
		}

		private void CloseFileClick(object sender, RoutedEventArgs e)
		{
			if (ViewModel.SelectedOpenedFile is DmxFile selectedFile && selectedFile.Loaded)
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
					ViewModel.CloseFile(selectedFile);
					int newIndex = OpenedFilesComboBox.Items.Count <= index ? index - 1 : index;
					if (newIndex < 0)
					{
						ViewModel.ClearUndoRedo();
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
					ViewModel.CloseFile(OpenedFilesList[0]);
				}
				OpenedFilesComboBox.SelectedIndex = -1;
				ViewModel.ClearUndoRedo();
				UpdateUndoRedoState();
			}
			else
				MessageBox.Show("Спочатку необхідно відкрити файл.",
								"DocumentMaker | Закриття файлу",
								MessageBoxButtons.OK,
								MessageBoxIcon.Information);
		}

		private async void InfoBtnClick(object sender, RoutedEventArgs e)
		{
			if (ViewModel.SelectedOpenedFile is DmxFile selectedFile && selectedFile.Loaded)
			{
				await DialogHost.Show(new HumanInformationDialog(ViewModel.GetSelectedHuman()));
			}
			else
				MessageBox.Show("Спочатку необхідно відкрити файл.",
								"DocumentMaker | Картка людини",
								MessageBoxButtons.OK,
								MessageBoxIcon.Information);
		}

		private void CorrectSaldoClick(object sender, RoutedEventArgs e)
		{
			bool isNeedUpdateSum = ViewModel.NeedUpdateSum;
			DisableUpdatingSum();
			SetDataToController();

			List<FullBackData> selectedBackDatas = new List<FullBackData>(GetSelectedBackDatas());
			IEnumerable<int> resultSums = ViewModel.CorrectSaldo(selectedBackDatas.Select(x => x.Controller));

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
					ViewModel.DisableActionsStacking();
					AddUndoRedoLinkNeedUpdateSum(isNeedUpdateSum);
				}
			}
			ViewModel.EnableActionsStacking();

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
				NumberText = ViewModel.CorrectDevelopmentWindow_NumberText,
				TakeSumFromSupport = ViewModel.CorrectDevelopmentWindow_TakeSumFromSupport,
				IsRemoveIdenticalNumbers = ViewModel.CorrectDevelopmentDialog_IsRemoveIdenticalNumbers,
			};
			await DialogHost.Show(dialog);

			ViewModel.CorrectDevelopmentWindow_NumberText = dialog.NumberText;
			ViewModel.CorrectDevelopmentWindow_TakeSumFromSupport = dialog.TakeSumFromSupport;
			ViewModel.CorrectDevelopmentDialog_IsRemoveIdenticalNumbers = dialog.IsRemoveIdenticalNumbers;

			if (dialog.IsCorrection && int.TryParse(dialog.NumberText, out int sum))
			{
				bool isNeedUpdateSum = ViewModel.NeedUpdateSum;
				DisableUpdatingSum();
				IEnumerable<int> resultSums = ViewModel.CorrectDevelopment(sum, dialog.TakeSumFromSupport, dialog.IsRemoveIdenticalNumbers);
				IEnumerator<FullBackDataController> backDataControllersEnum = ViewModel.BackDataControllers.GetEnumerator();
				IEnumerator<int> resultSumsEnum = resultSums.GetEnumerator();
				List<FullBackData> allFullBackDatas = new List<FullBackData>(GetAllFullBacksData());
				bool pushedFirst = false;
				ViewModel.DisableActionsStacking();
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
							ViewModel.AddUndoRedoLink(new UndoRedoLink(() =>
							{
								current.SumTextInput.Text = current.Controller.SumText;
							}));
							AddUndoRedoLinkNeedUpdateSum(isNeedUpdateSum);
						}
						current.SumTextInput.Text = resultSumsEnum.Current.ToString();
					}
				}
				ViewModel.EnableActionsStacking();

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
				NumberText = ViewModel.CorrectSupportWindow_NumberText,
				TakeSumFromDevelopment = ViewModel.CorrectSupportWindow_TakeSumFromDevelopment,
				IsCreateNewWorks = ViewModel.CorrectSupportDialog_IsCreateNewWorks,
				IsRemoveIdenticalNumbers = ViewModel.CorrectSupportDialog_IsRemoveIdenticalNumbers,
			};
			await DialogHost.Show(dialog);

			ViewModel.CorrectSupportWindow_NumberText = dialog.NumberText;
			ViewModel.CorrectSupportWindow_TakeSumFromDevelopment = dialog.TakeSumFromDevelopment;
			ViewModel.CorrectSupportDialog_IsCreateNewWorks = dialog.IsCreateNewWorks;
			ViewModel.CorrectSupportDialog_IsRemoveIdenticalNumbers = dialog.IsRemoveIdenticalNumbers;

			if (dialog.IsCorrection && int.TryParse(dialog.NumberText, out int sum))
			{
				bool isNeedUpdateSum = ViewModel.NeedUpdateSum;
				DisableUpdatingSum();
				IEnumerable<int> resultSums = ViewModel.CorrectSupport(sum, dialog.TakeSumFromDevelopment, dialog.IsCreateNewWorks, dialog.IsRemoveIdenticalNumbers, out List<KeyValuePair<FullBackDataController, int>> newControllers);
				IEnumerator<FullBackDataController> backDataControllersEnum = ViewModel.BackDataControllers.GetEnumerator();
				IEnumerator<int> resultSumsEnum = resultSums.GetEnumerator();
				List<FullBackData> allFullBackDatas = new List<FullBackData>(GetAllFullBacksData());
				bool pushedFirst = false;
				ViewModel.DisableActionsStacking();
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
							ViewModel.AddUndoRedoLink(new UndoRedoLink(() =>
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
				ViewModel.EnableActionsStacking();

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
				ViewModel.RemoveFromActionsStack(removedElems.Select(x => x.Controller));
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
				ViewModel.RemoveFromActionsStack(removedElems.Select(x => x.Controller));
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
				ViewModel.RemoveFromActionsStack(removedElems.Select(x => x.Controller));
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
				ViewModel.OpenFiles(filenames);
				LoadFiles();
				SetSelectedFile(filenames.Last(filename => filename.EndsWith(BaseDmxFile.Extension) || filename.EndsWith(DcmkFile.Extension)));
				ViewModel.ChangeOpenedFilesExtension();
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

			if (OpenedFilesComboBox != null && ViewModel.SelectedOpenedFile is DmxFile selectedFile && selectedFile.Loaded)
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
						saveFileDialog.FileName = ViewModel.GetDcmkFileName();
						res = saveFileDialog.ShowDialog();
						if (res == System.Windows.Forms.DialogResult.OK)
						{
							ViewModel.ExportDcmk(saveFileDialog.FileName);

							MessageBox.Show("Файл збережений.",
								"DocumentMaker | Export dcmk",
								MessageBoxButtons.OK,
								MessageBoxIcon.Information);
						}
					}

					if (res == System.Windows.Forms.DialogResult.Cancel)
					{
						cancelOpenedFilesSelectionChanged = true;
						ViewModel.SelectedOpenedFile = ViewModel.GetSelectedFile();
						return;
					}
				}

				ViewModel.ClearUndoRedo();
				UpdateUndoRedoState();

				FileContentGrid.Visibility = Visibility.Visible;
				ButtonOpenContent.Visibility = Visibility.Hidden;

				SetDataToController();
				ViewModel.SetDataFromFile(selectedFile);
				AddLoadedBackData();
				DataFooter?.UpdateAllSum();
				ReworkDataFooter?.UpdateAllSum();
				OtherDataFooter?.UpdateAllSum();
				SetDataFromController();
				ViewModel.SetSelectedFile(selectedFile);
				UpdateViewBackData();
				UpdateSaldo();

				ResetHaveUnsavedChanges();
			}
			else
			{
				FileContentGrid.Visibility = Visibility.Hidden;
				ButtonOpenContent.Visibility = Visibility.Visible;
			}
		}

		private void ActSumTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
		{
			#region Old logic (On dependency property value set)

			ViewModel.ActSum = ActSumInput.Text;
			if (OpenedFilesComboBox != null
				 && ViewModel.SelectedOpenedFile is DmxFile selectedFile)
			{
				selectedFile.ActSum = ActSumInput.Text;
			}

			#endregion

			HaveUnsavedChanges = true;
			if (ViewModel.IsActionsStackingEnabled && sender is System.Windows.Controls.TextBox textBox)
			{
				ViewModel.AddUndoRedoLink(new UndoRedoLink(() =>
				{
					textBox.Text = ViewModel.ActSum;
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
			ViewModel.TrimAllStrings();
			ViewModel.RandomizeWorkTypes(GetSelectedBackDatas(BacksData).Select(x => x.Controller));
			if (DataHeader != null) DataHeader.IsChecked = false;
			SetDataFromController();
			SetDataFromControllerBackDatas();
		}

		private void RandomizeReworkWorkTypes(object sender, RoutedEventArgs e)
		{
			HaveUnsavedChanges = true;
			ViewModel.TrimAllStrings();
			ViewModel.RandomizeReworkWorkTypes(GetSelectedBackDatas(ReworkBacksData).Select(x => x.Controller));
			if (ReworkDataHeader != null) ReworkDataHeader.IsChecked = false;
			SetDataFromController();
			SetDataFromControllerBackDatas();
		}

		private void ExportDcmkClick(object sender, RoutedEventArgs e)
		{
			if (ViewModel.SelectedOpenedFile is DmxFile selectedFile && selectedFile.Loaded)
			{
				saveFileDialog.FileName = ViewModel.GetDcmkFileName();
				if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					ViewModel.ExportDcmk(saveFileDialog.FileName);

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
					saveFileDialog.FileName = ViewModel.GetDcmkFileName();
					if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
					{
						ViewModel.ExportDcmk(saveFileDialog.FileName);

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
				TechnicalTaskDateText = TechnicalTaskDatePicker.Text,
				ActDateText = ActDatePicker.Text
			};
			await DialogHost.Show(dialog);
			if (dialog.IsChanging)
			{
				bool changed = ViewModel.ChangeTechnicalTaskDateAtAllFiles(dialog.TechnicalTaskDateText);
				changed = ViewModel.ChangeActDateAtAllFiles(dialog.ActDateText) || changed;
				if (changed)
				{
					DmxFile selectedFile = ViewModel.GetSelectedFile();
					if (selectedFile != null)
					{
						ViewModel.TechnicalTaskDateText = selectedFile.TechnicalTaskDateText;
						ViewModel.ActDateText = selectedFile.ActDateText;
						SetDataFromController();
					}
				}
			}
		}

		#endregion

		#region Methods

		private IEnumerable<FullBackData> DeleteSelectedBackData(StackPanel stackPanel)
		{
			DmxFile selectedFile = ViewModel.GetSelectedFile();
			List<UIElement> elems = new List<UIElement>();
			foreach (UIElement elem in stackPanel.Children)
			{
				if (elem is FullBackData backData)
				{
					if (backData.IsChecked.HasValue && backData.IsChecked.Value)
					{
						elems.Add(elem);
						backData.UnsubscribeAllEvents();
						ViewModel.BackDataControllers.Remove(backData.Controller);
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
			bool isNeedUpdateSum = ViewModel.NeedUpdateSum;
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
			bool actionsStackingEnable = ViewModel.IsActionsStackingEnabled;
			ViewModel.DisableActionsStacking();
			SetDataFromControllerBackDatas(BacksData);
			SetDataFromControllerBackDatas(ReworkBacksData);
			SetDataFromControllerBackDatas(OtherBacksData);
			if (actionsStackingEnable) ViewModel.EnableActionsStacking();
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
			IList<WorkObject> currentWorkTypesList = ViewModel.CurrentWorkTypesList,
				currentReworkWorkTypesList = ViewModel.CurrentReworkWorkTypesList;

			foreach (UIElement control in BacksData.Children)
			{
				if (control is FullBackData backData)
				{
					backData.SetViewByTemplate(ViewModel.TemplateType);
					backData.SetWorkTypesList(currentWorkTypesList);
					backData.SetGameNameList(ViewModel.GameNameList);
					backData.SetBackDataTypesList(ViewModel.CurrentBackDataTypesList);
				}
			}
			foreach (UIElement control in ReworkBacksData.Children)
			{
				if (control is FullBackData backData)
				{
					backData.SetViewByTemplate(ViewModel.TemplateType);
					backData.SetWorkTypesList(currentReworkWorkTypesList);
					backData.SetGameNameList(ViewModel.GameNameList);
					backData.SetBackDataTypesList(ViewModel.CurrentBackDataTypesList);
				}
			}
			foreach (UIElement control in OtherBacksData.Children)
			{
				if (control is FullBackData backData)
				{
					backData.SetViewByTemplate(ViewModel.TemplateType);
					backData.SetGameNameList(ViewModel.GameNameList);
					backData.SetBackDataTypesList(ViewModel.CurrentBackDataTypesList);
				}
			}

			DataHeader.SetViewByTemplate(ViewModel.TemplateType);
			ReworkDataHeader.SetViewByTemplate(ViewModel.TemplateType);
			OtherDataHeader.SetViewByTemplate(ViewModel.TemplateType);

			DataFooter.SetViewByTemplate(ViewModel.TemplateType);
			ReworkDataFooter.SetViewByTemplate(ViewModel.TemplateType);
			OtherDataFooter.SetViewByTemplate(ViewModel.TemplateType);
		}

		private void SetDataFromController()
		{
			bool actionsStackingEnable = ViewModel.IsActionsStackingEnabled;
			ViewModel.DisableActionsStacking();
			DocumentTemplateComboBox.SelectedIndex = (int)ViewModel.TemplateType;
			TechnicalTaskDatePicker.Text = ViewModel.TechnicalTaskDateText;
			ActDatePicker.Text = ViewModel.ActDateText;
			TechnicalTaskNumTextInput.Text = ViewModel.TechnicalTaskNumText;
			ActSumInput.Text = ViewModel.ActSum;
			ActSaldoInput.Text = ViewModel.ActSaldo;
			if (actionsStackingEnable) ViewModel.EnableActionsStacking();
		}

		private void SetDataToController()
		{
			bool actionsStackingEnable = ViewModel.IsActionsStackingEnabled;
			ViewModel.DisableActionsStacking();
			ViewModel.TechnicalTaskDateText = TechnicalTaskDatePicker.Text;
			ViewModel.ActDateText = ActDatePicker.Text;
			ViewModel.TechnicalTaskNumText = TechnicalTaskNumTextInput.Text;
			ViewModel.ActSum = ActSumInput.Text;
			ViewModel.ActSaldo = ActSaldoInput.Text;
			if (actionsStackingEnable) ViewModel.EnableActionsStacking();
		}

		private void LoadFiles()
		{
			ViewModel.LoadFiles();
			SetSelectedFile(ViewModel.GetSelectedFile()?.FullName);
		}

		private void AddLoadedBackData()
		{
			bool actionsStackingEnable = ViewModel.IsActionsStackingEnabled;
			ViewModel.DisableActionsStacking();

			DataFooter.ClearData();
			ReworkDataFooter.ClearData();
			OtherDataFooter.ClearData();
			foreach (FullBackDataController backDataController in ViewModel.BackDataControllers)
			{
				backDataController.SetActionsStack(ViewModel.GetActionsStack());
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

			if (actionsStackingEnable) ViewModel.EnableActionsStacking();
		}

		private IEnumerable<FullBackData> AddNewSupport(IEnumerable<FullBackDataController> controllers)
		{
			bool actionsStackingEnable = ViewModel.IsActionsStackingEnabled;
			ViewModel.DisableActionsStacking();

			DmxFile selectedFile = ViewModel.GetSelectedFile();
			if (selectedFile != null)
			{
				selectedFile.AddRangeBackModel(controllers.Select(x => x.GetModel()));
			}

			List<FullBackData> addedNewSupport = new List<FullBackData>();
			foreach (FullBackDataController backDataController in controllers)
			{
				backDataController.SetActionsStack(ViewModel.GetActionsStack());
				if (!backDataController.IsOtherType && backDataController.IsRework)
				{
					ViewModel.BackDataControllers.Add(backDataController);
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

			if (actionsStackingEnable) ViewModel.EnableActionsStacking();

			return addedNewSupport;
		}

		private void SetSelectedFile(string filename)
		{
			foreach (DmxFile file in OpenedFilesList)
			{
				if (Path.ChangeExtension(file.FullName, null) == Path.ChangeExtension(filename, null) || Path.ChangeExtension(file.Name, null) == Path.ChangeExtension(filename, null))
				{
					ViewModel.SelectedOpenedFile = file;
					break;
				}
			}
		}

		private void SetWindowSettingsFromController()
		{
			Top = ViewModel.WindowTop;
			Left = ViewModel.WindowLeft;
			Height = ViewModel.WindowHeight;
			Width = ViewModel.WindowWidth;
			WindowValidator.MoveToValidPosition(this);
		}

		private void UpdateActSum()
		{
			bool actionsStackingEnable = ViewModel.IsActionsStackingEnabled;
			if (ViewModel.NeedUpdateSum) ViewModel.DisableActionsStacking();

			uint sum = uint.TryParse(ActSumInput.Text, out uint s) ? s : 0;
			UpdateActSumBackDataPanel(BacksData, sum);
			UpdateActSumBackDataPanel(ReworkBacksData, sum);
			UpdateActSumBackDataPanel(OtherBacksData, sum);
			UpdateSaldo();

			if (ViewModel.NeedUpdateSum)
			{
				DropSaldoToLast();

				if (actionsStackingEnable) ViewModel.EnableActionsStacking();
			}
		}

		private void DropSaldoToLast()
		{
			if (!int.TryParse(ActSaldoInput.Text, out int currentSaldo)) return;

			FullBackDataController last = ViewModel.BackDataControllers.LastOrDefault();
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
						backData.SetActSum(sum, ViewModel.NeedUpdateSum);
					}
				}
			}
		}

		private void UpdateSaldo()
		{
			UpdateActSumSelected();
			uint sum = 0;
			if (uint.TryParse(ActSumInput.Text, out uint s))
			{
				sum = s;
			}

			if (uint.TryParse(DataFooter.AllSum, out uint curSum)
				&& uint.TryParse(ReworkDataFooter.AllSum, out uint curSumRework)
				&& uint.TryParse(OtherDataFooter.AllSum, out uint curSumOther))
			{
				ActSaldoInput.Text = ((int)sum - (int)(curSum + curSumRework + curSumOther)).ToString();
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

			List<string> notLoadedFilesList = ViewModel.GetNotLoadedFilesList();
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
				ViewModel.ResetWeights();
			}
		}

		private void DisableUpdatingSum()
		{
			SetNeedUpdateSumState(false);
		}

		private void SetNeedUpdateSumState(bool state)
		{
			ViewModel.NeedUpdateSum = state;
			DmxFile selectedFile = ViewModel.GetSelectedFile();
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
			ViewModel.AddUndoRedoLink(new UndoRedoLink(
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
			if (ViewModel.IsActionsStackingEnabled)
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
			ViewModel.DisableActionsStacking();
			ViewModel.Redo();
			ViewModel.EnableActionsStacking();
			UpdateUndoRedoState();
		}

		private void Undo()
		{
			ViewModel.DisableActionsStacking();
			ViewModel.Undo();
			ViewModel.EnableActionsStacking();
			UpdateUndoRedoState();
		}

		private void UpdateUndoRedoState()
		{
			MenuUndoButton.IsEnabled = ViewModel.CanUndo;
			ToolBarUndoButton.IsEnabled = ViewModel.CanUndo;
			MenuRedoButton.IsEnabled = ViewModel.CanRedo;
			ToolBarRedoButton.IsEnabled = ViewModel.CanRedo;
		}

		private bool HaveUnsavedChangesAtAll()
		{
			return ViewModel.HaveUnsavedChangesAtAll();
		}

		private void ResetHaveUnsavedChanges()
		{
			ViewModel.ResetHaveUnsavedChanges();
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
					saveFileDialog.FileName = ViewModel.GetDcmkFileName();
					dialogResult = saveFileDialog.ShowDialog();
					if (dialogResult == System.Windows.Forms.DialogResult.OK)
					{
						ViewModel.ExportDcmk(saveFileDialog.FileName);

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
			backData.Controller.SetActionsStack(ViewModel.GetActionsStack());
			ViewModel.BackDataControllers.Add(backData.Controller);
			backData.SetViewByTemplate(ViewModel.TemplateType);
			if (!isOtherType)
			{
				backData.SetWorkTypesList(isRework ? ViewModel.CurrentReworkWorkTypesList : ViewModel.CurrentWorkTypesList);
			}
			backData.SetBackDataTypesList(ViewModel.CurrentBackDataTypesList);
			backData.SetGameNameList(ViewModel.GameNameList);
			FullBackDataHeader header = isOtherType ? OtherDataHeader : (isRework ? ReworkDataHeader : DataHeader);
			backData.SubscribeSelectionChanged(() =>
			{
				header.UpdateIsCheckedState();
				UpdateActSumSelected();
			});
			UpdateActSum();
			header.UpdateIsCheckedState();

			DmxFile selectedFile = ViewModel.GetSelectedFile();
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
				AddUndoRedoLinkNeedUpdateSumWithCheck(ViewModel.NeedUpdateSum);
				DisableUpdatingSum();
			}
			UpdateSaldo();
		}

		#endregion

		#region Old logic (On dependency property value set)

		private void TechnicalTaskDatePicker_TextChanged(object sender, TextChangedEventArgs e)
		{
			ViewModel.TechnicalTaskDateText = TechnicalTaskDatePicker.Text;
			if (OpenedFilesComboBox != null && ViewModel.SelectedOpenedFile is DmxFile selectedFile)
			{
				selectedFile.TechnicalTaskDateText = TechnicalTaskDatePicker.Text;
			}
		}

		private void ActDatePicker_TextChanged(object sender, TextChangedEventArgs e)
		{
			ViewModel.ActDateText = ActDatePicker.Text;
			if (OpenedFilesComboBox != null && ViewModel.SelectedOpenedFile is DmxFile selectedFile)
			{
				selectedFile.ActDateText = ActDatePicker.Text;
			}
		}

		private void TechnicalTaskNumTextInput_TextChanged(object sender, TextChangedEventArgs e)
		{
			ViewModel.TechnicalTaskNumText = TechnicalTaskNumTextInput.Text;
			if (OpenedFilesComboBox != null && ViewModel.SelectedOpenedFile is DmxFile selectedFile)
			{
				selectedFile.TechnicalTaskNumText = TechnicalTaskNumTextInput.Text;
			}
		}

		#endregion
	}
}
