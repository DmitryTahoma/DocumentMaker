using ProjectsDb;
using ProjectsDb.Context;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectEditorLib.Model.Dialogs
{
	public class SelectProjectDialogModel : ProjectsDbConnector
	{
		public Task<List<Project>> LoadProjectsAsync()
		{
			return Task.Run(() => new List<Project>(db.Projects) );
		}
	}
}
