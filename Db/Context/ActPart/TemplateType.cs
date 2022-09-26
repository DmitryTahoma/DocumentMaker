using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Db.Context.ActPart
{
	public class TemplateType
	{
		public int Id { get; set; }
		public string Name { get; set; }

		[InverseProperty("TemplateType")]
		public List<Act> Acts { get; set; }
		[InverseProperty("TemplateType")]
		public List<WorkType> WorkTypes { get; set; }
	}
}
