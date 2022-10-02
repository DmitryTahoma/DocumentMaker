using Db.Context;
using Db.Context.HumanPart;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HumanEditorLib.Model
{
	public class LocalityEditModel
	{
		public LocalityEditModel()
		{

		}

		public async Task<IEnumerable<LocalityType>> LoadLocalities()
		{
			return await Task.Run(() =>
			{
				List<LocalityType> result;
				using (DocumentMakerContext db = new DocumentMakerContext())
				{
					result = new List<LocalityType>(db.LocalityTypes);
				}
				return result;
			});
		}

		public bool DeleteLocalityType(LocalityType localityType)
		{
			bool removed = false;
			using (DocumentMakerContext db = new DocumentMakerContext())
			{
				if (db.Addresses.Where(x => x.LocalityTypeId == localityType.Id).FirstOrDefault() == null)
				{
					db.LocalityTypes.Remove(db.LocalityTypes.FirstOrDefault(x => x.Id == localityType.Id));
					db.SaveChanges();
					removed = true;
				}
			}
			return removed;
		}

		public LocalityType AddLocalityType()
		{
			LocalityType localityType = new LocalityType();
			using (DocumentMakerContext db = new DocumentMakerContext())
			{
				localityType = db.LocalityTypes.Add(localityType);
				db.SaveChanges();
			}
			return localityType;
		}

		public void SaveChanges(IEnumerable<LocalityType> localityTypes)
		{
			using (DocumentMakerContext db = new DocumentMakerContext())
			{
				IEnumerator<LocalityType> local = localityTypes.GetEnumerator();

				foreach(LocalityType localityType in db.LocalityTypes)
				{
					if (!local.MoveNext()) break;
					localityType.Set(local.Current);
				}

				db.SaveChanges();
			}
		}
	}
}
