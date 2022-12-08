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
		bool cryptedConnectionStringSetted = false;

		public Task<List<Project>> LoadProjects()
		{
			return Task.Run(() =>
			{
				if (!cryptedConnectionStringSetted) return null;

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
				if (!cryptedConnectionStringSetted) return;

				await db.SyncCollectionAsync(collection);
			}
		}

		public void SetConnectionString(CryptedConnectionString cryptedConnectionString)
		{
			this.cryptedConnectionString = cryptedConnectionString;
			cryptedConnectionStringSetted = true;
		}
	}
}
