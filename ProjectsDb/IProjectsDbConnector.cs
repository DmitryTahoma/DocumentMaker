using DocumentMaker.Security;
using ProjectsDb.Context;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectsDb
{
	public interface IProjectsDbConnector
	{
		CryptedConnectionString ConnectionString { get; set; }
		bool ConnectionStringSetted { get; }

		void ConnectDb();
		Task ConnectDbAsync();
		void DisconnectDb();
		Task DisconnectDbAsync();
		IEnumerable<T> GetTable<T>() where T : class, IDbObject;
		Task<IEnumerable<T>> GetTableAsync<T>() where T : class, IDbObject;
		void SyncCollection<T>(ICollection<T> collection) where T : class, IDbObject;
		Task SyncCollectionAsync<T>(ICollection<T> collection) where T : class, IDbObject;
	}
}
