﻿using Db.Context.HumanPart;
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

		public string TINText
		{
			get { return (string)GetValue(TINTextProperty); }
			set { SetValue(TINTextProperty, value); }
		}
		public static readonly DependencyProperty TINTextProperty = DependencyProperty.Register(nameof(TINText), typeof(string), typeof(HumanEditViewModel));

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

		public string DevelopmentContractDateString
		{
			get { return (string)GetValue(DevelopmentContractDateStringProperty); }
			set { SetValue(DevelopmentContractDateStringProperty, value); }
		}
		public static readonly DependencyProperty DevelopmentContractDateStringProperty = DependencyProperty.Register(nameof(DevelopmentContractDateString), typeof(string), typeof(HumanEditViewModel));

		public string SupportContractNumber
		{
			get { return (string)GetValue(SupportContractNumberProperty); }
			set { SetValue(SupportContractNumberProperty, value); }
		}
		public static readonly DependencyProperty SupportContractNumberProperty = DependencyProperty.Register(nameof(SupportContractNumber), typeof(string), typeof(HumanEditViewModel));

		public string SupportContractDateString
		{
			get { return (string)GetValue(SupportContractDateStringProperty); }
			set { SetValue(SupportContractDateStringProperty, value); }
		}
		public static readonly DependencyProperty SupportContractDateStringProperty = DependencyProperty.Register(nameof(SupportContractDateString), typeof(string), typeof(HumanEditViewModel));

		public string EmploymentDateString
		{
			get { return (string)GetValue(EmploymentDateStringProperty); }
			set { SetValue(EmploymentDateStringProperty, value); }
		}
		public static readonly DependencyProperty EmploymentDateStringProperty = DependencyProperty.Register(nameof(EmploymentDateString), typeof(string), typeof(HumanEditViewModel));

		public string FiredDateString
		{
			get { return (string)GetValue(FiredDateStringProperty); }
			set { SetValue(FiredDateStringProperty, value); }
		}
		public static readonly DependencyProperty FiredDateStringProperty = DependencyProperty.Register(nameof(FiredDateString), typeof(string), typeof(HumanEditViewModel));

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
			ActionCommand = new Command<DependencyObject>(OnActionCommandExecute, CanExecuteActionCommand);
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
		public async void OnSelectModeExecute()
		{
			if (IsEditionMode) 
				await StartHumanEdition();
			else 
				StartHumanCreation();

			ModeSelected = true;
		}

		public Command UnselectMode { get; private set; }
		public void OnUnselectModeExecute()
		{
			ModeSelected = false; 
			ClearFields();
		}

		public Command<DependencyObject> ActionCommand { get; private set; }
		public async void OnActionCommandExecute(DependencyObject validateObj)
		{
			DependencyObject invalid = ValidationHelper.GetFirstInvalid(validateObj, true);
			if (invalid != null)
			{
				(invalid as UIElement)?.Focus();
			}
			else
			{
				if (IsEditionMode)
				{
					await SaveHumanChanges();
				}
				else
				{
					await AddHuman();
				}

				ModeSelected = false;
				ClearFields();
			}
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

		private async Task StartHumanEdition()
		{
			await model.ConnectDB();
			await model.LoadHuman(SelectedEditHuman);
			await model.DisconnectDB();

			Address address = SelectedEditHuman.Address;
			Contract developContract = SelectedEditHuman.DevelopmentContract, supportContract = SelectedEditHuman.SupportContract;
			Bank bank = SelectedEditHuman.Bank;

			if (address != null)
			{
				if(address.LocalityType != null)
				{
					SelectedLocalityType = LocalityTypesList.FirstOrDefault(x => x.Id == address.LocalityType.Id)
						?? address.LocalityType;
				}
				LocalityName = address.LocalityName;
				if(address.StreetType != null)
				{
					SelectedStreetType = StreetTypesList.FirstOrDefault(x => x.Id == address.StreetType.Id)
						?? address.StreetType;
				}
				StreetName = address.StreetName;
				HouseNumber = address.HouseNumber;
				ApartmentNumber = address.ApartmentNumber;
			}
			if(developContract != null)
			{
				DevelopmentContractNumber = developContract.Number;
				DevelopmentContractDateString = developContract.PreparationDate.ToString();
			}
			if(supportContract != null)
			{
				SupportContractNumber = supportContract.Number;
				SupportContractDateString = supportContract.PreparationDate.ToString();
			}
			if(bank != null)
			{
				SelectedBank = BanksList.FirstOrDefault(x => x.Id == bank.Id)
					?? bank;
			}
			Surname = SelectedEditHuman.Surname;
			Name = SelectedEditHuman.Name;
			SecondName = SelectedEditHuman.Secondname;
			TINText = SelectedEditHuman.TIN.ToString();
			CheckingAccount = SelectedEditHuman.CheckingAccount;
			EmploymentDateString = SelectedEditHuman.EmploymentDate?.ToString();
			FiredDateString = SelectedEditHuman.FiredDate?.ToString();
			IsFired = SelectedEditHuman.IsFired;
		}

		private void StartHumanCreation()
		{

		}

		private async Task SaveHumanChanges()
		{
			Human human = new Human(SelectedEditHuman);
			human.Set(GetHumanFromFields());
			await model.ConnectDB();
			Human savedHuman = await model.SaveHumanChanges(human);
			await model.DisconnectDB();

			int selectedIndex = HumanList.IndexOf(SelectedEditHuman);
			HumanList.Remove(SelectedEditHuman);
			HumanList.Insert(selectedIndex, savedHuman);
			SelectedEditHuman = savedHuman;

			snackbar.MessageQueue?.Enqueue("Зміни успішно збережені.",
				null, null, null, false, true, TimeSpan.FromSeconds(3));
		}

		private async Task AddHuman()
		{
			Human human = GetHumanFromFields();
			await model.ConnectDB();
			human = await model.AddHuman(human);
			await model.DisconnectDB();
			HumanList.Add(human);
			SelectedEditHuman = human;
			snackbar.MessageQueue?.Enqueue("Працівник \"" + human.FullName + "\" успішно добавлений.",
				null, null, null, false, true, TimeSpan.FromSeconds(3));
		}

		private void UpdateProperty(DependencyProperty prop)
		{
			object propVal = GetValue(prop);
			SetValue(prop, null);
			SetValue(prop, propVal);
		}

		private bool CanExecuteActionCommand(DependencyObject validateObj)
		{
			return ValidationHelper.IsValid(validateObj);
		}

		private void ClearFields()
		{
			string _ = string.Empty;
			Surname = _;
			Name = _;
			SecondName = _;
			TINText = _;
			SelectedLocalityType = LocalityTypesList.FirstOrDefault();
			LocalityName = _;
			SelectedStreetType = StreetTypesList.FirstOrDefault();
			StreetName = _;
			HouseNumber = _;
			ApartmentNumber = _;
			SelectedBank = BanksList.FirstOrDefault();
			CheckingAccount = _;
			DevelopmentContractNumber = _;
			DevelopmentContractDateString = _;
			SupportContractNumber = _;
			SupportContractDateString = _;
			EmploymentDateString = _;
			FiredDateString = _;
			IsFired = false;
		}

		private Human GetHumanFromFields()
		{
			Address address = new Address
			{
				LocalityTypeId = SelectedLocalityType.Id,
				LocalityName = LocalityName,
				StreetTypeId = SelectedStreetType.Id,
				StreetName = StreetName,
				HouseNumber = HouseNumber,
				ApartmentNumber = ApartmentNumber,
			};
			Contract developContract = null, supportContract = null;
			if (!string.IsNullOrEmpty(DevelopmentContractNumber) && !string.IsNullOrEmpty(DevelopmentContractDateString))
			{
				developContract = new Contract
				{
					Number = DevelopmentContractNumber,
					PreparationDate = DateTime.Parse(DevelopmentContractDateString),
				};
			}
			if (!string.IsNullOrEmpty(SupportContractNumber) && !string.IsNullOrEmpty(SupportContractDateString))
			{
				supportContract = new Contract
				{
					Number = SupportContractNumber,
					PreparationDate = DateTime.Parse(SupportContractDateString),
				};
			}
			Human human = new Human
			{
				Surname = Surname,
				Name = Name,
				Secondname = SecondName,
				TIN = long.Parse(TINText),
				Address = address,
				BankId = SelectedBank.Id,
				CheckingAccount = CheckingAccount,
				IsFired = IsFired,
			};
			if (developContract != null)
			{
				human.DevelopmentContract = developContract;
			}
			if (supportContract != null)
			{
				human.SupportContract = supportContract;
			}
			if (!string.IsNullOrEmpty(EmploymentDateString))
			{
				human.EmploymentDate = DateTime.Parse(EmploymentDateString);
			}
			if (IsFired && !string.IsNullOrEmpty(FiredDateString))
			{
				human.FiredDate = DateTime.Parse(FiredDateString);
			}
			return human;
		}

		#endregion
	}
}
