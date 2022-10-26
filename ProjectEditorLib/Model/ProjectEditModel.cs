using Db.Context;
using Db.Context.ActPart;
using Db.Context.BackPart;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectEditorLib.Model
{
	public class ProjectEditModel
	{
		DocumentMakerContext db = null;

		~ProjectEditModel()
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

		public async Task<IEnumerable<Project>> LoadProjects()
		{
			return await Task.Run(() => new List<Project>(db.Projects));
		}

		public async Task<Project> CreateProject(Project project)
		{
			return await Task.Run(() =>
			{
				project = db.Projects.Add(project);
				db.SaveChanges();
				return project;
			});
		}

		private void ReleaseContext()
		{
			if(db != null)
			{
				db.Dispose();
				db = null;
			}
		}

		public Task SaveNodeChanges(ProjectNode projectNode)
		{
			return Task.Run(async () =>
			{
				switch (projectNode.Type)
				{
					case ProjectNodeType.Project: await SaveProjectChanges((Project)projectNode.Context); break;
					case ProjectNodeType.Episode: await SaveEpisodeChanges((Episode)projectNode.Context); break;
					case ProjectNodeType.Back:
					case ProjectNodeType.Craft:
					case ProjectNodeType.Minigame:
					case ProjectNodeType.Dialog:
					case ProjectNodeType.Hog:
						await SaveBackChanges((Back)projectNode.Context, projectNode.Type); break;
					case ProjectNodeType.Regions: await SaveCountRegionsChanges((CountRegions)projectNode.Context); break;
				}
			});
		}

		private Task SaveProjectChanges(Project project)
		{
			return Task.Run(() =>
			{
				Project dbProject = db.Projects.First(x => x.Id == project.Id);
				dbProject.Set(project);

				List<AlternativeProjectName> dbAltNames = db.AlternativeProjectNames.Where(x => x.ProjectId == project.Id).ToList();
				List<AlternativeProjectName> altNamesToRemove = dbAltNames.ToList();
				foreach(AlternativeProjectName projName in dbAltNames)
				{
					if(project.AlternativeNames.FirstOrDefault(x => x.Id == projName.Id) != null)
					{
						altNamesToRemove.Remove(projName);
					}
				}
				db.AlternativeProjectNames.RemoveRange(altNamesToRemove);

				foreach (AlternativeProjectName projName in project.AlternativeNames)
				{
					if (projName.Id == 0)
					{
						db.AlternativeProjectNames.Add(projName);
					}
					else
					{
						db.AlternativeProjectNames.FirstOrDefault(x => x.Id == projName.Id).Set(projName);
					}
				}

				db.SaveChanges();
			});
		}

		private Task SaveEpisodeChanges(Episode episode)
		{
			return Task.Run(() =>
			{
				Episode dbEpisode = db.Episodes.FirstOrDefault(x => x.Id == episode.Id);
				if (dbEpisode == null)
				{
					episode = db.Episodes.Add(episode);
				}
				else
				{
					dbEpisode.Set(episode);
				}
				db.SaveChanges();
			});
		}

		private Task SaveBackChanges(Back back, ProjectNodeType projectNodeType)
		{
			return Task.Run(() =>
			{
				BackType backType = db.BackTypes.FirstOrDefault(x => x.Name == projectNodeType.ToString());
				bool isNewBackType = backType == null;
				if(isNewBackType)
				{
					backType = db.BackTypes.Add(new BackType { Name = projectNodeType.ToString() });
				}

				Back dbBack = db.Backs.FirstOrDefault(x => x.Id == back.Id);
				if(dbBack == null)
				{
					dbBack = db.Backs.Add(back);
				}
				else
				{
					dbBack.Set(back);

					if (back.Regions != null && back.Regions.Count > 0)
					{
						CountRegions regions = db.CountRegions.FirstOrDefault(x => x.BackId == dbBack.Id);
						if (regions == null)
						{
							regions = db.CountRegions.Add(new CountRegions());
						}

						regions.Set(back.Regions.FirstOrDefault());
					}
				}

				if (isNewBackType)
					dbBack.BackType = backType;
				else
					dbBack.BackTypeId = backType.Id;

				db.SaveChanges();
			});
		}

		private Task SaveCountRegionsChanges(CountRegions countRegions)
		{
			return Task.Run(() =>
			{
				CountRegions dbCountRegions = db.CountRegions.FirstOrDefault(x => x.Id == countRegions.Id);
				if(dbCountRegions == null)
				{
					dbCountRegions = db.CountRegions.Add(countRegions);
				}
				else
				{
					dbCountRegions.Set(countRegions);
				}
				db.SaveChanges();
			});
		}

		public Task<Project> LoadProject(Project project)
		{
			return Task.Run(async () =>
			{
				Project dbProject = db.Projects.First(x => x.Id == project.Id);
				project.Episodes = new List<Episode>(db.Episodes.Where(x => x.ProjectId == project.Id));
				project.AlternativeNames = new List<AlternativeProjectName>(db.AlternativeProjectNames.Where(x => x.ProjectId == project.Id));
				foreach(Episode episode in project.Episodes)
				{
					List<Back> backs = new List<Back>(db.Backs.Where(x => x.EpisodeId == episode.Id && x.BaseBackId == null));
					foreach (Back back in backs)
					{
						await LoadBack(back);
					}
					episode.Backs = backs;
				}
				return project;
			});
		}

		private async Task LoadBack(Back back)
		{
			await Task.Run(async () =>
			{
				back.BackType = db.BackTypes.FirstOrDefault(x => x.Id == back.BackTypeId);
				back.Regions = new List<CountRegions>(db.CountRegions.Where(x => x.BackId == back.Id));
				back.ChildBacks = new List<Back>(db.Backs.Where(x => x.BaseBackId == back.Id));

				foreach (Back childBack in back.ChildBacks)
				{
					await LoadBack(childBack);
				}
			});
		}

		public Task<bool> RemoveNode(IDbObject node)
		{
			return Task.Run(() =>
			{
				List<Back> removingBacks = null;

				Episode episode = node as Episode;
				if (episode != null)
				{
					removingBacks = episode.Backs;
					if(removingBacks == null)
					{
						removingBacks = new List<Back>();
					}
				}
				else if (node is Back back)
				{
					removingBacks = new List<Back> { back };
				}

				if (removingBacks == null)
				{
					if(node is CountRegions regions)
					{
						CountRegions dbRegions = db.CountRegions
							.FirstOrDefault(x => x.Id == regions.Id);

						db.CountRegions.Remove(dbRegions);
						db.SaveChanges();
						return true;
					}
				}
				else
				{
					PushChildBacks(ref removingBacks);

					bool canRemove = CanRemoveAll(removingBacks);

					if (canRemove)
					{
						List<CountRegions> removingDbRegions = new List<CountRegions>(GetDbRegionsOfBacks(removingBacks));
						List<Back> removingDbBacks = new List<Back>(GetDbBacksOfBacks(removingBacks));

						db.CountRegions.RemoveRange(removingDbRegions);
						db.Backs.RemoveRange(removingDbBacks);

						if (episode != null)
						{
							db.Episodes
							.Remove(db.Episodes
								.FirstOrDefault(x => x.Id == episode.Id));
						}

						db.SaveChanges();
					}

					return canRemove;
				}
				return false;
			});
		}

		private bool CanRemoveAll(IEnumerable<Back> backs)
		{
			foreach (WorkBackAdapter workBackAdapter in db.WorkBackAdapters)
			{
				foreach (Back removingBack in backs)
				{
					if (workBackAdapter.BackId == removingBack.Id)
					{
						return false;
					}
				}
			}
			return true;
		}

		private IEnumerable<CountRegions> GetDbRegionsOfBacks(IEnumerable<Back> back)
		{
			foreach (CountRegions regions in db.CountRegions)
			{
				foreach (Back removingBack in back)
				{
					if (regions.BackId == removingBack.Id)
					{
						yield return regions;
						break;
					}
				}
			}
		}

		private IEnumerable<Back> GetDbBacksOfBacks(IEnumerable<Back> back)
		{
			foreach (Back dbBack in db.Backs)
			{
				foreach (Back removingBack in back)
				{
					if (dbBack.Id == removingBack.Id)
					{
						yield return dbBack;
						break;
					}
				}
			}
		}

		private void PushChildBacks(ref List<Back> backs)
		{
			backs = GetChildBacks(backs);
		}

		private List<Back> GetChildBacks(List<Back> backs)
		{
			List<Back> result = new List<Back>();
			if (backs != null)
			{
				foreach (Back back in backs)
				{
					result.AddRange(GetChildBacks(back.ChildBacks));
				}
				result.AddRange(backs);
			}
			return result;
		}
	}
}
