using Db.Context.HumanPart;
using HumanEditorLib.Model;
using HumanEditorLib.View;
using Mvvm.Commands;
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
				StreetControl streetControl = new StreetControl();
				StreetControlViewModel streetControlViewModel = (StreetControlViewModel)streetControl.DataContext;
				streetControlViewModel.SetFromDatabase(streetType);
				streetCollection.Add(streetControl);
			}
		}

		#endregion
	}
}
