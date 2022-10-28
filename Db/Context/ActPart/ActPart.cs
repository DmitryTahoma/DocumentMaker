using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Db.Context.ActPart
{
	public class ActPart : IDbObject
	{
		public int Id { get; set; }
		public string Name { get; set; }

		[InverseProperty("ActPart")]
		public List<WorkType> WorkTypes { get; set; }

		public void Set(IDbObject other)
		{
			throw new System.NotImplementedException();
		}
	}
}
