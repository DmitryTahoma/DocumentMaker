using DocumentMaker.Security;
using ProjectsDb;
using ProjectsDb.Context;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ActGenerator.Model
{
	class ActGeneratorModel
	{
		CryptedConnectionString cryptedConnectionString = null;
		bool cryptedConnectionStringSetted = false;
		ProjectsDbContext db = null;

		~ActGeneratorModel()
		{
			ReleaseContext();
		}

		public Task<bool> TryConnectDB()
		{
			return Task.Run(() =>
			{
				if (cryptedConnectionStringSetted) db = new ProjectsDbContext(cryptedConnectionString.GetDecryptedConnectionString());
				return cryptedConnectionStringSetted;
			});
		}

		public async Task DisconnectDB()
		{
			await Task.Run(ReleaseContext);
		}

		public async Task<List<Project>> LoadProjects()
		{
			return await Task.Run(() => 
			{
				List<Project> projects = new List<Project>(db.Projects);
				foreach(Project project in projects)
				{
					project.AlternativeNames = db.AlternativeProjectNames.Where(x => x.ProjectId == project.Id).ToList();
				}
				return projects;
			});
		}

		public async Task SyncCollection<T>(ICollection<T> collection) where T : class, IDbObject
		{
			await db.SyncCollection(collection);
		}

		private void ReleaseContext()
		{
			if (db != null)
			{
				db.Dispose();
				db = null;
			}
		}

		public void SetConnectionString(CryptedConnectionString cryptedConnectionString)
		{
			this.cryptedConnectionString = cryptedConnectionString;
			cryptedConnectionStringSetted = true;
		}
	}
}
