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

		public ProjectsDbContext() : base("Data Source=10.32.16.170,1433;Network Library=DBMSSOCN;Initial Catalog=FbnProjectsDb;User ID=ProgTest; Password=qwerty123;") { }

		public DbSet<AlternativeProjectName> AlternativeProjectNames { get; set; }
		public DbSet<Back> Backs { get; set; }
		public DbSet<BackType> BackTypes { get; set; }
		public DbSet<CountRegions> CountRegions { get; set; }
		public DbSet<Project> Projects { get; set; }

		public async Task<IEnumerable<T>> GetTable<T>() where T : class, IDbObject
		{
			return await Task.Run(new Func<IEnumerable<T>>(() =>
			{
				return tables
					.Where(x => x.PropertyType.GenericTypeArguments
						.FirstOrDefault(y => y == typeof(T)) != null)
					.Select(x =>(IEnumerable<T>)x.GetValue(this))
					.FirstOrDefault();
			}));
		}

		public Task SyncCollection<T>(ICollection<T> collection) where T : class, IDbObject
		{
			return Task.Run(async () =>
			{
				List<T> dbCollection = new List<T>(await GetTable<T>());

				List<T> removeList = new List<T>();
				foreach(T elem in collection)
				{
					if(dbCollection.FirstOrDefault(x => x.Id == elem.Id) == null)
					{
						removeList.Add(elem);
					}
				}
				while(removeList.Count > 0)
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
			});
		}
	}
}
