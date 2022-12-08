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
		Dictionary<CheckBox, List<CheckBox>> dictionaryCheckBoxes = new Dictionary<CheckBox, List<CheckBox>>();

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

			if (!model.ConnectionStringSetted) return;

			if (State == ViewModelState.Initialized)
			{
				State = ViewModelState.Loading;
				await model.ConnectDbAsync();
				IEnumerable<Project> projects = await model.LoadProjectsAsync();
				await model.DisconnectDbAsync();
				State = ViewModelState.Loaded;

				projectsCheckBoxCollection.Clear();
				foreach (Project project in projects)
				{
					KeyValuePair<CheckBox, List<CheckBox>> checkBoxes = CreateProjectCheckBoxes(project);
					dictionaryCheckBoxes.Add(checkBoxes.Key, checkBoxes.Value);
					UpdateProjectsCheckBoxIsChecked();
					await Task.Delay(1);
				}
			}
			else if (State == ViewModelState.Loaded)
			{
				State = ViewModelState.Loading;
				await model.ConnectDbAsync();
				IEnumerable<Project> projects = await model.LoadProjectsAsync();
				await model.DisconnectDbAsync();

				List<CheckBox> removedCheckBoxes = projectsCheckBoxCollection.Cast<CheckBox>().ToList();
				foreach (Project project in projects)
				{
					bool finded = false;
					foreach (CheckBox checkBox in projectsCheckBoxCollection)
					{
						if (checkBox.DataContext is Project proj && proj.Id == project.Id)
						{
							checkBox.DataContext = project;
							checkBox.Content = project.Name;
							removedCheckBoxes.Remove(checkBox);
							finded = true;
							break;
						}
					}

					if (!finded)
					{
						KeyValuePair<CheckBox, List<CheckBox>> checkBoxes = CreateProjectCheckBoxes(project);
						dictionaryCheckBoxes.Add(checkBoxes.Key, checkBoxes.Value);
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

		private KeyValuePair<CheckBox, List<CheckBox>> CreateProjectCheckBoxes(Project context)
		{
			KeyValuePair<CheckBox, List<CheckBox>> result = new KeyValuePair<CheckBox, List<CheckBox>>(CreateProjectCheckBox(context), new List<CheckBox>());
			result.Value.Add(CreateProjectCheckBox(context));
			projectsCheckBoxCollection.Add(result.Key);
			result.Key.Checked += ProjectCheckBoxCheckedChange;
			result.Key.Unchecked += ProjectCheckBoxCheckedChange;
			foreach (AlternativeProjectName alternativeProjectName in context.AlternativeNames)
			{
				result.Value.Add(CreateProjectCheckBox(alternativeProjectName));
			}

			foreach (CheckBox checkBox in result.Value)
			{
				projectNamesCheckBoxCollection.Add(checkBox);
				checkBox.Checked += ProjectNameCheckBoxCheckedChange;
				checkBox.Unchecked += ProjectNameCheckBoxCheckedChange;
				checkBox.Visibility = Visibility.Collapsed;
			}

			return result;
		}

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
			}
			else if (context is AlternativeProjectName projectName)
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
			CheckBox s = sender as CheckBox;
			bool isChecked = true;
			foreach (CheckBox checkBox in dictionaryCheckBoxes[s])
			{
				checkBox.Visibility = (s.IsChecked.HasValue && s.IsChecked.Value) ? Visibility.Visible : Visibility.Collapsed;
				checkBox.IsChecked = isChecked;
				isChecked = false;
			}

			UpdateProjectsCheckBoxIsChecked();
			UpdateProjectNamesCheckBoxIsChecked();
		}

		private void ProjectNameCheckBoxCheckedChange(object sender, RoutedEventArgs e)
		{
			UpdateProjectNamesCheckBoxIsChecked();
		}

		#endregion
	}
}
