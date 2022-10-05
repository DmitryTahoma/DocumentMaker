using Db.Context.HumanPart;
using HumanEditorLib.Model;
using MaterialDesignThemes.Wpf;
using Mvvm;
using Mvvm.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace HumanEditorLib.ViewModel
{
	public class HumanEditViewModel : DependencyObject
	{
		HumanEditModel model = new HumanEditModel();
		ViewModelState state = ViewModelState.Initialized;

		Snackbar snackbar = null;

		public HumanEditViewModel()
		{
			InitCommands();
		}

		#region Properties

		public bool IsEditionMode
		{
			get { return (bool)GetValue(IsEditionModeProperty); }
			set { SetValue(IsEditionModeProperty, value); }
		}
		public static readonly DependencyProperty IsEditionModeProperty = DependencyProperty.Register(nameof(IsEditionMode), typeof(bool), typeof(HumanEditViewModel));

		public bool ModeSelected
		{
			get { return (bool)GetValue(ModeSelectedProperty); }
			set { SetValue(ModeSelectedProperty, value); }
		}
		public static readonly DependencyProperty ModeSelectedProperty = DependencyProperty.Register(nameof(ModeSelected), typeof(bool), typeof(HumanEditViewModel));

		public ObservableRangeCollection<Human> HumanList { get; private set; } = new ObservableRangeCollection<Human>();

		public Human SelectedEditHuman
		{
			get { return (Human)GetValue(SelectedEditHumanProperty); }
			set { SetValue(SelectedEditHumanProperty, value); }
		}
		public static readonly DependencyProperty SelectedEditHumanProperty = DependencyProperty.Register(nameof(SelectedEditHuman), typeof(Human), typeof(HumanEditViewModel));

		public string Surname
		{
			get { return (string)GetValue(SurnameProperty); }
			set { SetValue(SurnameProperty, value); }
		}
		public static readonly DependencyProperty SurnameProperty = DependencyProperty.Register(nameof(Surname), typeof(string), typeof(HumanEditViewModel));

		public string Name
		{
			get { return (string)GetValue(NameProperty); }
			set { SetValue(NameProperty, value); }
		}
		public static readonly DependencyProperty NameProperty = DependencyProperty.Register(nameof(Name), typeof(string), typeof(HumanEditViewModel));

		public string SecondName
		{
			get { return (string)GetValue(SecondNameProperty); }
			set { SetValue(SecondNameProperty, value); }
		}
		public static readonly DependencyProperty SecondNameProperty = DependencyProperty.Register(nameof(SecondName), typeof(string), typeof(HumanEditViewModel));

		public long TIN
		{
			get { return (long)GetValue(TINProperty); }
			set { SetValue(TINProperty, value); }
		}
		public static readonly DependencyProperty TINProperty = DependencyProperty.Register(nameof(TIN), typeof(long), typeof(HumanEditViewModel));

		public ObservableRangeCollection<LocalityType> LocalityTypesList { get; private set; } = new ObservableRangeCollection<LocalityType>();

		public LocalityType SelectedLocalityType
		{
			get { return (LocalityType)GetValue(SelectedLocalityTypeProperty); }
			set { SetValue(SelectedLocalityTypeProperty, value); }
		}
		public static readonly DependencyProperty SelectedLocalityTypeProperty =DependencyProperty.Register(nameof(SelectedLocalityType), typeof(LocalityType), typeof(HumanEditViewModel));

		public string LocalityName
		{
			get { return (string)GetValue(LocalityNameProperty); }
			set { SetValue(LocalityNameProperty, value); }
		}
		public static readonly DependencyProperty LocalityNameProperty =DependencyProperty.Register(nameof(LocalityName), typeof(string), typeof(HumanEditViewModel));

		public ObservableRangeCollection<StreetType> StreetTypesList { get; private set; } = new ObservableRangeCollection<StreetType>();

		public StreetType SelectedStreetType
		{
			get { return (StreetType)GetValue(SelectedStreetTypeProperty); }
			set { SetValue(SelectedStreetTypeProperty, value); }
		}
		public static readonly DependencyProperty SelectedStreetTypeProperty = DependencyProperty.Register(nameof(SelectedStreetType), typeof(StreetType), typeof(HumanEditViewModel));

		public string StreetName
		{
			get { return (string)GetValue(StreetNameProperty); }
			set { SetValue(StreetNameProperty, value); }
		}
		public static readonly DependencyProperty StreetNameProperty = DependencyProperty.Register(nameof(StreetName), typeof(string), typeof(HumanEditViewModel));

		public string HouseNumber
		{
			get { return (string)GetValue(HouseNumberProperty); }
			set { SetValue(HouseNumberProperty, value); }
		}
		public static readonly DependencyProperty HouseNumberProperty = DependencyProperty.Register(nameof(HouseNumber), typeof(string), typeof(HumanEditViewModel));

		public string ApartmentNumber
		{
			get { return (string)GetValue(ApartmentNumberProperty); }
			set { SetValue(ApartmentNumberProperty, value); }
		}
		public static readonly DependencyProperty ApartmentNumberProperty = DependencyProperty.Register(nameof(ApartmentNumber), typeof(string), typeof(HumanEditViewModel));

		public ObservableRangeCollection<Bank> BanksList { get; private set; } = new ObservableRangeCollection<Bank>();

		public Bank SelectedBank
		{
			get { return (Bank)GetValue(SelectedBankProperty); }
			set { SetValue(SelectedBankProperty, value); }
		}
		public static readonly DependencyProperty SelectedBankProperty = DependencyProperty.Register(nameof(SelectedBank), typeof(Bank), typeof(HumanEditViewModel));

		public string CheckingAccount
		{
			get { return (string)GetValue(CheckingAccountProperty); }
			set { SetValue(CheckingAccountProperty, value); }
		}
		public static readonly DependencyProperty CheckingAccountProperty = DependencyProperty.Register(nameof(CheckingAccount), typeof(string), typeof(HumanEditViewModel));

		public string DevelopmentContractNumber
		{
			get { return (string)GetValue(DevelopmentContractNumberProperty); }
			set { SetValue(DevelopmentContractNumberProperty, value); }
		}
		public static readonly DependencyProperty DevelopmentContractNumberProperty = DependencyProperty.Register(nameof(DevelopmentContractNumber), typeof(string), typeof(HumanEditViewModel));

		public DateTime DevelopmentContractDate
		{
			get { return (DateTime)GetValue(DevelopmentContractDateProperty); }
			set { SetValue(DevelopmentContractDateProperty, value); }
		}
		public static readonly DependencyProperty DevelopmentContractDateProperty = DependencyProperty.Register(nameof(DevelopmentContractDate), typeof(DateTime), typeof(HumanEditViewModel));

		public string SupportContractNumber
		{
			get { return (string)GetValue(SupportContractNumberProperty); }
			set { SetValue(SupportContractNumberProperty, value); }
		}
		public static readonly DependencyProperty SupportContractNumberProperty = DependencyProperty.Register(nameof(SupportContractNumber), typeof(string), typeof(HumanEditViewModel));

		public DateTime SupportContractDate
		{
			get { return (DateTime)GetValue(SupportContractDateProperty); }
			set { SetValue(SupportContractDateProperty, value); }
		}
		public static readonly DependencyProperty SupportContractDateProperty = DependencyProperty.Register(nameof(SupportContractDate), typeof(DateTime), typeof(HumanEditViewModel));

		public DateTime EmploymentDate
		{
			get { return (DateTime)GetValue(EmploymentDateProperty); }
			set { SetValue(EmploymentDateProperty, value); }
		}
		public static readonly DependencyProperty EmploymentDateProperty = DependencyProperty.Register(nameof(EmploymentDate), typeof(DateTime), typeof(HumanEditViewModel));

		public DateTime FiredDate
		{
			get { return (DateTime)GetValue(FiredDateProperty); }
			set { SetValue(FiredDateProperty, value); }
		}
		public static readonly DependencyProperty FiredDateProperty = DependencyProperty.Register(nameof(FiredDate), typeof(DateTime), typeof(HumanEditViewModel));

		public bool IsFired
		{
			get { return (bool)GetValue(IsFiredProperty); }
			set { SetValue(IsFiredProperty, value); }
		}
		public static readonly DependencyProperty IsFiredProperty = DependencyProperty.Register(nameof(IsFired), typeof(bool), typeof(HumanEditViewModel));

		#endregion

		#region Commands

		private void InitCommands()
		{
			LoadFromDatabase = new Command(OnLoadFromDatabaseExecute);
			SelectMode = new Command(OnSelectModeExecute, CanExecuteSelectMode);
			UnselectMode = new Command(OnUnselectModeExecute);
			ActionCommand = new Command(OnActionCommandExecute);
			BindSnackbar = new Command<Snackbar>(OnBindSnackbarExecute);
		}

		public Command LoadFromDatabase { get; private set; }
		public async void OnLoadFromDatabaseExecute()
		{
			if(state == ViewModelState.Loaded)
			{
				state = ViewModelState.Loading;
				await model.ConnectDB();
				await model.SyncCollection(LocalityTypesList);
				await model.SyncCollection(StreetTypesList);
				await model.SyncCollection(BanksList);
				await model.DisconnectDB();

				LocalityTypesList.UpdateCollection();
				UpdateProperty(SelectedLocalityTypeProperty);
				StreetTypesList.UpdateCollection();
				UpdateProperty(SelectedStreetTypeProperty);
				BanksList.UpdateCollection();
				UpdateProperty(SelectedBankProperty);
				state = ViewModelState.Loaded;
			}
			else if (state == ViewModelState.Initialized)
			{
				state = ViewModelState.Loading;
				await model.ConnectDB();
				HumanList.AddRange(await model.LoadHumans());
				LocalityTypesList.AddRange(await model.LoadLocalities());
				StreetTypesList.AddRange(await model.LoadStreets());
				BanksList.AddRange(await model.LoadBanks());
				await model.DisconnectDB();

				SelectedLocalityType = LocalityTypesList.FirstOrDefault();
				SelectedStreetType = StreetTypesList.FirstOrDefault();
				SelectedBank = BanksList.FirstOrDefault();
				state = ViewModelState.Loaded;
			}
		}

		public Command SelectMode { get; private set; }
		public void OnSelectModeExecute()
		{
			if (IsEditionMode) 
				StartHumanEdition();
			else 
				StartHumanCreation();

			ModeSelected = true;
		}

		public Command UnselectMode { get; private set; }
		public void OnUnselectModeExecute()
		{
			ModeSelected = false;
		}

		public Command ActionCommand { get; private set; }
		public async void OnActionCommandExecute()
		{
			if (IsEditionMode)
				SaveHumanChanges();
			else
				await AddHuman();
		}

		public Command<Snackbar> BindSnackbar { get; private set; }
		public void OnBindSnackbarExecute(Snackbar snackbar)
		{
			if(this.snackbar == null)
			{
				this.snackbar = snackbar;
			}
		}

		#endregion

		#region Methods

		private bool CanExecuteSelectMode()
		{
			return !ModeSelected && (!IsEditionMode || SelectedEditHuman != null);
		}

		private void StartHumanEdition()
		{

		}

		private void StartHumanCreation()
		{

		}

		private void SaveHumanChanges()
		{

		}

		private async Task AddHuman()
		{
			Address address = new Address
			{ 
				LocalityTypeId = SelectedLocalityType.Id,
				LocalityType = SelectedLocalityType,
				LocalityName = LocalityName,
				StreetTypeId = SelectedStreetType.Id,
				StreetType = SelectedStreetType,
				StreetName = StreetName,
				HouseNumber = HouseNumber,
				ApartmentNumber = ApartmentNumber,
			};
			Contract developContract = null, supportContract = null;
			if(!string.IsNullOrEmpty(DevelopmentContractNumber) && DevelopmentContractDate != default(DateTime))
			{
				developContract = new Contract
				{
					Number = DevelopmentContractNumber,
					PreparationDate = DevelopmentContractDate,
				};
			}
			if(!string.IsNullOrEmpty(SupportContractNumber) && SupportContractDate != default(DateTime))
			{
				supportContract = new Contract
				{
					Number = SupportContractNumber,
					PreparationDate = SupportContractDate,
				};
			}
			Human human = new Human
			{
				Surname = Surname,
				Name = Name,
				Secondname = SecondName,
				TIN = TIN,
				Address = address,
				BankId = SelectedBank.Id,
				Bank = SelectedBank,
				CheckingAccount = CheckingAccount,
				EmploymentDate = EmploymentDate,
				IsFired = IsFired,
			};
			if(developContract != null)
			{
				human.DevelopmentContract = developContract;
			}
			if(supportContract != null)
			{
				human.SupportContract = supportContract;
			}
			if(IsFired)
			{
				human.FiredDate = FiredDate;
			}
			await model.ConnectDB();
			human = await model.AddHuman(human);
			await model.DisconnectDB();
			HumanList.Add(human);
			SelectedEditHuman = human;
			ModeSelected = false;
			snackbar.MessageQueue?.Enqueue("Працівник \"" + human.FullName + "\" успішно добавлений.",
				null, null, null, false, true, TimeSpan.FromSeconds(3));
		}

		private void UpdateProperty(DependencyProperty prop)
		{
			object propVal = GetValue(prop);
			SetValue(prop, null);
			SetValue(prop, propVal);
		}

		#endregion
	}
}
