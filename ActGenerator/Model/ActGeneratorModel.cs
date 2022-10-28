using Db.Context;
using Db.Context.ActPart;
using Db.Context.BackPart;
using Db.Context.HumanPart;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ActGenerator.Model
{
	class ActGeneratorModel
	{
		DocumentMakerContext db = null;

		~ActGeneratorModel()
		{
			ReleaseContext();
		}

		public async Task ConnectDB()
		{
			await Task.Run(() => { db = new DocumentMakerContext(); });
		}

		public async Task DisconnectDB()
		{
			await Task.Run(ReleaseContext);
		}

		public async Task<List<Project>> LoadProjects()
		{
			return await Task.Run(() => 
			{
				List<Project> projects = new List<Project>(db.Projects);
				foreach(Project project in projects)
				{
					project.AlternativeNames = db.AlternativeProjectNames.Where(x => x.ProjectId == project.Id).ToList();
				}
				return projects;
			});
		}

		public async Task<List<Human>> LoadHumen()
		{
			return await Task.Run(() => new List<Human>(db.Humans));
		}

		public async Task<List<FullWork>> GenerateEnableWorks(List<KeyValuePair<Project, bool[]>> selectedProjectNames)
		{
			return await Task.Run(() =>
			{
				ActPart supportPart = GetActPart(ActPartType.Support);
				List<FullWork> workList = new List<FullWork>();
				List<WorkType> workTypes = new List<WorkType>(db.WorkTypes.Where(x => x.ActPartId == supportPart.Id));
				workTypes.ForEach(x => x.TemplateType = db.TemplateTypes.Where(y => y.Id == x.TemplateTypeId).FirstOrDefault());

				foreach(KeyValuePair<Project, bool[]> projectPair in selectedProjectNames)
				{
					foreach(Episode dbEpisode in db.Episodes.Where(x => x.ProjectId == projectPair.Key.Id).ToList())
					{
						foreach(Back dbBack in db.Backs.Where(x => x.EpisodeId == dbEpisode.Id).ToList())
						{
							dbBack.BackType = db.BackTypes.Where(x => x.Id == dbBack.BackTypeId).FirstOrDefault();

							List<AlternativeProjectName> alternativeProjectNames = new List<AlternativeProjectName>(
								db.AlternativeProjectNames
								.Where(x => x.ProjectId == db.Projects
									.Where(y => y.Id == db.Episodes
										.Where(z => z.Id == dbBack.EpisodeId)
										.FirstOrDefault().ProjectId)
									.FirstOrDefault().Id));

							CountRegions countRegions = db.CountRegions.Where(x => x.BackId == dbBack.Id).FirstOrDefault();

							IEnumerator enableAltNamesEnum = projectPair.Value.GetEnumerator();
							enableAltNamesEnum.MoveNext();
							List<AlternativeProjectName>.Enumerator alternativeProjectNamesEnum = alternativeProjectNames.GetEnumerator();
							AlternativeProjectName currentAltProjectName = null;
							do
							{
								if (enableAltNamesEnum.Current is bool b && !b)
								{
									continue;
								}

								foreach (WorkType supportWorkType in workTypes)
								{
									workList.Add(CreateNewFullWork(dbBack, supportPart, supportWorkType, currentAltProjectName));
									if (countRegions != null)
									{
										workList.Add(CreateNewFullWorkRegions(dbBack, supportPart, supportWorkType, currentAltProjectName, countRegions));
									}
								}

							} while (MoveEnumAndChangeCurrent(ref alternativeProjectNamesEnum, ref currentAltProjectName) && enableAltNamesEnum.MoveNext());
						}
					}
				}

				return workList;
			});
		}

		private void ReleaseContext()
		{
			if (db != null)
			{
				db.Dispose();
				db = null;
			}
		}

		private ActPart GetActPart(ActPartType type)
		{
			foreach(ActPart actPart in db.ActParts)
			{
				if(actPart.Name == type.ToString())
				{
					return actPart;
				}
			}
			ActPart newActPart = new ActPart { Name = type.ToString() };
			db.ActParts.Add(newActPart);
			db.SaveChanges();
			return newActPart;
		}

		private FullWork CreateNewFullWork(Back back, ActPart supportPart, WorkType workType, AlternativeProjectName alternativeProjectName)
		{
			return new FullWork
			{
				Work = new Work
				{
					//Act =
					WorkType = workType,
					ActPart = supportPart,
					WorkBackAdapter = new WorkBackAdapter
					{
						Back = back,
						AlternativeProjectName = alternativeProjectName,
					},
				},
			};
		}

		private FullWork CreateNewFullWorkRegions(Back back, ActPart supportPart, WorkType workType, AlternativeProjectName alternativeProjectName, CountRegions countRegions)
		{
			FullWork work = CreateNewFullWork(back, supportPart, workType, alternativeProjectName);
			if (work.Work.Regions == null)
			{
				work.Work.Regions = new List<Regions>();
			}
			for (int i = 1; i <= countRegions.Count; ++i)
			{
				work.Work.Regions.Add(new Regions { Number = i, Work = work.Work });
			}
			return work;
		}

		private bool MoveEnumAndChangeCurrent(ref List<AlternativeProjectName>.Enumerator listEnum, ref AlternativeProjectName current)
		{
			bool res = listEnum.MoveNext();
			if (res) current = listEnum.Current;
			return res;
		}
	}
}
