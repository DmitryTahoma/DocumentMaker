using MaterialDesignThemes.Wpf;
using Mvvm.Commands;
using ProjectsDb.Context;
using System.Windows;
using System.Windows.Controls;

namespace ActGenerator.ViewModel.Dialogs
{
	public class AddProjectNameDialogViewModel
	{
		readonly Style itemStyle = Application.Current.FindResource("AddProjectNameListItemCheckBox") as Style;

		UIElementCollection projectsCheckBoxCollection = null;
		UIElementCollection projectNamesCheckBoxCollection = null;

		public AddProjectNameDialogViewModel()
		{
			InitCommands();
		}

		#region Properties

		public bool IsPressedAdd { get; private set; }

		#endregion

		#region Commands

		private void InitCommands()
		{
			BindProjectsCheckBoxList = new Command<UIElementCollection>(OnBindProjectsCheckBoxListExecute);
			BindProjectNamesCheckBoxList = new Command<UIElementCollection>(OnBindProjectNamesCheckBoxListExecute);
			ViewLoaded = new Command(OnViewLoadedExecute);
			AddCommand = new Command(OnAddCommandExecute);
		}

		public Command<UIElementCollection> BindProjectsCheckBoxList { get; private set; }
		private void OnBindProjectsCheckBoxListExecute(UIElementCollection projectsCheckBoxCollection)
		{
			if (this.projectsCheckBoxCollection == null)
			{
				this.projectsCheckBoxCollection = projectsCheckBoxCollection;

				projectsCheckBoxCollection.Add(CreateProjectCheckBox(new Project { Name = "Darkness and Flame 2" }));
				projectsCheckBoxCollection.Add(CreateProjectCheckBox(new Project { Name = "Darkness and Flame 3" }));
				projectsCheckBoxCollection.Add(CreateProjectCheckBox(new Project { Name = "Darkness and Flame 4" }));
				projectsCheckBoxCollection.Add(CreateProjectCheckBox(new Project { Name = "Escape" }));
				projectsCheckBoxCollection.Add(CreateProjectCheckBox(new Project { Name = "New York Mysteries 4" }));
				projectsCheckBoxCollection.Add(CreateProjectCheckBox(new Project { Name = "Lost Lands Stories" }));
				projectsCheckBoxCollection.Add(CreateProjectCheckBox(new Project { Name = "Lost Lands 4" }));
				projectsCheckBoxCollection.Add(CreateProjectCheckBox(new Project { Name = "Lost Lands 6" }));
				projectsCheckBoxCollection.Add(CreateProjectCheckBox(new Project { Name = "Lost Lands 7" }));
				projectsCheckBoxCollection.Add(CreateProjectCheckBox(new Project { Name = "Lost Lands 8" }));
				projectsCheckBoxCollection.Add(CreateProjectCheckBox(new Project { Name = "Legendary Tales 1" }));
				projectsCheckBoxCollection.Add(CreateProjectCheckBox(new Project { Name = "Legendary Tales 2" }));
				projectsCheckBoxCollection.Add(CreateProjectCheckBox(new Project { Name = "Legendary Tales 3" }));
				projectsCheckBoxCollection.Add(CreateProjectCheckBox(new Project { Name = "The Legacy 1" }));
				projectsCheckBoxCollection.Add(CreateProjectCheckBox(new Project { Name = "The Legacy 3" }));
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
		private void OnViewLoadedExecute()
		{
			IsPressedAdd = false;
		}

		public Command AddCommand { get; private set; }
		private void OnAddCommandExecute()
		{
			IsPressedAdd = true;
			DialogHost.CloseDialogCommand.Execute(null, null);
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

		#endregion
	}
}
