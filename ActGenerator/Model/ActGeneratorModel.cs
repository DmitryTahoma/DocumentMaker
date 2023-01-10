using ActGenerator.ViewModel.Dialogs;
using DocumentMakerModelLibrary;
using DocumentMakerModelLibrary.Back;
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

		// contain Project or AlternativeProjectName with reference to Project
		List<IDbObject> projects = null;
		Dictionary<HumanData, IEnumerable<FullDocumentTemplate>> humen = null;

		List<FullDocumentTemplate> documentTemplates = null;
		List<GeneratedWorkList> generatedWorkLists = null;

		public ActGeneratorModel()
		{
			generatingParts.Add(LoadingProjects);
			generatingParts.Add(GeneratingAllWorks);
		}

		public void SetProjects(List<IDbObject> projects)
		{
			this.projects = projects;
		}
		
		public void SetHumen(Dictionary<HumanData, IEnumerable<FullDocumentTemplate>> humen)
		{
			this.humen = humen;
			documentTemplates = humen.SelectMany(x => x.Value).Distinct().ToList();
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
			foreach (IDbObject dbObject in projects)
			{
				if (dialogContext.Dispatcher.Invoke(() => dialogContext.IsClosing)) break;
				dialogContext.Dispatcher.Invoke(() => dialogContext.LabelText = "Завантаження проєкту \"" + dbObject.ToString() + '"');

				Project project = dbObject as Project ?? ((AlternativeProjectName)dbObject).Project;

				Project prevLoadedProject = GetPrevProject(dbObject);
				if (prevLoadedProject != null)
				{
					project.Backs = prevLoadedProject.Backs;
				}
				else
				{
					await LoadProject(project, dialogContext);
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

		private Project GetPrevProject(IDbObject currentDbObject)
		{
			int? projectId = currentDbObject is Project project ? project.Id : ((AlternativeProjectName)currentDbObject).ProjectId;

			foreach (IDbObject loadedDbObject in projects)
			{
				if (loadedDbObject == currentDbObject)
				{
					return null;
				}
				Project loadedProject = loadedDbObject as Project ?? ((AlternativeProjectName)loadedDbObject).Project;

				if (projectId == loadedProject.Id)
				{
					return loadedProject;
				}
			}
			return null;
		}

		private async Task GeneratingAllWorks(GenerationDialogViewModel dialogContext)
		{
			generatedWorkLists = new List<GeneratedWorkList>();
			double progressPart = dialogContext.Dispatcher.Invoke(() => dialogContext.ProgressMaximum)
				/ generatingParts.Count
				/ projects.SelectMany(x => x is Project project ? project.Backs : ((AlternativeProjectName)x).Project.Backs).Count()
				/ documentTemplates.SelectMany(y => y.ReworkWorkTypesList).Count();

			foreach (IDbObject dbObject in projects)
			{
				Project project = dbObject as Project;
				if (project == null) project = ((AlternativeProjectName)dbObject).Project;

				dialogContext.Dispatcher.Invoke(() => dialogContext.LabelText = "Генерація робіт проєкту \"" + dbObject.ToString() + '"');
				GeneratedWorkList generatedWorkList = new GeneratedWorkList
				{
					Project = project,
				};

				Project prevProject = GetPrevProject(dbObject);
				if (prevProject != null)
				{
					foreach(GeneratedWorkList prevWorkList in generatedWorkLists)
					{
						if(prevProject.Id == (prevWorkList.Project is AlternativeProjectName alternativeProjectName ? alternativeProjectName.ProjectId : prevWorkList.Project.Id))
						{
							generatedWorkList.GeneratedWorks = prevWorkList.GeneratedWorks;
							double skippedProgress = progressPart * generatedWorkList.GeneratedWorks.Count;
							dialogContext.Dispatcher.Invoke(() => dialogContext.ProgressValue += skippedProgress);
							await Task.Delay(1);
							break;
						}
					}
				}
				else
				{
					foreach (Back back in project.Backs)
					{
						if (dialogContext.Dispatcher.Invoke(() => dialogContext.IsClosing)) break;

						foreach (FullDocumentTemplate documentTemplate in documentTemplates)
						{
							foreach (WorkObject workObject in documentTemplate.ReworkWorkTypesList)
							{
								generatedWorkList.GeneratedWorks.Add(new GeneratedWork
								{
									WorkObject = workObject,
									Back = back,
								});

								dialogContext.Dispatcher.Invoke(() => dialogContext.ProgressValue += progressPart);
								await Task.Delay(1);
							}
						}
					}

					generatedWorkLists.Add(generatedWorkList);
				}
			}
		}
	}
}
