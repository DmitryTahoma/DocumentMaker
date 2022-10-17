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

		public Task<Project> LoadProject(Project project)
		{
			return Task.Run(() =>
			{
				Project dbProject = db.Projects.First(x => x.Id == project.Id);
				project.Episodes = new List<Episode>(db.Episodes.Where(x => x.ProjectId == project.Id));
				return project;
			});
		}
	}
}
