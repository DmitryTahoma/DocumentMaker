using Dml;
using ProjectsDb;
using ProjectsDb.Context;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ActGenerator.Model.Dialogs
{
	class AddProjectNameDialogModel : ProjectsDbConnector
	{
		NaturalStringComparer naturalStringComparer = new NaturalStringComparer();

		public Task<List<Project>> LoadProjectsAsync()
		{
			return Task.Run(() =>
			{
				List<Project> projects = new List<Project>(db.Projects);
				foreach (Project project in projects)
				{
					project.AlternativeNames = db.AlternativeProjectNames.Where(x => x.ProjectId == project.Id).ToList();
				}
				return projects;
			});
		}

		public Task SortProjectsAsync(List<Project> projects)
		{
			return Task.Run(() =>
			{
				projects.Sort(naturalStringComparer);

				foreach(Project project in projects)
				{
					project.AlternativeNames.Sort(naturalStringComparer);
				}
			});
		}
	}
}
