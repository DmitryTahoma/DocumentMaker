using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Db.Context.ActPart
{
	public class Work : IDbObject
	{
		public int Id { get; set; }
		[ForeignKey("Act")]
		public int? ActId { get; set; }
		public Act Act { get; set; }
		public int? WorkTypeId { get; set; }
		public WorkType WorkType { get; set; }
		public int? ActPartId { get; set; }
		public ActPart ActPart { get; set; }
		public int? WorkBackAdapterId { get; set; }
		public WorkBackAdapter WorkBackAdapter { get; set; }
		public int SpentTime { get; set; }

		[InverseProperty("Work")]
		public List<FullWork> FullWorks { get; set; }
		[InverseProperty("Work")]
		public List<Regions> Regions { get; set; }

		public void Set(IDbObject other)
		{
			throw new System.NotImplementedException();
		}
	}
}
