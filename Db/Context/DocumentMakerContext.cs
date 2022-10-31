using Db.Context.ActPart;
using Db.Context.BackPart;
using Db.Context.HumanPart;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Db.Context
{
	public class DocumentMakerContext : DbContext
	{
		static List<PropertyInfo> tables = new List<PropertyInfo>();

		static DocumentMakerContext()
		{
			tables = new List<PropertyInfo>(typeof(DocumentMakerContext).GetProperties()
				.Where(x => x.PropertyType.Name == typeof(DbSet<>).Name));
		}

		public DocumentMakerContext() : base("Data Source=10.32.16.170,1433;Network Library=DBMSSOCN;Initial Catalog=DocumentMaker;User ID=ProgTest; Password=qwerty123;") { }

		#region ActPart

		public DbSet<Act> Acts { get; set; }
		public DbSet<ActPart.ActPart> ActParts { get; set; }
		public DbSet<FullAct> FullActs { get; set; }
		public DbSet<FullWork> FullWorks { get; set; }
		public DbSet<Regions> Regions { get; set; }
		public DbSet<TemplateType> TemplateTypes { get; set; }
		public DbSet<Work> Works { get; set; }
		public DbSet<WorkBackAdapter> WorkBackAdapters { get; set; }
		public DbSet<WorkType> WorkTypes { get; set; }

		#endregion

		#region BackPart

		public DbSet<AlternativeProjectName> AlternativeProjectNames { get; set; }
		public DbSet<Back> Backs { get; set; }
		public DbSet<BackType> BackTypes { get; set; }
		public DbSet<CountRegions> CountRegions { get; set; }
		public DbSet<Episode> Episodes { get; set; }
		public DbSet<Project> Projects { get; set; }

		#endregion

		#region HumanPart

		public DbSet<Address> Addresses { get; set; }
		public DbSet<Bank> Banks { get; set; }
		public DbSet<Contract> Contracts { get; set; }
		public DbSet<Human> Humans { get; set; }
		public DbSet<LocalityType> LocalityTypes { get; set; }
		public DbSet<StreetType> StreetTypes { get; set; }

		#endregion

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
