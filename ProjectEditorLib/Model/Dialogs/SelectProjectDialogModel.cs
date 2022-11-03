using Db.Context;
using Db.Context.BackPart;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectEditorLib.Model.Dialogs
{
	public class SelectProjectDialogModel
	{
		public Task<List<Project>> LoadProjects()
		{
			return Task.Run(() =>
			{
				List<Project> result;
				using (DocumentMakerContext db = new DocumentMakerContext())
				{
					result = new List<Project>(db.Projects);
				}
				return result;
			});
		}

		public async Task SyncCollection<T>(ICollection<T> collection) where T : class, IDbObject
		{
			using(DocumentMakerContext db = new DocumentMakerContext())
			{
				await db.SyncCollection(collection);
			}
		}
	}
}
