using DocumentMaker.Security;
using ProjectsDb.Context;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectsDb
{
	public class ProjectsDbConnector : IProjectsDbConnector
	{
		protected ProjectsDbContext db = null;
		private CryptedConnectionString cryptedConnectionString = null;

		~ProjectsDbConnector()
		{
			ReleaseContext();
		}

		public CryptedConnectionString ConnectionString
		{
			get => cryptedConnectionString;
			set
			{
				cryptedConnectionString = value;
				ConnectionStringSetted = true;
			}
		}
		public bool ConnectionStringSetted { get; private set; }

		private void ReleaseContext()
		{
			if (db != null)
			{
				db.Dispose();
				db = null;
			}
		}

		public void ConnectDb()
		{
			db = new ProjectsDbContext(cryptedConnectionString.GetDecryptedConnectionString());
		}

		public Task ConnectDbAsync()
		{
			return Task.Run(ConnectDb);
		}

		public void DisconnectDb()
		{
			ReleaseContext();
		}

		public Task DisconnectDbAsync()
		{
			return Task.Run(DisconnectDb);
		}

		public IEnumerable<T> GetTable<T>() where T : class, IDbObject
		{
			return db.GetTable<T>();
		}

		public Task<IEnumerable<T>> GetTableAsync<T>() where T : class, IDbObject
		{
			return db.GetTableAsync<T>();
		}

		public void SyncCollection<T>(ICollection<T> collection) where T : class, IDbObject
		{
			db.SyncCollection(collection);
		}

		public Task SyncCollectionAsync<T>(ICollection<T> collection) where T : class, IDbObject
		{
			return db.SyncCollectionAsync(collection);
		}
	}
}
