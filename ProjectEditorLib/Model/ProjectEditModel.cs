using ProjectsDb;
using ProjectsDb.Context;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectEditorLib.Model
{
	public class ProjectEditModel : ProjectsDbConnector
	{
		const double nodesDaysLifetime = 45;

		public Task<Project> CreateProjectAsync(Project project)
		{
			return Task.Run(() =>
			{
				project = db.Projects.Add(project);
				db.SaveChanges();
				return project;
			});
		}

		public Task SaveNodeChangesAsync(ProjectNode projectNode)
		{
			return Task.Run(() =>
			{
				SaveNodeChanges(projectNode);
			});
		}

		public void SaveNodeChanges(ProjectNode projectNode)
		{
			switch (projectNode.Type)
			{
				case ProjectNodeType.Project: SaveProjectChanges((Project)projectNode.Context); break;
				case ProjectNodeType.Episode:
				case ProjectNodeType.Back:
				case ProjectNodeType.Craft:
				case ProjectNodeType.Minigame:
				case ProjectNodeType.Dialog:
				case ProjectNodeType.Hog:
					SaveBackChanges((Back)projectNode.Context, projectNode.Type); break;
				case ProjectNodeType.Regions: SaveCountRegionsChanges((CountRegions)projectNode.Context); break;
			}
		}

		private void SaveProjectChanges(Project project)
		{
			Project dbProject = db.Projects.First(x => x.Id == project.Id);
			dbProject.Set(project);

			List<AlternativeProjectName> dbAltNames = db.AlternativeProjectNames.Where(x => x.ProjectId == project.Id).ToList();
			List<AlternativeProjectName> altNamesToRemove = dbAltNames.ToList();
			foreach (AlternativeProjectName projName in dbAltNames)
			{
				if (project.AlternativeNames.FirstOrDefault(x => x.Id == projName.Id) != null)
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
		}

		private void SaveBackChanges(Back back, ProjectNodeType projectNodeType)
		{
			BackType backType = db.BackTypes.FirstOrDefault(x => x.Name == projectNodeType.ToString());
			bool isNewBackType = backType == null;
			if (isNewBackType)
			{
				backType = db.BackTypes.Add(new BackType { Name = projectNodeType.ToString() });
			}

			Back dbBack = db.Backs.FirstOrDefault(x => x.Id == back.Id);
			if (dbBack == null)
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
		}

		private void SaveCountRegionsChanges(CountRegions countRegions)
		{
			CountRegions dbCountRegions = db.CountRegions.FirstOrDefault(x => x.Id == countRegions.Id);
			if (dbCountRegions == null)
			{
				dbCountRegions = db.CountRegions.Add(countRegions);
			}
			else
			{
				dbCountRegions.Set(countRegions);
			}
			db.SaveChanges();
		}

		public Task<Project> LoadProjectAsync(Project project)
		{
			return Task.Run(() =>
			{
				Project dbProject = db.Projects.First(x => x.Id == project.Id);
				project.Backs = new List<Back>(db.Backs.Where(x => x.BaseBackId == null && x.ProjectId == project.Id && x.DeletionDate == null));
				project.AlternativeNames = new List<AlternativeProjectName>(db.AlternativeProjectNames.Where(x => x.ProjectId == project.Id));
				project.Backs.ForEach(LoadBack);
				return project;
			});
		}

		private void LoadBack(Back back)
		{
			back.BackType = db.BackTypes.FirstOrDefault(x => x.Id == back.BackTypeId);
			back.Regions = new List<CountRegions>(db.CountRegions.Where(x => x.DeletionDate == null && x.BackId == back.Id));
			back.ChildBacks = new List<Back>(db.Backs.Where(x => x.DeletionDate == null && x.BaseBackId == back.Id));

			foreach (Back childBack in back.ChildBacks)
			{
				LoadBack(childBack);
			}
		}

		public Task<bool> RemoveNodeAsync(IDbObject node)
		{
			return Task.Run(() =>
			{
				RemoveOldBacks();

				Back removingBack = null;

				if (node is Back back)
				{
					removingBack = back;
				}

				if (removingBack == null)
				{
					if (node is CountRegions regions)
					{
						CountRegions dbRegions = db.CountRegions
							.FirstOrDefault(x => x.Id == regions.Id);

						dbRegions.DeletionDate = DateTime.Now;
						db.SaveChanges();
						return true;
					}
				}
				else
				{
					Back dbBack = db.Backs
						.FirstOrDefault(x => x.Id == removingBack.Id);

					dbBack.DeletionDate = DateTime.Now;
					db.SaveChanges();

					return true;
				}
				return false;
			});
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

		private void RemoveOldBacks()
		{
			List<CountRegions> removingRegions = db.CountRegions
				.Where(x => x.DeletionDate.HasValue && DbFunctions.DiffDays(DateTime.Now, x.DeletionDate.Value).Value > nodesDaysLifetime)
				.ToList();
			List<Back> removingBacks = db.Backs
				.Where(x => x.DeletionDate.HasValue && DbFunctions.DiffDays(DateTime.Now, x.DeletionDate.Value).Value > nodesDaysLifetime)
				.ToList();
			removingBacks.ForEach(LoadBack);

			PushChildBacks(ref removingBacks);
			removingRegions.AddRange(GetDbRegionsOfBacks(removingBacks));

			db.Backs.RemoveRange(removingBacks);
			db.CountRegions.RemoveRange(removingRegions);
		}
	}
}
