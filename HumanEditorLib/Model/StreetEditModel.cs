using Db.Context;
using Db.Context.HumanPart;
using System.Collections.Generic;

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
	}
}
