using ProjectsDb.Context;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ProjectsDb
{
	public class ProjectsDbContext : DbContext
	{
		static List<PropertyInfo> tables = new List<PropertyInfo>();

		static ProjectsDbContext()
		{
			tables = new List<PropertyInfo>(typeof(ProjectsDbContext).GetProperties()
				.Where(x => x.PropertyType.Name == typeof(DbSet<>).Name));
		}

		public ProjectsDbContext(string nameOrConnectionString) : base(nameOrConnectionString) { }

		public DbSet<AlternativeProjectName> AlternativeProjectNames { get; set; }
		public DbSet<Back> Backs { get; set; }
		public DbSet<BackType> BackTypes { get; set; }
		public DbSet<CountRegions> CountRegions { get; set; }
		public DbSet<Project> Projects { get; set; }

		public IEnumerable<T> GetTable<T>() where T : class, IDbObject
		{
			return tables
				.Where(x => x.PropertyType.GenericTypeArguments
					.FirstOrDefault(y => y == typeof(T)) != null)
				.Select(x => (IEnumerable<T>)x.GetValue(this))
				.FirstOrDefault();
		}

		public Task<IEnumerable<T>> GetTableAsync<T>() where T : class, IDbObject
		{
			return Task.Run(GetTable<T>);
		}

		public void SyncCollection<T>(ICollection<T> collection) where T : class, IDbObject
		{
			List<T> dbCollection = new List<T>(GetTable<T>());

			List<T> removeList = new List<T>();
			foreach (T elem in collection)
			{
				if (dbCollection.FirstOrDefault(x => x.Id == elem.Id) == null)
				{
					removeList.Add(elem);
				}
			}
			while (removeList.Count > 0)
			{
				T first = removeList.First();
				collection.Remove(first);
				removeList.Remove(first);
			}

			IEnumerator<T> collectionEnum = collection.GetEnumerator();
			IEnumerator<T> dbCollectionEnum = dbCollection.GetEnumerator();

			while (collectionEnum.MoveNext() && dbCollectionEnum.MoveNext())
			{
				collectionEnum.Current.Set(dbCollectionEnum.Current);
			}

			foreach (T newElem in dbCollection.Where(x => collection.FirstOrDefault(y => y.Id == x.Id) == null))
			{
				collection.Add(newElem);
			}
		}

		public Task SyncCollectionAsync<T>(ICollection<T> collection) where T : class, IDbObject
		{
			return Task.Run(() => SyncCollection(collection));
		}
	}
}
