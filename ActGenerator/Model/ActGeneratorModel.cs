using ActGenerator.ViewModel.Dialogs;
using DocumentMakerModelLibrary;
using DocumentMakerModelLibrary.OfficeFiles.Human;
using ProjectsDb;
using ProjectsDb.Context;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ActGenerator.Model
{
	class ActGeneratorModel : ProjectsDbConnector
	{
		delegate Task GeneratingPart(GenerationDialogViewModel dialogContext);

		List<GeneratingPart> generatingParts = new List<GeneratingPart>();

		List<IDbObject> projects = null;
		IEnumerable<FullDocumentTemplate> documentTemplates = null;
		Dictionary<HumanData, IEnumerable<FullDocumentTemplate>> humen = null;

		public ActGeneratorModel()
		{
			generatingParts.Add(LoadingProjects);
		}

		public void SetProjects(List<IDbObject> projects)
		{
			this.projects = projects;
		}
		
		public void SetHumen(Dictionary<HumanData, IEnumerable<FullDocumentTemplate>> humen)
		{
			this.humen = humen;
		}

		public void SetTemplates(IEnumerable<FullDocumentTemplate> documentTemplates)
		{
			this.documentTemplates = documentTemplates;
		}

		public async Task StartGeneration(GenerationDialogViewModel dialogContext)
		{
			while(!dialogContext.Dispatcher.Invoke(() => dialogContext.IsClosing) && !dialogContext.Dispatcher.Invoke(() => dialogContext.GenerationStarted))
			{
				await Task.Delay(1);
			}

			if(dialogContext.Dispatcher.Invoke(() => dialogContext.GenerationStarted))
			{
				foreach(GeneratingPart generatingPart in generatingParts)
				{
					await generatingPart(dialogContext);

					if(dialogContext.IsClosing)
					{
						return;
					}
				}

				dialogContext.Dispatcher.Invoke(() => dialogContext.LabelText = "Згенеровано!");
			}
		}

		private async Task LoadingProjects(GenerationDialogViewModel dialogContext)
		{
			await ConnectDbAsync();

			double progressPart = dialogContext.Dispatcher.Invoke(() => dialogContext.ProgressMaximum) / generatingParts.Count / projects.Count;
			foreach (IDbObject project in projects)
			{
				if (dialogContext.Dispatcher.Invoke(() => dialogContext.IsClosing)) break;
				dialogContext.Dispatcher.Invoke(() => dialogContext.LabelText = "Завантаження проєкту \"" + project.ToString() + '"');

				if (project is Project proj)
				{
					await LoadProject(proj, dialogContext);
				}
				else if(project is AlternativeProjectName altProjectName && projects.Where(x => x is Project).FirstOrDefault(y => y.Id == altProjectName.ProjectId) == null)
				{
					await LoadProject(altProjectName.Project, dialogContext);
				}

				dialogContext.Dispatcher.Invoke(() => dialogContext.ProgressValue += progressPart);
			}

			await DisconnectDbAsync();
		}

		private async Task LoadProject(Project project, GenerationDialogViewModel dialogContext)
		{
			project.Backs = new List<Back>();
			foreach (Back back in db.Backs.Where(x => x.ProjectId == project.Id))
			{
				if (dialogContext.Dispatcher.Invoke(() => dialogContext.IsClosing)) return;

				project.Backs.Add(back);
				await Task.Delay(1);
			}
		}
	}
}
