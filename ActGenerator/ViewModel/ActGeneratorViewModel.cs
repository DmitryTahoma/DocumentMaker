using ActGenerator.Model;
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
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ActGenerator.ViewModel
{
	class ActGeneratorViewModel : DependencyObject, ICryptedConnectionStringRequired, IContainDialogHostId
	{
		ActGeneratorModel model = new ActGeneratorModel();

		GenerationDialog generationDialog = new GenerationDialog();
		GenerationDialogViewModel generationDialogViewModel = null;

		ProjectNamesListControlViewModel projectNamesListControlViewModel = null;
		HumenListControlViewModel humenListControlViewModel = null;

		public ActGeneratorViewModel()
		{
			generationDialogViewModel = (GenerationDialogViewModel)generationDialog.DataContext;

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

		public ObservableRangeCollection<DateTimeItem> DateTimeItems { get; private set; } = new ObservableRangeCollection<DateTimeItem>()
		{
			new DateTimeItem{ Text = "тиждень", DateTime = new DateTime(1, 1, 8) },
			new DateTimeItem{ Text = "місяць", DateTime = new DateTime(1, 2, 1) },
			new DateTimeItem{ Text = "3 місяці", DateTime = new DateTime(1, 4, 1) },
			new DateTimeItem{ Text = "пів року", DateTime = new DateTime(1, 7, 1) },
			new DateTimeItem{ Text = "рік", DateTime = new DateTime(2, 1, 1) },
		};

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

		public bool NotUseOldWorks
		{
			get { return (bool)GetValue(NotUseOldWorksProperty); }
			set { SetValue(NotUseOldWorksProperty, value); }
		}
		public static readonly DependencyProperty NotUseOldWorksProperty = DependencyProperty.Register(nameof(NotUseOldWorks), typeof(bool), typeof(ActGeneratorViewModel));

		public DateTimeItem SelectedDateTimeItem
		{
			get { return (DateTimeItem)GetValue(SelectedDateTimeItemProperty); }
			set { SetValue(SelectedDateTimeItemProperty, value); }
		}
		public static readonly DependencyProperty SelectedDateTimeItemProperty = DependencyProperty.Register(nameof(SelectedDateTimeItem), typeof(DateTimeItem), typeof(ActGeneratorViewModel));

		public ViewModelState State
		{
			get { return (ViewModelState)GetValue(StateProperty); }
			set { SetValue(StateProperty, value); }
		}
		public static readonly DependencyProperty StateProperty = DependencyProperty.Register(nameof(State), typeof(ViewModelState), typeof(ActGeneratorViewModel), new PropertyMetadata(), CommandHelper.UpdateAllCanExecute);

		public DateTime? MinActDate
		{
			get { return (DateTime?)GetValue(MinActDateProperty); }
			set { SetValue(MinActDateProperty, value); }
		}
		public static readonly DependencyProperty MinActDateProperty = DependencyProperty.Register(nameof(MinActDate), typeof(DateTime?), typeof(ActGeneratorViewModel));

		public string MinActDateString
		{
			get { return (string)GetValue(MinActDateStringProperty); }
			set { SetValue(MinActDateStringProperty, value); }
		}
		public static readonly DependencyProperty MinActDateStringProperty = DependencyProperty.Register(nameof(MinActDateString), typeof(string), typeof(ActGeneratorViewModel));

		public DateTime? MaxActDate
		{
			get { return (DateTime?)GetValue(MaxActDateProperty); }
			set { SetValue(MaxActDateProperty, value); }
		}
		public static readonly DependencyProperty MaxActDateProperty = DependencyProperty.Register(nameof(MaxActDate), typeof(DateTime?), typeof(ActGeneratorViewModel));

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
			model.SetTemplates(humenListControlViewModel.GetDocumentTemplatesList());
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

			CloseOnClickAwayDialogHost = false;
			generationDialogViewModel.DialogHostId = DialogHostId;
			Task dialogTask = DialogHost.Show(generationDialog, DialogHostId);
			model.SetProjects(projectNamesListControlViewModel.SelectedDbProjects);
			model.SetHumen(humenListControlViewModel.GetHumen());
			_ = Task.Run(async() => { await model.StartGeneration(generationDialogViewModel); });
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

		#endregion

		#region Methods

		public void LoadFromSession(ActGeneratorSession actGeneratorSession)
		{
			MinSumText = actGeneratorSession.MinSumText;
			MaxSumText = actGeneratorSession.MaxSumText;
			MinActDate = actGeneratorSession.MinActDate;
			MaxActDate = actGeneratorSession.MaxActDate;
			NotUseOldWorks = actGeneratorSession.NotUseOldWorks;
			CollapseRegionsWorks = actGeneratorSession.CollapseRegionsWorks;
			SelectedDateTimeItem = DateTimeItems?.FirstOrDefault(x => x.DateTime == actGeneratorSession.SelectedDateTimeItem?.DateTime)
				?? DateTimeItems?.FirstOrDefault();
		}

		public void SetCryptedConnectionString(CryptedConnectionString cryptedConnectionString)
		{
			model.ConnectionString = cryptedConnectionString;
		}

		#endregion
	}
}
