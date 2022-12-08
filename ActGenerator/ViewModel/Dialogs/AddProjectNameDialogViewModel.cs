using ActGenerator.Model.Dialogs;
using DocumentMaker.Security;
using MaterialDesignThemes.Wpf;
using Mvvm;
using Mvvm.Commands;
using ProjectEditorLib.ViewModel;
using ProjectsDb.Context;
using System;
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

		#endregion

		#region Commands

		private void InitCommands()
		{
			BindProjectsCheckBoxList = new Command<UIElementCollection>(OnBindProjectsCheckBoxListExecute);
			BindProjectNamesCheckBoxList = new Command<UIElementCollection>(OnBindProjectNamesCheckBoxListExecute);
			ViewLoaded = new Command(OnViewLoadedExecute);
			AddCommand = new Command(OnAddCommandExecute);
			ChangeIsCheckedAll = new Command(OnChangeIsCheckedAllExecute);
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
			if(this.projectNamesCheckBoxCollection == null)
			{
				this.projectNamesCheckBoxCollection = projectNamesCheckBoxCollection;

				projectNamesCheckBoxCollection.Add(CreateProjectCheckBox(new Project { Name = "Escape" }));
				projectNamesCheckBoxCollection.Add(CreateProjectCheckBox(new AlternativeProjectName { Name = "Exit" }));
			}
		}

		public Command ViewLoaded { get; private set; }
		private async void OnViewLoadedExecute()
		{
			IsPressedAdd = false;

			if (!model.ConnectionStringSetted) return;

			if(State == ViewModelState.Initialized)
			{
				State = ViewModelState.Loading;
				await model.ConnectDbAsync();
				IEnumerable<Project> projects = await model.LoadProjectsAsync();
				await model.DisconnectDbAsync();
				State = ViewModelState.Loaded;

				projectsCheckBoxCollection.Clear();
				foreach(Project project in projects)
				{
					projectsCheckBoxCollection.Add(CreateProjectCheckBox(project));
					UpdateProjectsCheckBoxIsChecked();
					await Task.Delay(1);
				}
			}
			else if(State == ViewModelState.Loaded)
			{
				State = ViewModelState.Loading;
				await model.ConnectDbAsync();
				IEnumerable<Project> projects = await model.LoadProjectsAsync();
				await model.DisconnectDbAsync();

				List<CheckBox> removedCheckBoxes = projectsCheckBoxCollection.Cast<CheckBox>().ToList();
				foreach (Project project in projects)
				{
					bool finded = false;
					foreach(CheckBox checkBox in projectsCheckBoxCollection)
					{
						if(checkBox.DataContext is Project proj && proj.Id == project.Id)
						{
							checkBox.DataContext = project;
							checkBox.Content = project.Name;
							removedCheckBoxes.Remove(checkBox);
							finded = true;
							break;
						}
					}

					if(!finded)
					{
						projectsCheckBoxCollection.Add(CreateProjectCheckBox(project));
					}
				}

				removedCheckBoxes.ForEach(x => projectNamesCheckBoxCollection.Remove(x));
				UpdateProjectsCheckBoxIsChecked();
				State = ViewModelState.Loaded;
			}
		}

		public Command AddCommand { get; private set; }
		private void OnAddCommandExecute()
		{
			IsPressedAdd = true;
			DialogHost.CloseDialogCommand.Execute(null, null);
		}

		public Command ChangeIsCheckedAll { get; private set; }
		private void OnChangeIsCheckedAllExecute()
		{
			needUpdateProjectsCheckBoxIsChecked = false;
			foreach (CheckBox checkBox in projectsCheckBoxCollection)
			{
				checkBox.IsChecked = ProjectsCheckBoxIsChecked;
			}
			needUpdateProjectsCheckBoxIsChecked = true;
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
			checkBox.Checked += ProjectCheckBoxCheckedChange;
			checkBox.Unchecked += ProjectCheckBoxCheckedChange;

			if(context is Project project)
			{
				checkBox.Content = project.Name;
			}
			else if(context is AlternativeProjectName projectName)
			{
				checkBox.Content = projectName.Name;
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

			IEnumerator<CheckBox> projectsCheckBoxCollectionEnum = projectsCheckBoxCollection.Cast<CheckBox>().GetEnumerator();
			if(!projectsCheckBoxCollectionEnum.MoveNext())
			{
				if(ProjectsCheckBoxIsChecked == null)
				{
					ProjectsCheckBoxIsChecked = false;
				}
			}
			else
			{
				bool? isChecked = projectsCheckBoxCollectionEnum.Current.IsChecked;

				while(projectsCheckBoxCollectionEnum.MoveNext())
				{
					if(projectsCheckBoxCollectionEnum.Current.IsChecked != isChecked)
					{
						ProjectsCheckBoxIsChecked = null;
						return;
					}
				}

				ProjectsCheckBoxIsChecked = isChecked;
			}
		}

		private void ProjectCheckBoxCheckedChange(object sender, RoutedEventArgs e)
		{
			UpdateProjectsCheckBoxIsChecked();
		}

		#endregion
	}
}
