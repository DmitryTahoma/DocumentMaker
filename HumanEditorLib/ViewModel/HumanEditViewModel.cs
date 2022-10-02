using Db.Context.HumanPart;
using HumanEditorLib.Model;
using Mvvm;
using Mvvm.Commands;
using System.Collections.Generic;
using System.Windows;

namespace HumanEditorLib.ViewModel
{
	public class HumanEditViewModel : DependencyObject
	{
		HumanEditModel model = new HumanEditModel();
		bool loaded = false;

		public HumanEditViewModel()
		{
			InitCommands();
		}

		#region Properties

		public bool ModeSelected
		{
			get { return (bool)GetValue(ModeSelectedProperty); }
			set { SetValue(ModeSelectedProperty, value); }
		}
		public static readonly DependencyProperty ModeSelectedProperty = DependencyProperty.Register(nameof(ModeSelected), typeof(bool), typeof(HumanEditViewModel));

		public ObservableRangeCollection<Human> HumanList { get; private set; } = new ObservableRangeCollection<Human>();

		#endregion

		#region Commands

		private void InitCommands()
		{
			LoadFromDatabase = new Command(OnLoadFromDatabaseExecute);
		}

		public Command LoadFromDatabase { get; private set; }
		public async void OnLoadFromDatabaseExecute()
		{
			if(!loaded)
			{
				loaded = true;
				HumanList.AddRange(await model.LoadHumans());
			}
		}

		#endregion

		#region Methods

		#endregion
	}
}
