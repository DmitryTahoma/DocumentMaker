using DocumentMaker.Security;
using ProjectsDb;
using ProjectsDb.Context;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectEditorLib.Model.Dialogs
{
	public class SelectProjectDialogModel
	{
		CryptedConnectionString cryptedConnectionString = null;

		public Task<List<Project>> LoadProjects()
		{
			return Task.Run(() =>
			{
				List<Project> result;
				using (ProjectsDbContext db = new ProjectsDbContext(cryptedConnectionString.GetDecryptedConnectionString()))
				{
					result = new List<Project>(db.Projects);
				}
				return result;
			});
		}

		public async Task SyncCollection<T>(ICollection<T> collection) where T : class, IDbObject
		{
			using(ProjectsDbContext db = new ProjectsDbContext(cryptedConnectionString.GetDecryptedConnectionString()))
			{
				await db.SyncCollection(collection);
			}
		}

		public void SetConnectionString(CryptedConnectionString cryptedConnectionString)
		{
			this.cryptedConnectionString = cryptedConnectionString;
		}
	}
}
