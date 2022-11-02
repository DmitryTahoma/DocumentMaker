using Db.Context;
using Db.Context.ActPart;
using Db.Context.BackPart;
using Db.Context.HumanPart;
using Dml;
using DocumentMakerModelLibrary;
using DocumentMakerModelLibrary.Controls;
using DocumentMakerModelLibrary.Files;
using ProjectEditorLib.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DmlBack = Dml.Model.Back;

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

		public Task RemoveUsedWorks(List<FullWork> works, bool canUseOldWorks, DateTime dateDiff)
		{
			return Task.Run(() => 
			{
				DateTime opornDate = DateTime.Now.AddYears(-dateDiff.Year + 1).AddMonths(-dateDiff.Month + 1).AddDays(-dateDiff.Day + 1);

				List<FullWork> removeWorks = (canUseOldWorks 
					? db.FullWorks
						.Where(x => db.Acts
							.FirstOrDefault(y => y.Id == db.Works
								.FirstOrDefault(z => z.Id == x.WorkId).ActId)
							.CreationTime > opornDate)
					: db.FullWorks)
					.ToList();
				removeWorks
					.ForEach(a => a.Work = db.Works
						.First(b => b.Id == a.WorkId));
				removeWorks
					.ForEach(c => c.Work.WorkBackAdapter = db.WorkBackAdapters
						.First(d => d.Id == c.Work.WorkBackAdapterId));
				removeWorks
					.ForEach(e => e.Work.WorkBackAdapter.Back = db.Backs
						.FirstOrDefault(f => f.Id == e.Work.WorkBackAdapter.BackId));
				removeWorks
					.ForEach(g => g.Work.Regions = new List<Regions>(db.Regions
						.Where(h => h.WorkId == g.WorkId)));

				works.RemoveAll(j => 
				{
					foreach(FullWork removeWork in removeWorks)
					{
						if (removeWork.Work.WorkTypeId == j.Work.WorkType.Id
						&& removeWork.Work.WorkBackAdapter.BackId == j.Work.WorkBackAdapter.Back.Id
						&& (removeWork.Work.WorkBackAdapter.BackId != null 
							|| removeWork.Work.WorkBackAdapter.Text == j.Work.WorkBackAdapter.Text)
						&& ((removeWork.Work.WorkBackAdapter.AlternativeProjectNameId == null 
							&& j.Work.WorkBackAdapter.AlternativeProjectName == null)
							|| (j.Work.WorkBackAdapter.AlternativeProjectName != null 
								&& removeWork.Work.WorkBackAdapter.AlternativeProjectNameId == j.Work.WorkBackAdapter.AlternativeProjectName.Id)))
						{
							if (removeWork.Work.Regions != null && j.Work.Regions != null)
							{
								if(removeWork.Work.Regions.Count != j.Work.Regions.Count)
								{
									RemoveRegions(j.Work.Regions, removeWork.Work.Regions.Select(k => k.Number).ToList());
									removeWorks.Remove(removeWork);
									return false;
								}

								List<Regions>.Enumerator removeWorkRegionsEnum = removeWork.Work.Regions.GetEnumerator();
								List<Regions>.Enumerator jRegionsEnum = j.Work.Regions.GetEnumerator();

								while(removeWorkRegionsEnum.MoveNext() && jRegionsEnum.MoveNext())
								{
									if(removeWorkRegionsEnum.Current.Number != jRegionsEnum.Current.Number)
									{
										RemoveRegions(j.Work.Regions, removeWork.Work.Regions.Select(k => k.Number).ToList());
										removeWorks.Remove(removeWork);
										return false;
									}
								}
							}
							removeWorks.Remove(removeWork);
							return true;
						}
					}
					return false; 
				});
			});
		}

		public async Task SyncCollection<T>(ICollection<T> collection) where T : class, IDbObject
		{
			await db.SyncCollection(collection);
		}

		public delegate bool GenerateActsCallback(HumanDataContext humanDataContext);
		public List<KeyValuePair<HumanDataContext, List<FullWork>>> GenerateActs(List<FullWork> enableWorks, IList<HumanDataContext> humen, int minSum, int maxSum, GenerateActsCallback lessWorksCallback, GenerateActsCallback unrealSumCallback)
		{
			List<KeyValuePair<HumanDataContext, List<FullWork>>> acts = new List<KeyValuePair<HumanDataContext, List<FullWork>>>();
			foreach (HumanDataContext human in humen)
			{
				List<FullWork> currentWorks = enableWorks.Where(x => x.Work.WorkType.TemplateType.Name == human.Template.Type.ToString()).ToList();
				int sum = int.Parse(human.SumText);
				KeyValuePair<HumanDataContext, List<FullWork>> act =
					GenerateActWorks(human, ref sum, minSum, maxSum,
						enableWorks
							.Where(x => x.Work.WorkType.TemplateType.Name == human.Template.Type.ToString())
							.ToList());

				if (sum > 0)
				{
					if (lessWorksCallback.Invoke(human))
					{
						enableWorks.AddRange(act.Value);
						continue;
					}
					else
					{
						return null;
					}
				}

				if (!TryMixSumInWorks(act, ref sum, minSum, maxSum))
				{
					if (unrealSumCallback.Invoke(human))
					{
						break;
					}
					else
					{
						return null;
					}
				}
				else
				{
					acts.Add(act);
				}
			}
			return acts;
		}

		public void ExportActs(string path, List<KeyValuePair<HumanDataContext, List<FullWork>>> acts, List<Project> projects)
		{
			DocumentMakerModel documentMakerModel = new DocumentMakerModel();
			if(documentMakerModel.TryLoadReworkWorkTypes(Path.Combine(PathHelper.ExecutingAssemblyPath, "SupportTypes.xlsx")))
			{
				foreach(var act in acts)
				{
					DocumentMakerModel actModel = new DocumentMakerModel
					{
						SelectedHuman = act.Key.FullName,
						ActSum = act.Key.SumText,
						TemplateType = act.Key.Template.Type
					};
					actModel.ExportDcmk(Path.Combine(path, act.Key.FullName + DcmkFile.Extension),
						act.Value.Select(x =>
						{
							bool isRegions = x.Work.Regions != null && x.Work.Regions.Count > 0;

							FullBackDataModel backModel = new FullBackDataModel
							{
								IsRework = true,
								EpisodeNumberText = x.Work.WorkBackAdapter.Back.Episode.Number.ToString(),
								BackName = x.Work.WorkBackAdapter.Back.Name,
								GameName = x.Work.WorkBackAdapter.AlternativeProjectName?.Name ?? projects.First(y => y.Id == x.Work.WorkBackAdapter.Back.Episode.ProjectId).Name,
								SpentTimeText = x.Sum.ToString(),
								SumText = x.Sum.ToString(),
								BackNumberText = x.Work.WorkBackAdapter.Back.Number.ToString(),
								Type = ParseAndConvertProjectNodeTypeToBackType(x.Work.WorkBackAdapter.Back.BackType.Name, isRegions),
								WorkObjectId = documentMakerModel.DocumentTemplatesList.First(z => z.Type == act.Key.Template.Type).ReworkWorkTypesList.First(a => a.Name == x.Work.WorkType.Name).Id,
							};

							if(isRegions)
							{
								foreach(Regions regions in x.Work.Regions)
								{
									backModel.BackCountRegionsText += regions.Number.ToString() + ',';
								}
							}

							return backModel;
						}));
				}
			}
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

		private void RemoveRegions(List<Regions> regions, List<int> removeNumbers)
		{
			regions.RemoveAll(x => removeNumbers.Remove(x.Number));
		}

		private KeyValuePair<HumanDataContext, List<FullWork>> GenerateActWorks(HumanDataContext human, ref int sum, int minSum, int maxSum, List<FullWork> works)
		{
			Random random = new Random();

			List<FullWork> actWorks = new List<FullWork>();
			while(sum > 0 && works.Count > 0)
			{
				int index = random.Next(0, works.Count);
				FullWork randWork = works[index];
				randWork.Sum = random.Next(minSum, maxSum);
				sum -= randWork.Sum;
				works.Remove(randWork);
				actWorks.Add(randWork);
			}

			return new KeyValuePair<HumanDataContext, List<FullWork>>(human, actWorks);
		}

		private bool TryMixSumInWorks(KeyValuePair<HumanDataContext, List<FullWork>> act, ref int sum, int minSum, int maxSum)
		{
			int counter = maxSum;
			bool speedUpMultiplier = true;
			int dir = (sum > 0 ? 1 : -1);
			while (sum != 0)
			{
				int step = (speedUpMultiplier && sum > 100 ? (sum / 100) : 1) * dir;
				bool changedOne = false;
				foreach(FullWork work in act.Value)
				{
					if((sum > 0 && work.Sum + step <= maxSum)
						|| (sum <= 0 && work.Sum + step >= minSum))
					{
						work.Sum += step;
						sum -= step;
						changedOne = true;

						if(sum == 0)
						{
							break;
						}
					}
				}

				if(!changedOne)
				{
					if(speedUpMultiplier)
					{
						speedUpMultiplier = false;
					}
					else
					{
						return false;
					}
				}

				--counter;
				if(counter <= 0)
				{
					return false;
				}
			}

			return true;
		}

		private DmlBack.BackType ConvertProjectNodeTypeToBackType(ProjectNodeType projectNodeType, bool isRegions)
		{
			switch (projectNodeType)
			{
				case ProjectNodeType.Back: return isRegions ? DmlBack.BackType.Regions : DmlBack.BackType.Back;
				case ProjectNodeType.Hog: return isRegions ? DmlBack.BackType.HogRegions : DmlBack.BackType.Hog;
				case ProjectNodeType.Minigame: return DmlBack.BackType.Mg;
				case ProjectNodeType.Craft: return DmlBack.BackType.Craft;
				case ProjectNodeType.Dialog: return DmlBack.BackType.Dialog;
			}
			throw new ArgumentException("cannot convert to Dml.Model.Back.BackType");
		}

		private DmlBack.BackType ParseAndConvertProjectNodeTypeToBackType(string str, bool isRegions)
		{
			return ConvertProjectNodeTypeToBackType((ProjectNodeType)Enum.Parse(typeof(ProjectNodeType), str), isRegions);
		}
	}
}
