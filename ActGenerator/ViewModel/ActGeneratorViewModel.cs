using ActGenerator.Model;
using ActGenerator.Model.Controls;
using ActGenerator.View.Controls;
using ActGenerator.View.Dialogs;
using ActGenerator.ViewModel.Controls;
using ActGenerator.ViewModel.Dialogs;
using ActGenerator.ViewModel.Interfaces;
using Dml;
using Dml.Model.Template;
using DocumentMaker.Security;
using MaterialDesignThemes.Wpf;
using Mvvm;
using Mvvm.Commands;
using ProjectEditorLib.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;

namespace ActGenerator.ViewModel
{
	class ActGeneratorViewModel : DependencyObject, ICryptedConnectionStringRequired, IContainDialogHostId
	{
		ActGeneratorModel model = new ActGeneratorModel();

		GenerationDialog generationDialog = new GenerationDialog();
		GenerationDialogViewModel generationDialogViewModel = null;
		Snackbar snackbar = null;
		TextBlock snackbarTextBlock = new TextBlock { HorizontalAlignment = HorizontalAlignment.Center };

		ProjectNamesListControlViewModel projectNamesListControlViewModel = null;
		HumenListControlViewModel humenListControlViewModel = null;
		DocumentListControlViewModel documentListControlViewModel = null;

		DateTimeItem savedSelectedDateTimeItem = null;

		public ActGeneratorViewModel()
		{
			generationDialogViewModel = (GenerationDialogViewModel)generationDialog.DataContext;
			generationDialogViewModel.AddToActListCommand = new Command(AddGeneratedActsToList);

			InitCommands();

			State = ViewModelState.Initialized;
		}

		#region Properties

		public string InnerDialogHostId { get; } = "ActGeneratorDialogHost";

		public string DialogHostId { get; set; }

		public bool CloseOnClickAwayDialogHost
		{
			get { return (bool)GetValue(CloseOnClickAwayDialogHostProperty); }
			set { SetValue(CloseOnClickAwayDialogHostProperty, value); }
		}
		public static readonly DependencyProperty CloseOnClickAwayDialogHostProperty = DependencyProperty.Register(nameof(CloseOnClickAwayDialogHost), typeof(bool), typeof(ActGeneratorViewModel));

		public string MinSumText
		{
			get { return (string)GetValue(MinSumTextProperty); }
			set { SetValue(MinSumTextProperty, value); }
		}
		public static readonly DependencyProperty MinSumTextProperty = DependencyProperty.Register(nameof(MinSumText), typeof(string), typeof(ActGeneratorViewModel));

		public string MaxSumText
		{
			get { return (string)GetValue(MaxSumTextProperty); }
			set { SetValue(MaxSumTextProperty, value); }
		}
		public static readonly DependencyProperty MaxSumTextProperty = DependencyProperty.Register(nameof(MaxSumText), typeof(string), typeof(ActGeneratorViewModel));

		public ViewModelState State
		{
			get { return (ViewModelState)GetValue(StateProperty); }
			set { SetValue(StateProperty, value); }
		}
		public static readonly DependencyProperty StateProperty = DependencyProperty.Register(nameof(State), typeof(ViewModelState), typeof(ActGeneratorViewModel), new PropertyMetadata(), CommandHelper.UpdateAllCanExecute);

		public DateTime? TechnicalTaskDate
		{
			get { return (DateTime?)GetValue(TechnicalTaskDateProperty); }
			set { SetValue(TechnicalTaskDateProperty, value); }
		}
		public static readonly DependencyProperty TechnicalTaskDateProperty = DependencyProperty.Register(nameof(TechnicalTaskDate), typeof(DateTime?), typeof(ActGeneratorViewModel));

		public string MinActDateString
		{
			get { return (string)GetValue(MinActDateStringProperty); }
			set { SetValue(MinActDateStringProperty, value); }
		}
		public static readonly DependencyProperty MinActDateStringProperty = DependencyProperty.Register(nameof(MinActDateString), typeof(string), typeof(ActGeneratorViewModel));

		public DateTime? ActDate
		{
			get { return (DateTime?)GetValue(ActDateProperty); }
			set { SetValue(ActDateProperty, value); }
		}
		public static readonly DependencyProperty ActDateProperty = DependencyProperty.Register(nameof(ActDate), typeof(DateTime?), typeof(ActGeneratorViewModel));

		public string MaxActDateString
		{
			get { return (string)GetValue(MaxActDateStringProperty); }
			set { SetValue(MaxActDateStringProperty, value); }
		}
		public static readonly DependencyProperty MaxActDateStringProperty = DependencyProperty.Register(nameof(MaxActDateString), typeof(string), typeof(ActGeneratorViewModel));

		public bool CollapseRegionsWorks
		{
			get { return (bool)GetValue(CollapseRegionsWorksProperty); }
			set { SetValue(CollapseRegionsWorksProperty, value); }
		}
		public static readonly DependencyProperty CollapseRegionsWorksProperty = DependencyProperty.Register(nameof(CollapseRegionsWorks), typeof(bool), typeof(ActGeneratorViewModel));

		public XmlLanguage Language
		{
			get { return (XmlLanguage)GetValue(LanguageProperty); }
			set { SetValue(LanguageProperty, value); }
		}
		public static readonly DependencyProperty LanguageProperty = DependencyProperty.Register(nameof(Language), typeof(XmlLanguage), typeof(ActGeneratorViewModel), new PropertyMetadata(XmlLanguage.GetLanguage("uk")));

		#endregion

		#region Commands

		private void InitCommands()
		{
			CloseOpenedDialog = new Command(OnCloseOpenedDialogExecute);
			ActGeneratorLoaded = new Command(OnActGeneratorLoadedExecute);
			GenerateActs = new Command<DependencyObject>(OnGenerateActsExecute);
			BindDialogHostName = new Command<IContainDialogHostId>(OnBindDialogHostNameExecute);
			SendCryptedConnectionString = new Command<ICryptedConnectionStringRequired>(OnSendCryptedConnectionStringExecute);
			BindProjectNamesListControl = new Command<ProjectNamesListControlViewModel>(OnBindProjectNamesListControlExecute);
			BindHumenListControl = new Command<HumenListControlViewModel>(OnBindHumenListControlExecute);
			BindDocumentListControl = new Command<DocumentListControlViewModel>(OnBindDocumentListControlExecute);
			BindSnackbar = new Command<Snackbar>(OnBindSnackbarExecute);
			ClearKeyboardFocusOnEnter = new Command<KeyEventArgs>(OnClearKeyboardFocusOnEnterExecute);
		}

		public Command CloseOpenedDialog { get; private set; }
		private void OnCloseOpenedDialogExecute()
		{
			if(DialogHost.IsDialogOpen(InnerDialogHostId))
			{
				DialogHost.Close(InnerDialogHostId);
			}
		}

		public Command ActGeneratorLoaded { get; private set; }
		private void OnActGeneratorLoadedExecute()
		{

		}

		public Command<DependencyObject> GenerateActs { get; private set; }
		private async void OnGenerateActsExecute(DependencyObject validateObj)
		{
			if (ValidationHelper.GetFirstInvalid(validateObj, true) is UIElement invalid)
			{
				if (invalid is FrameworkElement frameworkElement) frameworkElement.BringIntoView();
				invalid.Focus();
				return;
			}
			else if(!projectNamesListControlViewModel.IsSelectedProject())
			{
				await SendSnackbarMessage("Необрані проєкти", 3);
				return;
			}
			else if(!humenListControlViewModel.IsSelectedHuman())
			{
				await SendSnackbarMessage("Необрані працівники", 3);
				return;
			}

			List<HumanListItemControlModel> humen = humenListControlViewModel.GetHumen().ToList();
			int minSum = int.Parse(MinSumText), maxSum = int.Parse(MaxSumText);
			model.SetSumLimits(minSum, maxSum);

			model.SetProjects(projectNamesListControlViewModel.SelectedDbProjects);
			model.SetHumen(humen);
			model.SetIgnoringActDate(documentListControlViewModel.SelectedDateTimeItem.DateTime);
			model.SetDocumentList(documentListControlViewModel.GetDocumentList());
			model.SetIsCollapseRegionsWorks(CollapseRegionsWorks);
			model.SetDates(TechnicalTaskDate.Value, ActDate.Value);

			foreach (HumanListItemControlModel human in humen)
			{
				if (model.IsReadyToGeneration(human))
				{
					int countEnableWorks = model.GetCountEnabledWorks(human.SelectedTemplates);
					int sum = (int)human.Sum;
					int minCountWorks = model.GetMinCountWorks(sum);
					if (countEnableWorks <= 0)
					{
						MessageBox.Show("На задану суму акту \"" + human.HumanData.Name + "\" недостатня кількість робіт.", "Змініть налаштування сум", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
						return;
					}
					else if (countEnableWorks < minCountWorks)
					{
						int targetWorkCost = sum / countEnableWorks + 100;
						if (MessageBoxResult.No == MessageBox.Show("На задану суму акту \"" + human.HumanData.Name + "\" недостатня кількість робіт. Змінити макимум суми однієї роботи на " + targetWorkCost + " гривень?", "Змініть налаштування сум", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No))
						{
							return;
						}
						else
						{
							maxSum = targetWorkCost;
							MaxSumText = maxSum.ToString();
							model.SetSumLimits(minSum, maxSum);
						}
					}
				}
			}

			if (!model.ContainsHumanDatas())
			{
				model.SetHumanDatas(humenListControlViewModel.GetAllHumanDatas());
			}

			CloseOnClickAwayDialogHost = false;
			generationDialogViewModel.DialogHostId = DialogHostId;
			Task dialogTask = DialogHost.Show(generationDialog, DialogHostId);
			_ = Task.Run(async() =>
			{
				while (!generationDialogViewModel.Dispatcher.Invoke(() => generationDialogViewModel.IsClosing) && !generationDialogViewModel.Dispatcher.Invoke(() => generationDialogViewModel.GenerationStarted))
				{
					await Task.Delay(1);
				}

				if (generationDialogViewModel.Dispatcher.Invoke(() => generationDialogViewModel.GenerationStarted))
				{
					model.SetSavingPath(generationDialogViewModel.Dispatcher.Invoke(() => generationDialogViewModel.SelectedFolderPath));

					await model.StartGeneration(generationDialogViewModel);
				}

				generationDialogViewModel.Dispatcher.Invoke(() =>
				{
					if (!generationDialogViewModel.IsClosing)
						generationDialogViewModel.GenerationSuccessed = true;

					List<string> names = model.GetGeneratedActNames();
					generationDialogViewModel.CanAddGeneratedActs = names != null && names.Count > 0;
				});
			});
			await dialogTask;
			CloseOnClickAwayDialogHost = true;
		}

		public Command<IContainDialogHostId> BindDialogHostName { get; private set; }
		private void OnBindDialogHostNameExecute(IContainDialogHostId dialogHostIdContainer)
		{
			dialogHostIdContainer.DialogHostId = InnerDialogHostId;
		}

		public Command<ICryptedConnectionStringRequired> SendCryptedConnectionString { get; private set; }
		private void OnSendCryptedConnectionStringExecute(ICryptedConnectionStringRequired connectionStringRequired)
		{
			if (model.ConnectionStringSetted)
			{
				connectionStringRequired.SetCryptedConnectionString(model.ConnectionString);
			}
		}

		public Command<ProjectNamesListControlViewModel> BindProjectNamesListControl{ get; private set; }
		private void OnBindProjectNamesListControlExecute(ProjectNamesListControlViewModel projectNamesListControlViewModel)
		{
			if(this.projectNamesListControlViewModel == null)
			{
				this.projectNamesListControlViewModel = projectNamesListControlViewModel;
			}
		}

		public Command<HumenListControlViewModel> BindHumenListControl { get; private set; }
		private void OnBindHumenListControlExecute(HumenListControlViewModel humenListControlViewModel)
		{
			if(this.humenListControlViewModel == null)
			{
				this.humenListControlViewModel = humenListControlViewModel;
			}
		}

		public Command<DocumentListControlViewModel> BindDocumentListControl { get; private set; }
		private void OnBindDocumentListControlExecute(DocumentListControlViewModel documentListControlViewModel)
		{
			if(this.documentListControlViewModel == null)
			{
				this.documentListControlViewModel = documentListControlViewModel;

				documentListControlViewModel.SelectedDateTimeItem = documentListControlViewModel.DateTimeItems?.FirstOrDefault(x => x.DateTime == savedSelectedDateTimeItem?.DateTime)
					?? documentListControlViewModel.DateTimeItems?.FirstOrDefault();
			}
		}

		public Command<Snackbar> BindSnackbar { get; private set; }
		private void OnBindSnackbarExecute(Snackbar snackbar)
		{
			if(this.snackbar == null)
			{
				this.snackbar = snackbar;
			}
		}

		public Command<KeyEventArgs> ClearKeyboardFocusOnEnter { get; private set; }
		private void OnClearKeyboardFocusOnEnterExecute(KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				Keyboard.ClearFocus();
			}
		}

		#endregion

		#region Methods

		public void LoadFromSession(ActGeneratorSession actGeneratorSession)
		{
			MinSumText = actGeneratorSession.MinSumText;
			MaxSumText = actGeneratorSession.MaxSumText;
			TechnicalTaskDate = actGeneratorSession.TechnicalTaskDate;
			ActDate = actGeneratorSession.ActDate;
			CollapseRegionsWorks = actGeneratorSession.CollapseRegionsWorks;
			savedSelectedDateTimeItem = actGeneratorSession.SelectedDateTimeItem;
		}

		public void SetCryptedConnectionString(CryptedConnectionString cryptedConnectionString)
		{
			model.ConnectionString = cryptedConnectionString;
		}

		private async Task SendSnackbarMessage(string message, double durationSeconds)
		{
			bool waitClosing = snackbar.MessageQueue.QueuedMessages.Count != 0;

			snackbar.MessageQueue.Clear();
			if (waitClosing) await Task.Delay(300);

			snackbarTextBlock.Text = message;
			snackbar.Width = snackbarTextBlock.Text.Length * 8;
			snackbar.MessageQueue.Enqueue(snackbarTextBlock, null, null, null, false, true, TimeSpan.FromSeconds(durationSeconds));
		}

		private async void AddGeneratedActsToList()
		{
			foreach(string path in model.GetGeneratedActNames())
			{
				documentListControlViewModel.LoadAct(path);
				await Task.Delay(1);
			}
		}

		public DateTimeItem GetSelectedDateTimeItem()
		{
			return documentListControlViewModel.SelectedDateTimeItem;
		}

		#endregion
	}
}
