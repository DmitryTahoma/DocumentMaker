using ActGenerator.Model;
using ActGenerator.ViewModel.Controls;
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
using System.Windows;

namespace ActGenerator.ViewModel
{
	class ActGeneratorViewModel : DependencyObject, ICryptedConnectionStringRequired
	{
		ActGeneratorModel model = new ActGeneratorModel();

		public ActGeneratorViewModel()
		{
			InitCommands();

			State = ViewModelState.Initialized;
		}

		#region Properties

		public string DialogHostId { get; } = "ActGeneratorDialogHost";

		public ObservableRangeCollection<HumanDataContext> HumanList { get; private set; } = new ObservableRangeCollection<HumanDataContext>();

		public ObservableRangeCollection<DocumentTemplate> DocumentTemplates { get; private set; } = new ObservableRangeCollection<DocumentTemplate> 
		{
			new DocumentTemplate("Скриптувальник", DocumentTemplateType.Scripter),
			new DocumentTemplate("Технічний дизайнер", DocumentTemplateType.Cutter),
			new DocumentTemplate("Художник", DocumentTemplateType.Painter),
			new DocumentTemplate("Моделлер", DocumentTemplateType.Modeller),
		};

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

		#endregion

		#region Commands

		private void InitCommands()
		{
			CloseOpenedDialog = new Command(OnCloseOpenedDialogExecute);
			LoadFromDatabase = new Command(OnLoadFromDatabaseExecute);
			GenerateActs = new Command<DependencyObject>(OnGenerateActsExecute);
			BindDialogHostName = new Command<IContainDialogHostId>(OnBindDialogHostNameExecute);
			SendCryptedConnectionString = new Command<ICryptedConnectionStringRequired>(OnSendCryptedConnectionStringExecute);
		}

		public Command CloseOpenedDialog { get; private set; }
		private void OnCloseOpenedDialogExecute()
		{
			if(DialogHost.IsDialogOpen(DialogHostId))
			{
				DialogHost.Close(DialogHostId);
			}
		}

		public Command LoadFromDatabase { get; private set; }
		private void OnLoadFromDatabaseExecute()
		{

		}

		public Command<DependencyObject> GenerateActs { get; private set; }
		private void OnGenerateActsExecute(DependencyObject validateObj)
		{
			if (ValidationHelper.GetFirstInvalid(validateObj, true) is UIElement invalid)
			{
				invalid.Focus();
				return;
			}
		}

		public Command<IContainDialogHostId> BindDialogHostName { get; private set; }
		private void OnBindDialogHostNameExecute(IContainDialogHostId dialogHostIdContainer)
		{
			dialogHostIdContainer.DialogHostId = DialogHostId;
		}

		public Command<ICryptedConnectionStringRequired> SendCryptedConnectionString { get; private set; }
		private void OnSendCryptedConnectionStringExecute(ICryptedConnectionStringRequired connectionStringRequired)
		{
			if (model.ConnectionStringSetted)
			{
				connectionStringRequired.SetCryptedConnectionString(model.ConnectionString);
			}
		}

		#endregion

		#region Methods

		public void LoadFromSession(ActGeneratorSession actGeneratorSession)
		{
			MinSumText = actGeneratorSession.MinSumText;
			MaxSumText = actGeneratorSession.MaxSumText;
			NotUseOldWorks = actGeneratorSession.NotUseOldWorks;
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
