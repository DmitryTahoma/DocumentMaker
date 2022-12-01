using DocumentMaker.Security;
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
		CryptedConnectionString cryptedConnectionString = null;
		bool cryptedConnectionStringSetted = false;
		ProjectsDbContext db = null;

		~ProjectRestoreModel()
		{
			ReleaseContext();
		}

		public Task<bool> TryConnectDB()
		{
			return Task.Run(() =>
			{
				if(cryptedConnectionStringSetted) db = new ProjectsDbContext(cryptedConnectionString.GetDecryptedConnectionString());
				return cryptedConnectionStringSetted;
			});
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

					project.Backs = projectBacks;
				}
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

		public Task Restore(IDbObject dbObject)
		{
			return Task.Run(() =>
			{
				if (dbObject is Project)
				{
					db.Backs
						.Where(x => x.ProjectId == dbObject.Id)
						.ToList()
						.ForEach(x => x.DeletionDate = null);

					db.CountRegions
						.Where(x => db.Backs
							.FirstOrDefault(y => y.Id == x.BackId).ProjectId == dbObject.Id)
						.ToList()
						.ForEach(x => x.DeletionDate = null);

					db.SaveChanges();
				}
				else if (dbObject is CountRegions)
				{
					CountRegions dbRegions = db.CountRegions.First(x => x.Id == dbObject.Id);
					dbRegions.DeletionDate = null;
					db.SaveChanges();
				}
				else if (dbObject is Back)
				{
					Back dbBack = db.Backs.First(x => x.Id == dbObject.Id);
					dbBack.DeletionDate = null;
					db.SaveChanges();
				}
			});
		}

		public Task<IEnumerable<IDbObject>> RemoveForever(IDbObject dbObject)
		{
			return Task.Run(async () =>
			{
				if(dbObject is Project)
				{
					IEnumerable<IDbObject> result = null;

					Back removeBack = GetFirstOrDefaultRemoveBack(dbObject);
					while (removeBack != null)
					{
						IEnumerable<IDbObject> removedObjs = await RemoveForever(removeBack);
						result = result == null ? removedObjs : result.Concat(removedObjs);

						removeBack = GetFirstOrDefaultRemoveBack(dbObject);
					}

					CountRegions removeCountRegions = GetFirstOrDefaultRemoveCountRegions(dbObject);
					while(removeCountRegions != null)
					{
						IEnumerable<IDbObject> removedObjs = await RemoveForever(removeCountRegions);
						result = result == null ? removedObjs : result.Concat(removedObjs);

						removeCountRegions = GetFirstOrDefaultRemoveCountRegions(dbObject);
					}

					return result;
				}
				else if(dbObject is CountRegions)
				{
					CountRegions countRegions = db.CountRegions.FirstOrDefault(x => x.Id == dbObject.Id);
					db.CountRegions.Remove(countRegions);
					db.SaveChanges();
					return new CountRegions[] { countRegions };
				}
				else if(dbObject is Back)
				{
					Back dbBack = db.Backs.First(x => x.Id == dbObject.Id);
					Stack<Back> removingBacks = new Stack<Back>();
					removingBacks.Push(dbBack);
					PushToStackChilds(dbBack, removingBacks);
					List<CountRegions> removingCountRegions = GetDbRegionsOfBacks(removingBacks);

					db.CountRegions.RemoveRange(removingCountRegions);
					db.Backs.RemoveRange(removingBacks);
					db.SaveChanges();

					return removingCountRegions
						.Cast<IDbObject>()
						.Concat(removingBacks
							.Cast<IDbObject>());
				}

				return null;
			});
		}

		private void PushToStackChilds(Back root, Stack<Back> stackOfBacks)
		{
			foreach(Back back in db.Backs.Where(x => x.BaseBackId == root.Id).ToList())
			{
				stackOfBacks.Push(back);
				PushToStackChilds(back, stackOfBacks);
			}
		}

		private List<CountRegions> GetDbRegionsOfBacks(IEnumerable<Back> backs)
		{
			List<int> backIds = backs
				.Select(y => y.Id)
				.ToList();

			return db.CountRegions
				.Where(x => x.BackId.HasValue && backIds
					.Contains(x.BackId.Value))
				.ToList();
		}

		private Back GetFirstOrDefaultRemoveBack(IDbObject project)
		{
			return db.Backs
				.FirstOrDefault(x => x.ProjectId == project.Id && x.DeletionDate.HasValue);
		}

		private CountRegions GetFirstOrDefaultRemoveCountRegions(IDbObject project)
		{
			List<int> backs = db.Backs
				.Where(y => y.ProjectId == project.Id)
				.Select(x => x.Id)
				.ToList();

			return db.CountRegions
				.FirstOrDefault(x => x.DeletionDate.HasValue && x.BackId.HasValue && backs
					.Contains(x.BackId.Value));
		}

		public void SetConnectionString(CryptedConnectionString cryptedConnectionString)
		{
			this.cryptedConnectionString = cryptedConnectionString;
			cryptedConnectionStringSetted = true;
		}
	}
}
