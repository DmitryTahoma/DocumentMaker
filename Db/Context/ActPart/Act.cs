using Db.Context.HumanPart;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Db.Context.ActPart
{
	public class Act : IDbObject
	{
		public int Id { get; set; }
		public int? HumanId { get; set; }
		public Human Human { get; set; }
		public int? TemplateTypeId { get; set; }
		public TemplateType TemplateType { get; set; }
		public DateTime CreationTime { get; set; }

		[InverseProperty("Act")]
		public List<FullAct> FullActs { get; set; }
		[InverseProperty("Act")]
		public List<Work> Works { get; set; }

		public void Set(IDbObject other)
		{
			throw new NotImplementedException();
		}
	}
}
