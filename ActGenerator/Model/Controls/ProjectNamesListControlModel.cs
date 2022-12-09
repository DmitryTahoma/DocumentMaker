using ProjectsDb;
using ProjectsDb.Context;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ActGenerator.Model.Controls
{
	class ProjectNamesListControlModel : ProjectsDbConnector
	{
		public Task<List<Project>> LoadProjectsAsync()
		{
			return Task.Run(db.Projects.ToList);
		}

		public Task<List<AlternativeProjectName>> LoadAlternativeProjectNamesAsync()
		{
			return Task.Run(db.AlternativeProjectNames.ToList);
		}
	}
}
