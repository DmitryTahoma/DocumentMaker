using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Db.Context.BackPart
{
	public class Episode
	{
		public int Id { get; set; }
		public int? ProjectId { get; set; }
		public Project Project { get; set; }
		public string Name { get; set; }
		public int Number { get; set; }

		[InverseProperty("Episode")]
		public List<Back> Backs { get; set; }
	}
}
