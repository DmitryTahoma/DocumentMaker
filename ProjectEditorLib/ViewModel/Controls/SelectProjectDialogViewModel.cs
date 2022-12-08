using ProjectsDb.Context;
using MaterialDesignThemes.Wpf;
using Mvvm;
using Mvvm.Commands;
using ProjectEditorLib.Model.Dialogs;
using System.Collections.Generic;
using System.Windows;
using DocumentMaker.Security;
using System.Windows.Data;
using System.Linq;
using Dml;

namespace ProjectEditorLib.ViewModel.Controls
{
	public class SelectProjectDialogViewModel : DependencyObject, ICryptedConnectionStringRequired
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

		public string LastOpenedProjectName { get; set; } = null;

		#endregion

		#region Commands

		private void InitCommands()
		{
			BindValidateObj = new Command<DependencyObject>(OnBindValidateObjExecute);
			OpenProjectCommand = new Command(OnOpenProjectCommandExecute, CanExecuteOpenProjectCommand);
			LoadFromDatabase = new Command(OnLoadFromDatabaseExecute);
			SetCustomSort = new Command<ResourceDictionary>(OnSetCustomSortExecute);
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

			if (!model.ConnectionStringSetted) return;

			if (State == ViewModelState.Initialized)
			{
				State = ViewModelState.Loading;
				await model.ConnectDbAsync();
				projects.AddRange(await model.LoadProjectsAsync());
				await model.DisconnectDbAsync();
				SelectedProject = null;
				SelectedProject = projects.FirstOrDefault(x => x.Name == LastOpenedProjectName);
				State = ViewModelState.Loaded;
			}
			else if (State == ViewModelState.Loaded)
			{
				State = ViewModelState.Loading;
				projects.SuppressingNotifications = true;
				await model.ConnectDbAsync();
				await model.SyncCollectionAsync(projects);
				await model.DisconnectDbAsync();
				projects.SuppressingNotifications = false;
				projects.UpdateCollection();
				SelectedProject = null;
				SelectedProject = projects.FirstOrDefault(x => x.Name == LastOpenedProjectName);
				State = ViewModelState.Loaded;
			}
		}

		public Command<ResourceDictionary> SetCustomSort { get; private set; }
		private void OnSetCustomSortExecute(ResourceDictionary resourceDictionary)
		{
			((ListCollectionView)((CollectionViewSource)resourceDictionary["SortedProjects"]).View).CustomSort = new Dml.NaturalStringComparer();
		}

		#endregion

		public void SetCryptedConnectionString(CryptedConnectionString cryptedConnectionString)
		{
			model.ConnectionString = cryptedConnectionString;
		}
	}
}
