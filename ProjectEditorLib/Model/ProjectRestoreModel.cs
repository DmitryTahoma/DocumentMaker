using ProjectsDb;
using ProjectsDb.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectEditorLib.Model
{
	public class ProjectRestoreModel
	{
		ProjectsDbContext db = null;

		~ProjectRestoreModel()
		{
			ReleaseContext();
		}

		public async Task ConnectDB()
		{
			await Task.Run(() => { db = new ProjectsDbContext(); });
		}

		public async Task DisconnectDB()
		{
			await Task.Run(ReleaseContext);
		}

		private void ReleaseContext()
		{
			if (db != null)
			{
				db.Dispose();
				db = null;
			}
		}

		public Task<List<Project>> LoadRemovedNodes()
		{
			return Task.Run(() =>
			{
				List<Project> projects =
					db.Projects
						.Where(x =>
							db.Backs
								.FirstOrDefault(y => y.DeletionDate != null && y.ProjectId == x.Id) != null)
						.ToList();

				foreach (Project project in projects)
				{
					project.Backs =
						db.Backs
							.Where(x => x.DeletionDate != null && x.ProjectId == project.Id)
							.ToList();
					project.Backs
						.ForEach(x => x.BackType = db.BackTypes
							.FirstOrDefault(y => y.Id == x.BackTypeId));

					project.Backs.Sort((x, y) => -DateTime.Compare(x.DeletionDate.Value, y.DeletionDate.Value));
				}

				projects.Sort((x, y) => -DateTime.Compare(x.Backs.First().DeletionDate.Value, y.Backs.First().DeletionDate.Value));

				return projects;
			});
		}
	}
}
