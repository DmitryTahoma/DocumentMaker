using Db.Context;
using Db.Context.HumanPart;
using System.Collections.Generic;
using System.Linq;

namespace HumanEditorLib.Model
{
	public class BankEditModel
	{
		public BankEditModel()
		{

		}

		public IEnumerable<Bank> LoadBanks()
		{
			List<Bank> result;
			using (DocumentMakerContext db = new DocumentMakerContext())
			{
				result = new List<Bank>(db.Banks);
			}
			return result;
		}

		public bool DeleteBank(Bank bank)
		{
			bool removed = false;
			using (DocumentMakerContext db = new DocumentMakerContext())
			{
				if(db.Humans.FirstOrDefault(x =>x.BankId == bank.Id) == null)
				{
					db.Banks.Remove(db.Banks.FirstOrDefault(x => x.Id == bank.Id));
					db.SaveChanges();
					removed = true;
				}
			}
			return removed;
		}

		public Bank AddBank()
		{
			Bank bank = new Bank();
			using(DocumentMakerContext db  =new DocumentMakerContext())
			{
				bank = db.Banks.Add(bank);
				db.SaveChanges();
			}
			return bank;
		}

		public void SaveChanges(IEnumerable<Bank> banks)
		{
			using(DocumentMakerContext db = new DocumentMakerContext())
			{
				IEnumerator<Bank> local = banks.GetEnumerator();

				foreach(Bank bank in db.Banks)
				{
					if (!local.MoveNext()) break;
					bank.Set(local.Current);
				}

				db.SaveChanges();
			}
		}
	}
}
