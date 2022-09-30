using Db.Context;
using Db.Context.HumanPart;
using System.Collections.Generic;

namespace HumanEditorLib.Model
{
	public class HumanEditModel
	{
		public HumanEditModel()
		{

		}

		public IEnumerable<Human> LoadHumans()
		{
			List<Human> result = new List<Human>();
			using(DocumentMakerContext db = new DocumentMakerContext())
			{
				result.AddRange(db.Humans);
			}
			return result;
		}
	}
}
