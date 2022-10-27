using Db.Context;
using System.Linq;

namespace ProjectEditorLib.Model
{
	public class ProjectModel
	{
		public bool CanRemoveAltProjectName(int altProjectNameId)
		{
			if (altProjectNameId <= 0) return true;

			using(DocumentMakerContext db = new DocumentMakerContext())
			{
				return db.WorkBackAdapters.FirstOrDefault(x => x.AlternativeProjectNameId == altProjectNameId) == null;
			}
		}
	}
}
