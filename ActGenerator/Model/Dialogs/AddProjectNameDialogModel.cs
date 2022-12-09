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
			return Task.Run(db.Projects.ToList);
		}

		public Task<List<AlternativeProjectName>> LoadAlternativeProjectNamesAsync()
		{
			return Task.Run(db.AlternativeProjectNames.ToList);
		}

		public Task SortCollectionAsync<T>(List<T> objects) where T : class
		{
			return Task.Run(() => objects.Sort(naturalStringComparer));
		}
	}
}
