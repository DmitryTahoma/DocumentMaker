﻿using Db.Context.HumanPart;
using HumanEditorLib.Model;
using HumanEditorLib.View;
using Mvvm.Commands;
using Mvvm;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace HumanEditorLib.ViewModel
{
	public class LocalityEditViewModel : DependencyObject
	{
		LocalityEditModel model;
		ViewModelState state = ViewModelState.Initialized;
		bool haveUnsavedChanges = false;

		UIElementCollection localityCollection = null;

		public LocalityEditViewModel()
		{
			model = new LocalityEditModel();
			InitCommands();
		}

		#region Properties

		#endregion

		#region Commands

		private void InitCommands()
		{
			BindLocalityCollection = new Command<UIElementCollection>(OnBindLocalityCollectionExecute);
			LoadFromDatabase = new Command(OnLoadFromDatabaseExecute);
			DeleteLocalityType = new Command<LocalityControl>(OnDeleteLocalityTypeExecute);
			AddLocalityType = new Command(OnAddLocalityTypeExecute);
			SaveChanges = new Command<DependencyObject>(OnSaveChangesExecute, CanExecuteSaveChanges); 
			PropertyChangedCommand = new Command(OnPropertyChangedCommandExecute);
		}

		public Command<UIElementCollection> BindLocalityCollection { get; private set; }
		private void OnBindLocalityCollectionExecute(UIElementCollection collection)
		{
			if(localityCollection == null)
			{
				localityCollection = collection;
			}
		}

		public Command LoadFromDatabase { get; private set; }
		private async void OnLoadFromDatabaseExecute()
		{
			if (state == ViewModelState.Initialized)
			{
				state = ViewModelState.Loading;
				foreach (LocalityType localityType in await model.LoadLocalities())
				{
					AddLocalityTypeToView(localityType);
				}
				state = ViewModelState.Loaded;
			}
		}

		public Command<LocalityControl> DeleteLocalityType { get; private set; }
		private void OnDeleteLocalityTypeExecute(LocalityControl localityControl)
		{
			LocalityControlViewModel localityControlViewModel = (LocalityControlViewModel)localityControl.DataContext;
			LocalityType localityType = localityControlViewModel.GetModel();
			bool removedFromDatabase = false;
			try
			{
				removedFromDatabase = model.DeleteLocalityType(localityType);
			}
			catch(NullReferenceException) // не найдена запись в базе
			{
				removedFromDatabase = true;
			}

			if(removedFromDatabase)
			{
				localityCollection.Remove(localityControl);
			}
		}

		public Command AddLocalityType { get; private set; }
		private void OnAddLocalityTypeExecute()
		{
			AddLocalityTypeToView(model.AddLocalityType());
		}

		public Command<DependencyObject> SaveChanges { get; private set; }
		private void OnSaveChangesExecute(DependencyObject validateObj)
		{
			DependencyObject invalid = ValidationHelper.GetFirstInvalid(validateObj, true);
			if (invalid != null)
			{
				(invalid as UIElement)?.Focus();
			}
			else
			{
				model.SaveChanges(GetLocalitiesModels());
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

		private void AddLocalityTypeToView(LocalityType localityType)
		{
			LocalityControl localityControl = new LocalityControl();
			LocalityControlViewModel localityControlViewModel = (LocalityControlViewModel)localityControl.DataContext;
			localityControlViewModel.SetModel(localityType);
			localityControlViewModel.DeleteLocalityType = DeleteLocalityType;
			localityControlViewModel.PropertyChangedCommand = PropertyChangedCommand;
			localityCollection.Add(localityControl);
		}

		private IEnumerable<LocalityType> GetLocalitiesModels()
		{
			foreach(UIElement elem in localityCollection)
			{
				if(elem is LocalityControl localityControl)
				{
					yield return ((LocalityControlViewModel)localityControl.DataContext).GetModel();
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
