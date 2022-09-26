using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Db.Context.HumanPart
{
	public class Contract
	{
		public int Id { get; set; }
		public string Number { get; set; }
		[Column(TypeName = "date")]
		public DateTime PreparationDate { get; set; }
	}
}
