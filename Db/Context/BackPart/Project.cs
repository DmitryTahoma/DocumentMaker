using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Db.Context.BackPart
{
	public class Project : IDbObject
	{
		public int Id { get; set; }
		public string Name { get; set; }

		[InverseProperty("Project")]
		public List<AlternativeProjectName> AlternativeNames { get; set; }
		[InverseProperty("Project")]
		public List<Episode> Episodes { get; set; }

		public void Set(IDbObject other)
		{
			throw new System.NotImplementedException();
		}
	}
}
