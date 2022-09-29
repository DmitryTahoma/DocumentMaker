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
	public class LocalityEditViewModel
	{
		LocalityEditModel model;
		UIElementCollection localityCollection = null;

		public LocalityEditViewModel()
		{
			model = new LocalityEditModel();
			InitCommands();
		}

		#region Commands

		private void InitCommands()
		{
			BindLocalityCollection = new Command<UIElementCollection>(OnBindLocalityCollectionExecute);
			LoadFromDatabase = new Command(OnLoadFromDatabaseExecute);
			DeleteLocalityType = new Command<LocalityControl>(OnDeleteLocalityTypeExecute);
			AddLocalityType = new Command(OnAddLocalityTypeExecute);
			SaveChanges = new Command(OnSaveChangesExecute);
		}

		public Command<UIElementCollection> BindLocalityCollection { get; private set; }
		private void OnBindLocalityCollectionExecute(UIElementCollection collection)
		{
			localityCollection = collection;
		}

		public Command LoadFromDatabase { get; private set; }
		private void OnLoadFromDatabaseExecute()
		{
			foreach(LocalityType localityType in model.LoadLocalities())
			{
				AddLocalityTypeToView(localityType);
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

		public Command SaveChanges { get; private set; }
		private void OnSaveChangesExecute()
		{
			model.SaveChanges(GetLocalitiesModels());
		}

		#endregion

		#region Methods

		private void AddLocalityTypeToView(LocalityType localityType)
		{
			LocalityControl localityControl = new LocalityControl();
			LocalityControlViewModel localityControlViewModel = (LocalityControlViewModel)localityControl.DataContext;
			localityControlViewModel.SetModel(localityType);
			localityControlViewModel.DeleteLocalityType = DeleteLocalityType;
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

		#endregion
	}
}
