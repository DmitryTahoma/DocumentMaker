using Db.Context.HumanPart;
using HumanEditorLib.Model;
using HumanEditorLib.View;
using Mvvm;
using Mvvm.Commands;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace HumanEditorLib.ViewModel
{
	public class BankEditViewModel : DependencyObject
	{
		BankEditModel model;
		ViewModelState state = ViewModelState.Initialized;
		bool haveUnsavedChanges = false;

		UIElementCollection bankCollection = null;

		public BankEditViewModel()
		{
			model = new BankEditModel();
			InitCommands();
		}

		#region Properties

		#endregion

		#region Commands

		private void InitCommands()
		{
			BindBankCollection = new Command<UIElementCollection>(OnBindBankCollectionExecute);
			LoadFromDatabase = new Command(OnLoadFromDatabaseExecute);
			DeleteBank = new Command<BankControl>(OnDeleteBankExecute);
			AddBank = new Command(OnAddBankExecute);
			SaveChanges = new Command<DependencyObject>(OnSaveChangesExecute, CanExecuteSaveChanges);
			PropertyChangedCommand = new Command(OnPropertyChangedCommandExecute);
		}

		public Command<UIElementCollection> BindBankCollection { get; private set; }
		private void OnBindBankCollectionExecute(UIElementCollection collection)
		{
			if(bankCollection == null)
			{
				bankCollection = collection;
			}
		}

		public Command LoadFromDatabase { get; private set; }
		private async void OnLoadFromDatabaseExecute()
		{
			if (state == ViewModelState.Initialized)
			{
				state = ViewModelState.Loading;
				foreach (Bank streetType in await model.LoadBanks())
				{
					AddBankToView(streetType);
				}
				state = ViewModelState.Loaded;
			}
		}

		public Command<BankControl> DeleteBank { get; private set; }
		private void OnDeleteBankExecute(BankControl bankControl)
		{
			BankControlViewModel bankControlViewModel = (BankControlViewModel)bankControl.DataContext;
			Bank bank = bankControlViewModel.GetModel();
			bool removedFromDatabase = false;
			try
			{
				removedFromDatabase = model.DeleteBank(bank);
			}
			catch(NullReferenceException) // не найдена запись в базе
			{
				removedFromDatabase = true;
			}

			if(removedFromDatabase)
			{
				bankCollection.Remove(bankControl);
			}
		}

		public Command AddBank { get; private set; }
		private void OnAddBankExecute()
		{
			AddBankToView(model.AddBank());
		}

		public Command<DependencyObject> SaveChanges { get; private set; }
		private void OnSaveChangesExecute(DependencyObject validateObj)
		{
			DependencyObject invalid = ValidationHelper.GetFirstInvalid(validateObj, true);
			if(invalid != null)
			{
				(invalid as UIElement)?.Focus();
			}
			else
			{
				model.SaveChanges(GetBanksModels());
				haveUnsavedChanges = false;
			}
		}

		public Command PropertyChangedCommand { get; private set; }
		private void OnPropertyChangedCommandExecute()
		{
			haveUnsavedChanges = true;
		}

		#endregion

		#region Methods

		private void AddBankToView(Bank bank)
		{
			BankControl bankControl = new BankControl();
			BankControlViewModel bankControlViewModel = (BankControlViewModel)bankControl.DataContext;
			bankControlViewModel.SetModel(bank);
			bankControlViewModel.DeleteBank = DeleteBank;
			bankControlViewModel.PropertyChangedCommand = PropertyChangedCommand;
			bankCollection.Add(bankControl);
		}

		private IEnumerable<Bank> GetBanksModels()
		{
			foreach(UIElement elem in bankCollection)
			{
				if(elem is BankControl bankControl)
				{
					yield return ((BankControlViewModel)bankControl.DataContext).GetModel();
				}
			}
		}

		private bool CanExecuteSaveChanges(DependencyObject validateObj)
		{
			return haveUnsavedChanges && ValidationHelper.IsValid(validateObj);
		}

		#endregion
	}
}
