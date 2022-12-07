using ActGenerator.View.Controls;
using ActGenerator.View.Dialogs;
using ActGenerator.ViewModel.Dialogs;
using MaterialDesignThemes.Wpf;
using Mvvm.Commands;

namespace ActGenerator.ViewModel.Controls
{
	public class ProjectNamesListControlViewModel
	{
		AddProjectNameDialog addProjectNameDialog = null;
		AddProjectNameDialogViewModel addProjectNameDialogViewModel = null;

		public ProjectNamesListControlViewModel()
		{
			addProjectNameDialog = new AddProjectNameDialog();
			addProjectNameDialogViewModel = (AddProjectNameDialogViewModel)addProjectNameDialog.DataContext;

			InitCommands();
		}

		#region Properties

		public string DialogHostId { get; set; } = null;

		#endregion

		#region Commands

		private void InitCommands()
		{
			OpenAddProjectNameDialog = new Command(OnOpenAddProjectNameDialogExecute);
		}

		public Command OpenAddProjectNameDialog { get; private set; }
		private async void OnOpenAddProjectNameDialogExecute()
		{
			//listSelectorViewModel.ItemsDisplayMemberPath = nameof(Project.Name);
			//List<Project> projects = new List<Project>(dbProjects.Where(x => !ProjectsList.Contains(x)));
			//projects.RemoveAll(x => ProjectsList.Contains(x));
			//listSelectorViewModel.SetItems(projects);
			await DialogHost.Show(addProjectNameDialog, DialogHostId);
			//if (IsOpenActGeneratorDialogHost)
			//{
				//if (listSelectorViewModel.IsAddingPressed && listSelectorViewModel.SelectedItems != null)
				//{
				//	listSelectorViewModel.SelectedItems
				//		.Cast<Project>()
				//		.ToList()
				//		.ForEach(AddProjectToStack);
				//}
			//}
		}

		#endregion
	}
}
