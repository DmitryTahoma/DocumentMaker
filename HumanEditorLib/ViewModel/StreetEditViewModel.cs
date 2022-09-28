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
	public class StreetEditViewModel
	{
		StreetEditModel model;
		UIElementCollection streetCollection = null;

		public StreetEditViewModel()
		{
			model = new StreetEditModel();
			InitCommands();
		}

		#region Commands

		private void InitCommands()
		{
			BindStreetCollection = new Command<UIElementCollection>(OnBindStreetCollectionExecute);
			LoadFromDatabase = new Command(OnLoadFromDatabaseExecute);
			DeleteStreetType = new Command<StreetControl>(OnDeleteStreetTypeExecute);
			AddStreetType = new Command(OnAddStreetTypeExecute);
			SaveChanges = new Command(OnSaveChangesExecute);
		}

		public Command<UIElementCollection> BindStreetCollection { get; private set; }
		private void OnBindStreetCollectionExecute(UIElementCollection collection)
		{
			streetCollection = collection;
		}

		public Command LoadFromDatabase { get; private set; }
		private void OnLoadFromDatabaseExecute()
		{
			foreach(StreetType streetType in model.LoadStreets())
			{
				AddStreetTypeToView(streetType);
			}
		}

		public Command<StreetControl> DeleteStreetType { get; private set; }
		private void OnDeleteStreetTypeExecute(StreetControl streetControl)
		{
			StreetControlViewModel streetControlViewModel = (StreetControlViewModel)streetControl.DataContext;
			StreetType streetType = streetControlViewModel.GetModel();
			bool removedFromDatabase = false;
			try
			{
				removedFromDatabase = model.DeleteStreetType(streetType);
			}
			catch(NullReferenceException) // не найдена запись в базе
			{
				removedFromDatabase = true;
			}

			if(removedFromDatabase)
			{
				streetCollection.Remove(streetControl);
			}
		}

		public Command AddStreetType { get; private set; }
		private void OnAddStreetTypeExecute()
		{
			AddStreetTypeToView(model.AddStreetType());
		}

		public Command SaveChanges { get; private set; }
		private void OnSaveChangesExecute()
		{
			model.SaveChanges(GetStreetsModels());
		}

		#endregion

		#region Methods

		private void AddStreetTypeToView(StreetType streetType)
		{
			StreetControl streetControl = new StreetControl();
			StreetControlViewModel streetControlViewModel = (StreetControlViewModel)streetControl.DataContext;
			streetControlViewModel.SetModel(streetType);
			streetControlViewModel.DeleteStreetType = DeleteStreetType;
			streetCollection.Add(streetControl);
		}

		private IEnumerable<StreetType> GetStreetsModels()
		{
			foreach(UIElement elem in streetCollection)
			{
				if(elem is StreetControl streetControl)
				{
					yield return ((StreetControlViewModel)streetControl.DataContext).GetModel();
				}
			}
		}

		#endregion
	}
}
