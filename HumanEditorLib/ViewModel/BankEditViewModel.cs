using Db.Context.HumanPart;
using HumanEditorLib.Model;
using HumanEditorLib.View;
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
		bool loaded = false;

		UIElementCollection bankCollection = null;

		public BankEditViewModel()
		{
			model = new BankEditModel();
			InitCommands();
		}

		#region Properties

		public bool HaveUnsavedChanges
		{
			get { return (bool)GetValue(HaveUnsavedChangesProperty); }
			set { SetValue(HaveUnsavedChangesProperty, value); }
		}
		public static readonly DependencyProperty HaveUnsavedChangesProperty = DependencyProperty.Register(nameof(HaveUnsavedChanges), typeof(bool), typeof(BankEditViewModel));

		#endregion

		#region Commands

		private void InitCommands()
		{
			BindBankCollection = new Command<UIElementCollection>(OnBindBankCollectionExecute);
			LoadFromDatabase = new Command(OnLoadFromDatabaseExecute);
			DeleteBank = new Command<BankControl>(OnDeleteBankExecute);
			AddBank = new Command(OnAddBankExecute);
			SaveChanges = new Command(OnSaveChangesExecute);
			PropertyChangedCommand = new Command(OnPropertyChangedCommandExecute);
		}

		public Command<UIElementCollection> BindBankCollection { get; private set; }
		private void OnBindBankCollectionExecute(UIElementCollection collection)
		{
			if(!loaded)
			{
				bankCollection = collection;
			}
		}

		public Command LoadFromDatabase { get; private set; }
		private void OnLoadFromDatabaseExecute()
		{
			if (!loaded)
			{
				foreach (Bank streetType in model.LoadBanks())
				{
					AddBankToView(streetType);
				}
				loaded = true;
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

		public Command SaveChanges { get; private set; }
		private void OnSaveChangesExecute()
		{
			model.SaveChanges(GetBanksModels());
			HaveUnsavedChanges = false;
		}

		public Command PropertyChangedCommand { get; private set; }
		private void OnPropertyChangedCommandExecute()
		{
			HaveUnsavedChanges = true;
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

		#endregion
	}
}
