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
					List<Back> projectBacks =
						db.Backs
							.Where(x => x.DeletionDate != null && x.ProjectId == project.Id)
							.ToList();
					foreach(Back back in projectBacks)
					{
						back.ChildBacks = null;
						back.BaseBack = null;
						LoadBack(back);
					}

					projectBacks.Sort((x, y) => -DateTime.Compare(x.DeletionDate.Value, y.DeletionDate.Value));
					project.Backs = projectBacks;
				}

				projects.Sort((x, y) => -DateTime.Compare(x.Backs.First().DeletionDate.Value, y.Backs.First().DeletionDate.Value));

				return projects;
			});
		}

		private void LoadBack(Back back)
		{
			foreach(Back childBack in db.Backs.Where(x => x.BaseBackId == back.Id))
			{
				back.ChildBacks = new List<Back>();
				Back loadedChildBack = new Back();
				loadedChildBack.Set(childBack);
				back.ChildBacks.Add(loadedChildBack);
			}
			back.BackType = db.BackTypes.FirstOrDefault(x => x.Id == back.BackTypeId);
			back.Regions = db.CountRegions.Where(x => x.BackId == back.Id).ToList();
			if (back.ChildBacks != null)
			{
				foreach (Back childBack in back.ChildBacks)
				{
					LoadBack(childBack);
				}
			}
			if(back.BaseBackId.HasValue)
			{
				Back loadedParrentBack = new Back();
				loadedParrentBack.Set(db.Backs.First(x => x.Id == back.BaseBackId));
				back.BaseBack = loadedParrentBack;
				LoadBack(loadedParrentBack);
			}
		}
	}
}
