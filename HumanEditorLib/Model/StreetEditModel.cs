using Db.Context;
using Db.Context.HumanPart;
using System.Collections.Generic;
using System.Linq;

namespace HumanEditorLib.Model
{
	public class StreetEditModel
	{
		public StreetEditModel()
		{
		}

		public IEnumerable<StreetType> LoadStreets()
		{
			List<StreetType> result;
			using (DocumentMakerContext db = new DocumentMakerContext())
			{
				result = new List<StreetType>(db.StreetTypes);
			}
			return result;
		}

		public bool DeleteStreetType(StreetType streetType)
		{
			bool removed = false;
			using (DocumentMakerContext db = new DocumentMakerContext())
			{
				if(db.Addresses.Where(x => x.StreetTypeId == streetType.Id).FirstOrDefault() == null)
				{
					db.StreetTypes.Remove(db.StreetTypes.FirstOrDefault(x => x.Id == streetType.Id));
					db.SaveChanges();
					removed = true;
				}
			}
			return removed;
		}
	}
}
