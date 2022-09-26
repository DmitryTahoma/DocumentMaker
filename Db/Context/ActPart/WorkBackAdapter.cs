using Db.Context.BackPart;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Db.Context.ActPart
{
	public class WorkBackAdapter
	{
		public int Id { get; set; }
		public int? BackId { get; set; }
		public Back Back { get; set; }
		public string Text { get; set; }

		[InverseProperty("WorkBackAdapter")]
		public List<Work> Works { get; set; }
	}
}
