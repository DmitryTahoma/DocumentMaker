using Db.Context;
using Db.Context.HumanPart;
using Mvvm;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace HumanEditorLib.Model
{
	public class HumanEditModel
	{
		DocumentMakerContext db = null;

		public HumanEditModel()
		{

		}

		~HumanEditModel()
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

		public async Task<IEnumerable<Human>> LoadHumans()
		{
			return await Task.Run(() =>
			{
				return new List<Human>(db.Humans);
			});
		}

		public async Task<IEnumerable<LocalityType>> LoadLocalities()
		{
			return await Task.Run(() =>
			{
				return new List<LocalityType>(db.LocalityTypes);
			});
		}

		public async Task<IEnumerable<StreetType>> LoadStreets()
		{
			return await Task.Run(() =>
			{
				return new List<StreetType>(db.StreetTypes);
			});
		}

		public async Task<IEnumerable<Bank>> LoadBanks()
		{
			return await Task.Run(() =>
			{
				return new List<Bank>(db.Banks);
			});
		}

		public async Task UpdateLocalities(ObservableRangeCollection<LocalityType> localities)
		{
			await Task.Run(async () =>
			{
				IEnumerable<LocalityType> dbLocalities = await LoadLocalities();
				localities.SuppressingNotifications = true;

				localities.RemoveAll(x => dbLocalities.FirstOrDefault(y => y.Id == x.Id) == null);

				IEnumerator<LocalityType> localitiesEnum = localities.GetEnumerator();
				IEnumerator<LocalityType> dbLocalitiesEnum = dbLocalities.GetEnumerator();

				while(localitiesEnum.MoveNext() && dbLocalitiesEnum.MoveNext())
				{
					localitiesEnum.Current.Set(dbLocalitiesEnum.Current);
				}

				localities.AddRange(dbLocalities.Where(x => localities.FirstOrDefault(y => y.Id == x.Id) == null));

				localities.SuppressingNotifications = false;
			});
		}

		public async Task SyncCollection<T>(ObservableRangeCollection<T> collection) where T : class, IDbObject
		{
			await Task.Run(async () =>
			{
				List<T> dbCollection = new List<T>(await db.GetTable<T>());
				collection.SuppressingNotifications = true;

				collection.RemoveAll(x => dbCollection.FirstOrDefault(y => y.Id == x.Id) == null);

				IEnumerator<T> collectionEnum = collection.GetEnumerator();
				IEnumerator<T> dbCollectionEnum = dbCollection.GetEnumerator();

				while(collectionEnum.MoveNext() && dbCollectionEnum.MoveNext())
				{
					collectionEnum.Current.Set(dbCollectionEnum.Current);
				}

				collection.AddRange(dbCollection.Where(x => collection.FirstOrDefault(y => y.Id == x.Id) == null));

				collection.SuppressingNotifications = false;
			});
		}

		public async Task<Human> AddHuman(Human human)
		{
			return await Task.Run(() =>
			{
				human = db.Humans.Add(human);
				db.SaveChanges();
				return human;
			});
		}

		private void ReleaseContext()
		{
			if (db != null)
			{
				db.Dispose();
				db = null;
			}
		}

		public async Task LoadHuman(Human human)
		{
			await Task.Run(() =>
			{
				if (human.Address == null && human.AddressId != null)
					human.Address = db.Addresses.FirstOrDefault(x => x.Id == human.AddressId);

				if (human.DevelopmentContract == null && human.DevelopmentContractId != null)
					human.DevelopmentContract = db.Contracts.FirstOrDefault(x => x.Id == human.DevelopmentContractId);

				if (human.SupportContract == null && human.SupportContractId != null)
					human.SupportContract = db.Contracts.FirstOrDefault(x => x.Id == human.SupportContractId);

				if (human.Bank == null && human.BankId != null)
					human.Bank = db.Banks.FirstOrDefault(x => x.Id == human.BankId);
			});
		}

		public async Task<Human> SaveHumanChanges(Human human)
		{
			return await Task.Run(() => 
			{
				Human dbHuman = db.Humans.FirstOrDefault(x => x.Id == human.Id);
				dbHuman.Set(human);
				Bank dbBank = db.Banks.FirstOrDefault(x => x.Id == human.BankId);
				if (dbBank != null)
				{
					dbHuman.Bank = dbBank;
				}
				dbHuman.Address = db.Addresses.Where(x => x.Id == human.Address.Id).FirstOrDefault();
				if(dbHuman.Address == null)
				{
					dbHuman.Address = human.Address;
				}
				else
				{
					dbHuman.Address.Set(human.Address);
				}

				if(human.DevelopmentContract != null)
				{
					dbHuman.DevelopmentContract = db.Contracts.FirstOrDefault(x => x.Id == human.DevelopmentContract.Id);

					if (dbHuman.DevelopmentContract == null)
					{
						dbHuman.DevelopmentContract = human.DevelopmentContract;
					}
					else
					{
						dbHuman.DevelopmentContract.Set(human.DevelopmentContract);
					}
				}

				if (human.SupportContract != null)
				{
					dbHuman.SupportContract = db.Contracts.FirstOrDefault(x => x.Id == human.SupportContract.Id);
					if (dbHuman.SupportContract == null)
					{
						dbHuman.SupportContract = human.SupportContract;
					}
					else
					{
						dbHuman.SupportContract.Set(human.SupportContract);
					}
				}

				db.SaveChanges();
				return dbHuman;
			});
		}
	}
}
