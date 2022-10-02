using Db.Context;
using Db.Context.HumanPart;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HumanEditorLib.Model
{
	public class HumanEditModel
	{
		public HumanEditModel()
		{

		}

		public async Task<IEnumerable<Human>> LoadHumans()
		{
			return await Task.Run(() =>
			{
				List<Human> result = new List<Human>();
				using (DocumentMakerContext db = new DocumentMakerContext())
				{
					result.AddRange(db.Humans);
				}
				return result;
			});
		}
	}
}
