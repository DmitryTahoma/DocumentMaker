using Db.Context.BackPart;
using MaterialDesignThemes.Wpf;
using Mvvm;
using Mvvm.Commands;
using ProjectEditorLib.Model.Dialogs;
using System.Collections.Generic;
using System.Windows;

namespace ProjectEditorLib.ViewModel.Controls
{
	public class SelectProjectDialogViewModel : DependencyObject
	{
		SelectProjectDialogModel model = new SelectProjectDialogModel();

		ObservableRangeCollection<Project> projects = new ObservableRangeCollection<Project>();
		DependencyObject validateObj = null;

		public SelectProjectDialogViewModel()
		{
			InitCommands();

			State = ViewModelState.Initialized;
		}

		#region Properties

		public IEnumerable<Project> Projects => projects;

		public Project SelectedProject
		{
			get { return (Project)GetValue(SelectedProjectProperty); }
			set { SetValue(SelectedProjectProperty, value); }
		}
		public static readonly DependencyProperty SelectedProjectProperty = DependencyProperty.Register(nameof(SelectedProject), typeof(Project), typeof(SelectProjectDialogViewModel));

		public bool IsOpen { get; private set; } = false;

		public string DialogHostId { get; set; } = "";

		public ViewModelState State
		{
			get { return (ViewModelState)GetValue(StateProperty); }
			set { SetValue(StateProperty, value); }
		}
		public static readonly DependencyProperty StateProperty = DependencyProperty.Register(nameof(State), typeof(ViewModelState), typeof(SelectProjectDialogViewModel));

		#endregion

		#region Commands

		private void InitCommands()
		{
			BindValidateObj = new Command<DependencyObject>(OnBindValidateObjExecute);
			OpenProjectCommand = new Command(OnOpenProjectCommandExecute, CanExecuteOpenProjectCommand);
			LoadFromDatabase = new Command(OnLoadFromDatabaseExecute);
		}

		public Command<DependencyObject> BindValidateObj { get; private set; }
		private void OnBindValidateObjExecute(DependencyObject dependencyObject)
		{
			validateObj = dependencyObject;
		}

		public Command OpenProjectCommand { get; private set; }
		private void OnOpenProjectCommandExecute()
		{
			if(ValidationHelper.GetFirstInvalid(validateObj, true) is UIElement uiElement)
			{
				uiElement.Focus();
				return;
			}

			IsOpen = true;
			DialogHost.Close(DialogHostId);
		}
		private bool CanExecuteOpenProjectCommand()
		{
			return State == ViewModelState.Loaded && validateObj != null && ValidationHelper.IsValid(validateObj);
		}

		public Command LoadFromDatabase { get; private set; }
		private async void OnLoadFromDatabaseExecute()
		{
			IsOpen = false;
			if (State == ViewModelState.Initialized)
			{
				State = ViewModelState.Loading;
				projects.AddRange(await model.LoadProjects());
				State = ViewModelState.Loaded;
			}
			else if (State == ViewModelState.Loaded)
			{
				State = ViewModelState.Loading;
				projects.SuppressingNotifications = true;
				await model.SyncCollection(projects);
				projects.SuppressingNotifications = false;
				projects.UpdateCollection();
				State = ViewModelState.Loaded;
			}
		}

		#endregion
	}
}
