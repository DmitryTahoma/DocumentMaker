using Db.Context;
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
						await SaveBackChanges((Back)projectNode.Context, projectNode.Type); break;
				}
			});
		}

		private Task SaveProjectChanges(Project project)
		{
			return Task.Run(() =>
			{
				Project dbProject = db.Projects.First(x => x.Id == project.Id);
				dbProject.Set(project);
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
						CountRegions regions = db.CountRegions.FirstOrDefault(x => x.Id == dbBack.Id);
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

		public Task<Project> LoadProject(Project project)
		{
			return Task.Run(async () =>
			{
				Project dbProject = db.Projects.First(x => x.Id == project.Id);
				project.Episodes = new List<Episode>(db.Episodes.Where(x => x.ProjectId == project.Id));
				foreach(Episode episode in project.Episodes)
				{
					episode.Backs = new List<Back>(db.Backs.Where(x => x.EpisodeId == episode.Id));
					foreach(Back back in episode.Backs)
					{
						await LoadBack(back);
					}
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
	}
}
