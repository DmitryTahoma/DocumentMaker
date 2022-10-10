using Db.Context.BackPart;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Db.Context.ActPart
{
	public class WorkBackAdapter : IDbObject
	{
		public int Id { get; set; }
		public int? BackId { get; set; }
		public Back Back { get; set; }
		public string Text { get; set; }
		public int? AlternativeProjectNameId { get; set; }
		public AlternativeProjectName AlternativeProjectName { get; set; }

		[InverseProperty("WorkBackAdapter")]
		public List<Work> Works { get; set; }

		public void Set(IDbObject other)
		{
			throw new System.NotImplementedException();
		}
	}
}
