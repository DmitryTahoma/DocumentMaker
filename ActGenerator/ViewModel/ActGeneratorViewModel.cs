using ActGenerator.Model;
using ActGenerator.View.Dialogs;
using ActGenerator.ViewModel.Dialogs;
using Db.Context.BackPart;
using Dml.Model.Template;
using MaterialDesignThemes.Wpf;
using Mvvm;
using Mvvm.Commands;
using System.Collections.Generic;

namespace ActGenerator.ViewModel
{
	class ActGeneratorViewModel
	{
		public ActGeneratorViewModel()
		{
			InitCommands();

			ProjectsList = new ObservableRangeCollection<string>
			{
				"Lost Lands 8",
				"Tricky doors",
				"Lost Lands Stories",
				"Legendary Tales 3",
			};
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

		public ObservableRangeCollection<string> ProjectsList { get; private set; } = new ObservableRangeCollection<string>();

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
		}

		public Command AddProjectCommand { get; private set; }
		private async void OnAddProjectCommandExecute()
		{
			ListSelector listSelector = new ListSelector();
			ListSelectorViewModel listSelectorViewModel = (ListSelectorViewModel)listSelector.DataContext;
			listSelectorViewModel.ItemsDisplayMemberPath = nameof(Project.Name);
			listSelectorViewModel.SetItems(new List<Project> 
			{
				new Project { Name="Project1" }, 
				new Project { Name="Project2" }, 
				new Project { Name="Project3" }, 
				new Project { Name="Project4" }, 
			});
			await DialogHost.Show(listSelector, DialogHostId);
		}

		public Command CloseOpenedDialog { get; private set; }
		private void OnCloseOpenedDialogExecute()
		{
			if(DialogHost.IsDialogOpen(DialogHostId))
			{
				DialogHost.Close(DialogHostId);
			}
		}

		#endregion
	}
}
