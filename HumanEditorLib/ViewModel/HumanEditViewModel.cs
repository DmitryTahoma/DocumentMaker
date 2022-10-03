using Db.Context.HumanPart;
using HumanEditorLib.Model;
using Mvvm;
using Mvvm.Commands;
using System.Windows;

namespace HumanEditorLib.ViewModel
{
	public class HumanEditViewModel : DependencyObject
	{
		HumanEditModel model = new HumanEditModel();
		ViewModelState state = ViewModelState.Initialized;

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

		#endregion

		#region Commands

		private void InitCommands()
		{
			LoadFromDatabase = new Command(OnLoadFromDatabaseExecute);
			SelectMode = new Command(OnSelectModeExecute, CanExecuteSelectMode);
			UnselectMode = new Command(OnUnselectModeExecute);
		}

		public Command LoadFromDatabase { get; private set; }
		public async void OnLoadFromDatabaseExecute()
		{
			if (state == ViewModelState.Initialized)
			{
				state = ViewModelState.Loading;
				HumanList.AddRange(await model.LoadHumans());
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

		#endregion
	}
}
