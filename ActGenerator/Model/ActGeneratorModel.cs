using Db.Context;
using Db.Context.BackPart;
using Db.Context.HumanPart;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ActGenerator.Model
{
	class ActGeneratorModel
	{
		DocumentMakerContext db = null;

		~ActGeneratorModel()
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

		public async Task<List<Project>> LoadProjects()
		{
			return await Task.Run(() => new List<Project>(db.Projects));
		}

		public async Task<List<Human>> LoadHumen()
		{
			return await Task.Run(() => new List<Human>(db.Humans));
		}

		private void ReleaseContext()
		{
			if (db != null)
			{
				db.Dispose();
				db = null;
			}
		}
	}
}
