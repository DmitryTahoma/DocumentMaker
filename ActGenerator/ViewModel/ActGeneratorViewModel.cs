using ActGenerator.Model;
using ActGenerator.View.Dialogs;
using ActGenerator.ViewModel.Dialogs;
using Db.Context.BackPart;
using Dml.Model.Template;
using MaterialDesignThemes.Wpf;
using Mvvm;
using Mvvm.Commands;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ActGenerator.ViewModel
{
	class ActGeneratorViewModel
	{
		ActGeneratorModel model = new ActGeneratorModel();
		List<Project> dbProjects = null;

		public ActGeneratorViewModel()
		{
			InitCommands();

			HumanList = new ObservableRangeCollection<HumanDataContext>
			{
				new HumanDataContext("Алєйникова Марина"),
				new HumanDataContext("Баєв Олександр"),
				new HumanDataContext("Байдуж Максим"),
				new HumanDataContext("Байдуж Марина"),
			};
		}

		#region Properties

		public string DialogHostId { get; } = "ActGeneratorDialogHost";

		public ObservableRangeCollection<Project> ProjectsList { get; private set; } = new ObservableRangeCollection<Project>();

		public ObservableRangeCollection<HumanDataContext> HumanList { get; private set; } = new ObservableRangeCollection<HumanDataContext>();

		public ObservableRangeCollection<DocumentTemplate> DocumentTemplates { get; private set; } = new ObservableRangeCollection<DocumentTemplate> 
		{
			new DocumentTemplate("Скриптувальник", DocumentTemplateType.Scripter),
			new DocumentTemplate("Технічний дизайнер", DocumentTemplateType.Cutter),
			new DocumentTemplate("Художник", DocumentTemplateType.Painter),
			new DocumentTemplate("Моделлер", DocumentTemplateType.Modeller),
		};

		#endregion

		#region Commands

		private void InitCommands()
		{
			AddProjectCommand = new Command(OnAddProjectCommandExecute);
			CloseOpenedDialog = new Command(OnCloseOpenedDialogExecute);
			LoadFromDatabase = new Command(OnLoadFromDatabaseExecute);
			RemoveProjectCommand = new Command<IList>(OnRemoveProjectCommandExecute);
		}

		public Command AddProjectCommand { get; private set; }
		private async void OnAddProjectCommandExecute()
		{
			ListSelector listSelector = new ListSelector();
			ListSelectorViewModel listSelectorViewModel = (ListSelectorViewModel)listSelector.DataContext;
			listSelectorViewModel.ItemsDisplayMemberPath = nameof(Project.Name);
			List<Project> projects = new List<Project>(dbProjects.Where(x => !ProjectsList.Contains(x)));
			projects.RemoveAll(x => ProjectsList.Contains(x));
			listSelectorViewModel.SetItems(projects);
			await DialogHost.Show(listSelector, DialogHostId);
			if(listSelectorViewModel.SelectedItems != null)
			{
				ProjectsList.AddRange(listSelectorViewModel.SelectedItems.Cast<Project>());
			}
		}

		public Command<IList> RemoveProjectCommand { get; private set; }
		private void OnRemoveProjectCommandExecute(IList selectedItems)
		{
			ProjectsList.RemoveRange(selectedItems.Cast<Project>());
		}

		public Command CloseOpenedDialog { get; private set; }
		private void OnCloseOpenedDialogExecute()
		{
			if(DialogHost.IsDialogOpen(DialogHostId))
			{
				DialogHost.Close(DialogHostId);
			}
		}

		public Command LoadFromDatabase { get; private set; }
		private async void OnLoadFromDatabaseExecute()
		{
			if (dbProjects == null)
			{
				await model.ConnectDB();
				dbProjects = await model.LoadProjects();
				await model.DisconnectDB();
			}
		}

		#endregion
	}
}
