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
			return Task.Run(() =>
			{
				if (projectNode.Type == ProjectNodeType.Episode)
				{
					Episode episode = db.Episodes.FirstOrDefault(x => x.Id == projectNode.Context.Id);
					if (episode == null)
					{
						episode = db.Episodes.Add((Episode)projectNode.Context);
					}
					else
					{
						episode.Set(projectNode.Context);
					}
					db.SaveChanges();
				}
			});
		}
	}
}
