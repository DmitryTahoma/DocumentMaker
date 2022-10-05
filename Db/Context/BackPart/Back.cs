using Db.Context.ActPart;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Db.Context.BackPart
{
	public class Back : IDbObject
	{
		public int Id { get; set; }
		public int? EpisodeId { get; set; }
		public Episode Episode { get; set; }
		public int? BackTypeId { get; set; }
		public BackType BackType { get; set; }
		public string Name { get; set; }
		public int? BaseBackId { get; set; }
		public Back BaseBack { get; set; }

		[InverseProperty("Back")]
		public List<Minigame> Minigames { get; set; }
		[InverseProperty("Back")]
		public List<CountRegions> Regions { get; set; }
		[InverseProperty("BaseBack")]
		public List<Back> ChildBacks { get; set; }
		[InverseProperty("Back")]
		public List<WorkBackAdapter> WorkBackAdapters { get; set; }

		public void Set(IDbObject other)
		{
			throw new System.NotImplementedException();
		}
	}
}
