using Db.Context;
using Db.Context.BackPart;
using System.Collections.Generic;
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

		private void ReleaseContext()
		{
			if(db != null)
			{
				db.Dispose();
				db = null;
			}
		}
	}
}
