using ActGenerator.Model.Dialogs;
using DocumentMaker.Security;
using MaterialDesignThemes.Wpf;
using Mvvm;
using Mvvm.Commands;
using ProjectEditorLib.ViewModel;
using ProjectsDb.Context;
using System.Collections;
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

		readonly Style itemStyle = Application.Current.FindResource("AddProjectNameListItemCheckBox") as Style;

		UIElementCollection projectsCheckBoxCollection = null;
		UIElementCollection projectNamesCheckBoxCollection = null;

		bool needUpdateProjectsCheckBoxIsChecked = true;
		bool needUpdateProjectNamesCheckBoxIsChecked = true;

		public AddProjectNameDialogViewModel()
		{
			InitCommands();

			State = ViewModelState.Initialized;
		}

		#region Properties

		public bool IsPressedAdd { get; private set; }

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
			BindProjectNamesCheckBoxList = new Command<UIElementCollection>(OnBindProjectNamesCheckBoxListExecute);
			ViewLoaded = new Command(OnViewLoadedExecute);
			AddCommand = new Command(OnAddCommandExecute);
			ChangeIsCheckedAllProjects = new Command(OnChangeIsCheckedAllProjectsExecute);
			ChangeIsCheckedAllProjectNames = new Command(OnChangeIsCheckedAllProjectNamesExecute);
		}

		public Command<UIElementCollection> BindProjectsCheckBoxList { get; private set; }
		private void OnBindProjectsCheckBoxListExecute(UIElementCollection projectsCheckBoxCollection)
		{
			if (this.projectsCheckBoxCollection == null)
			{
				this.projectsCheckBoxCollection = projectsCheckBoxCollection;
			}
		}

		public Command<UIElementCollection> BindProjectNamesCheckBoxList { get; private set; }
		private void OnBindProjectNamesCheckBoxListExecute(UIElementCollection projectNamesCheckBoxCollection)
		{
			if (this.projectNamesCheckBoxCollection == null)
			{
				this.projectNamesCheckBoxCollection = projectNamesCheckBoxCollection;
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
			needUpdateProjectNamesCheckBoxIsChecked = false;
			foreach (CheckBox checkBox in projectsCheckBoxCollection)
			{
				checkBox.IsChecked = ProjectsCheckBoxIsChecked;
			}
			needUpdateProjectsCheckBoxIsChecked = true;
			needUpdateProjectNamesCheckBoxIsChecked = true;
			UpdateProjectNamesCheckBoxIsChecked();
		}

		public Command ChangeIsCheckedAllProjectNames { get; private set; }
		private void OnChangeIsCheckedAllProjectNamesExecute()
		{
			needUpdateProjectNamesCheckBoxIsChecked = false;
			foreach (CheckBox checkBox in projectNamesCheckBoxCollection)
			{
				checkBox.IsChecked = ProjectNamesCheckBoxIsChecked;
			}
			needUpdateProjectNamesCheckBoxIsChecked = true;
		}

		#endregion

		#region Methods

		private CheckBox CreateProjectCheckBox(IDbObject context)
		{
			CheckBox checkBox = new CheckBox
			{
				DataContext = context,
				Style = itemStyle,
			};

			if (context is Project project)
			{
				checkBox.Content = project.Name;
				checkBox.Checked += ProjectCheckBoxCheckedChange;
				checkBox.Unchecked += ProjectCheckBoxCheckedChange;
			}
			else if (context is AlternativeProjectName projectName)
			{
				checkBox.Content = projectName.Name;
				checkBox.Checked += ProjectNameCheckBoxCheckedChange;
				checkBox.Unchecked += ProjectNameCheckBoxCheckedChange;
			}

			return checkBox;
		}

		public void SetCryptedConnectionString(CryptedConnectionString cryptedConnectionString)
		{
			model.ConnectionString = cryptedConnectionString;
		}

		private void UpdateProjectsCheckBoxIsChecked()
		{
			if (!needUpdateProjectsCheckBoxIsChecked) return;
			ProjectsCheckBoxIsChecked = UpdateCheckBoxState(ProjectsCheckBoxIsChecked, projectsCheckBoxCollection);
		}

		private void UpdateProjectNamesCheckBoxIsChecked()
		{
			if (!needUpdateProjectNamesCheckBoxIsChecked) return;
			ProjectNamesCheckBoxIsChecked = UpdateCheckBoxState(ProjectNamesCheckBoxIsChecked, projectNamesCheckBoxCollection);
		}

		private bool? UpdateCheckBoxState(bool? prevState, IEnumerable checkBoxList)
		{
			IEnumerator<CheckBox> checkBoxListEnum = checkBoxList.Cast<CheckBox>().GetEnumerator();

			bool findedFirst = false;
			bool? isChecked = null;

			while (checkBoxListEnum.MoveNext())
			{
				if (checkBoxListEnum.Current.Visibility == Visibility.Visible)
				{
					findedFirst = true;
					isChecked = checkBoxListEnum.Current.IsChecked;
					break;
				}
			}

			if (!findedFirst)
			{
				return false;
			}
			else
			{
				while (checkBoxListEnum.MoveNext())
				{
					if (checkBoxListEnum.Current.Visibility == Visibility.Visible && checkBoxListEnum.Current.IsChecked != isChecked)
					{
						return null;
					}
				}

				return isChecked;
			}
		}

		private void ProjectCheckBoxCheckedChange(object sender, RoutedEventArgs e)
		{
			UpdateProjectsCheckBoxIsChecked();
		}

		private void ProjectNameCheckBoxCheckedChange(object sender, RoutedEventArgs e)
		{
			UpdateProjectNamesCheckBoxIsChecked();
		}

		private async Task LoadProjectsAsync()
		{
			State = ViewModelState.Loading;
			await model.ConnectDbAsync();
			List<Project> projects = await model.LoadProjectsAsync();
			List<AlternativeProjectName> alternativeProjectNames = await model.LoadAlternativeProjectNamesAsync();
			await model.DisconnectDbAsync();
			await model.SortCollectionAsync(projects);
			await model.SortCollectionAsync(alternativeProjectNames);
			State = ViewModelState.Loaded;

			Task[] tasksFilling =
			{
				FillCheckBoxCollectionAsync(projectsCheckBoxCollection, projects),
				FillCheckBoxCollectionAsync(projectNamesCheckBoxCollection, alternativeProjectNames),
			};

			foreach (Task task in tasksFilling)
			{
				await task;
			}
		}

		private Task FillCheckBoxCollectionAsync<T>(UIElementCollection checkBoxCollection, List<T> objects) where T : IDbObject
		{
			return Task.Run(async () =>
			{
				foreach (T obj in objects)
				{
					Dispatcher.Invoke(() =>
					{
						checkBoxCollection.Add(CreateProjectCheckBox(obj));
						if (obj is Project)
							UpdateProjectsCheckBoxIsChecked();
						else
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
			await model.SortCollectionAsync(alternativeProjectNames);

			Task updatingProjects = GetUpdateCheckBoxCollectionTask(projectsCheckBoxCollection, projects);
			Task updatingAlternativeNames = GetUpdateCheckBoxCollectionTask(projectNamesCheckBoxCollection, alternativeProjectNames);
			_ = model.SortCollectionAsync(projects).ContinueWith(t => updatingProjects.Start());
			_ = model.SortCollectionAsync(alternativeProjectNames).ContinueWith(t => updatingAlternativeNames.Start());
			await updatingProjects;
			await updatingAlternativeNames;

			UpdateProjectsCheckBoxIsChecked();
			UpdateProjectNamesCheckBoxIsChecked();
			State = ViewModelState.Loaded;
		}

		private Task GetUpdateCheckBoxCollectionTask<T>(UIElementCollection checkBoxCollection, List<T> objects) where T : IDbObject, new()
		{
			return new Task(() =>
			{
				int localCount = Dispatcher.Invoke(() => checkBoxCollection.Count);
				while (localCount > objects.Count)
				{
					Dispatcher.Invoke(() => checkBoxCollection.RemoveAt(0));
					--localCount;
				}

				T fakeObject = new T();
				while (localCount < objects.Count)
				{
					Dispatcher.Invoke(() => checkBoxCollection.Add(CreateProjectCheckBox(fakeObject)));
					++localCount;
				}

				IEnumerator<T> projectsEnum = objects.GetEnumerator();
				IEnumerator<CheckBox> checkBoxCollectionEnum = Dispatcher.Invoke(() => checkBoxCollection.Cast<CheckBox>().GetEnumerator());
				while (projectsEnum.MoveNext() && Dispatcher.Invoke(() => checkBoxCollectionEnum.MoveNext()))
				{
					Dispatcher.Invoke(() => checkBoxCollectionEnum.Current.DataContext = projectsEnum.Current);
					if(projectsEnum.Current is Project project)
					{
						Dispatcher.Invoke(() => checkBoxCollectionEnum.Current.Content = project.Name);
					}
					else if(projectsEnum.Current is AlternativeProjectName alternativeProjectName)
					{
						Dispatcher.Invoke(() => checkBoxCollectionEnum.Current.Content = alternativeProjectName.Name);
					}
				}
			});
		}

		#endregion
	}
}
