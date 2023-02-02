using ActGenerator.Model.Dialogs;
using ActGenerator.View.Controls;
using ActGenerator.View.Dialogs.Controls;
using ActGenerator.ViewModel.Dialogs.Controls;
using DocumentMaker.Security;
using MaterialDesignThemes.Wpf;
using Mvvm;
using Mvvm.Commands;
using ProjectEditorLib.ViewModel;
using ProjectsDb.Context;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ActGenerator.ViewModel.Dialogs
{
	public class AddProjectNameDialogViewModel : DependencyObject, ICryptedConnectionStringRequired
	{
		AddProjectNameDialogModel model = new AddProjectNameDialogModel();

		UIElementCollection itemsCollection = null;
		IEnumerable<ProjectNameDialogItem> castedItems = null;
		IEnumerable<ProjectNameDialogItemViewModel> itemsViewModel = null;

		bool needUpdateProjectsCheckBoxIsChecked = true;
		bool needUpdateProjectNamesCheckBoxIsChecked = true;

		public AddProjectNameDialogViewModel()
		{
			InitCommands();

			State = ViewModelState.Initialized;
		}

		#region Properties

		public bool IsPressedAdd { get; private set; }

		public IEnumerable<IDbObject> SelectedProjectsAndNames { get; private set; }

		public ViewModelState State
		{
			get { return (ViewModelState)GetValue(StateProperty); }
			set { SetValue(StateProperty, value); }
		}
		public static readonly DependencyProperty StateProperty = DependencyProperty.Register(nameof(State), typeof(ViewModelState), typeof(AddProjectNameDialogViewModel));

		public bool? ProjectsCheckBoxIsChecked
		{
			get { return (bool?)GetValue(ProjectsCheckBoxIsCheckedProperty); }
			set { SetValue(ProjectsCheckBoxIsCheckedProperty, value); }
		}
		public static readonly DependencyProperty ProjectsCheckBoxIsCheckedProperty = DependencyProperty.Register(nameof(ProjectsCheckBoxIsChecked), typeof(bool?), typeof(AddProjectNameDialogViewModel), new PropertyMetadata(false));

		public bool? ProjectNamesCheckBoxIsChecked
		{
			get { return (bool?)GetValue(ProjectNamesCheckBoxIsCheckedProperty); }
			set { SetValue(ProjectNamesCheckBoxIsCheckedProperty, value); }
		}
		public static readonly DependencyProperty ProjectNamesCheckBoxIsCheckedProperty = DependencyProperty.Register(nameof(ProjectNamesCheckBoxIsChecked), typeof(bool?), typeof(AddProjectNameDialogViewModel), new PropertyMetadata(false));

		#endregion

		#region Commands

		private void InitCommands()
		{
			BindProjectsCheckBoxList = new Command<UIElementCollection>(OnBindProjectsCheckBoxListExecute);
			ViewLoaded = new Command(OnViewLoadedExecute);
			AddCommand = new Command(OnAddCommandExecute);
			ChangeIsCheckedAllProjects = new Command(OnChangeIsCheckedAllProjectsExecute);
			ChangeIsCheckedAllProjectNames = new Command(OnChangeIsCheckedAllProjectNamesExecute);
		}

		public Command<UIElementCollection> BindProjectsCheckBoxList { get; private set; }
		private void OnBindProjectsCheckBoxListExecute(UIElementCollection projectsCheckBoxCollection)
		{
			if (itemsCollection == null)
			{
				itemsCollection = projectsCheckBoxCollection;

				castedItems = itemsCollection.Cast<ProjectNameDialogItem>();
				itemsViewModel = castedItems.Select(x => (ProjectNameDialogItemViewModel)x.DataContext);
				SelectedProjectsAndNames =
					itemsViewModel
					.Where(vm1 => vm1.IsCheckedProject)
					.Select(vm2 => vm2.Project)
					.Cast<IDbObject>()
					.Union(itemsViewModel
						.Where(vm3 => vm3.AlternativeProjectName != null && vm3.IsCheckedAlternativeProjectName)
						.Select(vm4 => vm4.AlternativeProjectName)
						.Cast<IDbObject>());
			}
		}

		public Command ViewLoaded { get; private set; }
		private async void OnViewLoadedExecute()
		{
			IsPressedAdd = false;

			ProjectsCheckBoxIsChecked = false;
			ChangeIsCheckedAllProjects?.Execute();
			ProjectNamesCheckBoxIsChecked = false;
			ChangeIsCheckedAllProjectNames?.Execute();

			if (!model.ConnectionStringSetted) return;

			if (State == ViewModelState.Initialized)
			{
				await LoadProjectsAsync();
			}
			else if (State == ViewModelState.Loaded)
			{
				await UpdateProjectsAsync();
			}
		}

		public Command AddCommand { get; private set; }
		private void OnAddCommandExecute()
		{
			IsPressedAdd = true;

			DialogHost.CloseDialogCommand.Execute(null, null);
		}

		public Command ChangeIsCheckedAllProjects { get; private set; }
		private void OnChangeIsCheckedAllProjectsExecute()
		{
			needUpdateProjectsCheckBoxIsChecked = false;
			foreach (ProjectNameDialogItemViewModel vm in itemsViewModel)
			{
				vm.IsCheckedProject = ProjectsCheckBoxIsChecked.HasValue && ProjectsCheckBoxIsChecked.Value;
			}
			needUpdateProjectsCheckBoxIsChecked = true;
		}

		public Command ChangeIsCheckedAllProjectNames { get; private set; }
		private void OnChangeIsCheckedAllProjectNamesExecute()
		{
			needUpdateProjectNamesCheckBoxIsChecked = false;
			foreach (ProjectNameDialogItemViewModel vm in itemsViewModel)
			{
				vm.IsCheckedAlternativeProjectName = ProjectNamesCheckBoxIsChecked.HasValue && ProjectNamesCheckBoxIsChecked.Value;
			}
			needUpdateProjectNamesCheckBoxIsChecked = true;
		}

		#endregion

		#region Methods

		private ProjectNameDialogItem CreateProjectItem(Project project, AlternativeProjectName alternativeProjectName)
		{
			ProjectNameDialogItem projectNameDialogItem = new ProjectNameDialogItem();
			ProjectNameDialogItemViewModel viewModel = (ProjectNameDialogItemViewModel)projectNameDialogItem.DataContext;

			viewModel.Project = project;
			viewModel.AlternativeProjectName = alternativeProjectName;

			viewModel.ProjectCheckedChangedCommand = new Command(UpdateProjectsCheckBoxIsChecked);
			viewModel.AlternativeProjectNameCheckedChangedCommand = new Command(UpdateProjectNamesCheckBoxIsChecked);

			return projectNameDialogItem;
		}

		public void SetCryptedConnectionString(CryptedConnectionString cryptedConnectionString)
		{
			model.ConnectionString = cryptedConnectionString;
		}

		private void UpdateProjectsCheckBoxIsChecked()
		{
			if (!needUpdateProjectsCheckBoxIsChecked) return;
			ProjectsCheckBoxIsChecked = CheckBoxList.UpdateGeneralCheckBoxState(itemsViewModel, vm => vm.IsCheckedProject);
		}

		private void UpdateProjectNamesCheckBoxIsChecked()
		{
			if (!needUpdateProjectNamesCheckBoxIsChecked) return;
			ProjectNamesCheckBoxIsChecked = CheckBoxList.UpdateGeneralCheckBoxState(itemsViewModel, vm => vm.IsCheckedAlternativeProjectName);
		}

		private async Task LoadProjectsAsync()
		{
			State = ViewModelState.Loading;
			await model.ConnectDbAsync();
			List<Project> projects = await model.LoadProjectsAsync();
			List<AlternativeProjectName> alternativeProjectNames = await model.LoadAlternativeProjectNamesAsync();
			await model.DisconnectDbAsync();
			await model.SortCollectionAsync(projects);
			State = ViewModelState.Loaded;
			await FillProjectsAsync(projects, alternativeProjectNames);
		}

		private Task FillProjectsAsync(List<Project> projects, List<AlternativeProjectName> alternativeProjectNames)
		{
			return Task.Run(async () =>
			{
				foreach (Project project in projects)
				{
					Dispatcher.Invoke(() =>
					{
						itemsCollection.Add(CreateProjectItem(project, alternativeProjectNames.FirstOrDefault(x => x.ProjectId == project.Id)));
						UpdateProjectsCheckBoxIsChecked();
						UpdateProjectNamesCheckBoxIsChecked();
					});
					await Task.Delay(1);
				}
			});
		}

		private async Task UpdateProjectsAsync()
		{
			State = ViewModelState.Loading;
			await model.ConnectDbAsync();
			List<Project> projects = await model.LoadProjectsAsync();
			List<AlternativeProjectName> alternativeProjectNames = await model.LoadAlternativeProjectNamesAsync();
			await model.DisconnectDbAsync();
			await model.SortCollectionAsync(projects);
			await UpdateProjectsItemsAsync(projects, alternativeProjectNames);

			UpdateProjectsCheckBoxIsChecked();
			UpdateProjectNamesCheckBoxIsChecked();
			State = ViewModelState.Loaded;
		}

		private async Task UpdateProjectsItemsAsync(List<Project> projects, List<AlternativeProjectName> alternativeProjectNames)
		{
			await Task.Run(async () =>
			{
				int localCount = Dispatcher.Invoke(() => itemsCollection.Count);
				while (localCount > projects.Count)
				{
					Dispatcher.Invoke(() => itemsCollection.RemoveAt(0));
					--localCount;
					await Task.Delay(1);
				}

				while (localCount < projects.Count)
				{
					Dispatcher.Invoke(() => itemsCollection.Add(CreateProjectItem(null, null)));
					++localCount;
					await Task.Delay(1);
				}

				IEnumerator<Project> projectsEnum = projects.GetEnumerator();
				IEnumerator<ProjectNameDialogItemViewModel> vmEnum = itemsViewModel.GetEnumerator();
				while (projectsEnum.MoveNext() && Dispatcher.Invoke(() => vmEnum.MoveNext()))
				{
					Dispatcher.Invoke(() => vmEnum.Current.Project = projectsEnum.Current);
					Dispatcher.Invoke(() => vmEnum.Current.AlternativeProjectName = alternativeProjectNames.FirstOrDefault(x => x.ProjectId == projectsEnum.Current.Id));
					await Task.Delay(1);
				}
			});
		}

		#endregion
	}
}
