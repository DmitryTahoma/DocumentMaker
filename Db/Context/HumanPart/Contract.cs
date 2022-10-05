using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Db.Context.HumanPart
{
	public class Contract : IDbObject
	{
		public int Id { get; set; }
		public string Number { get; set; }
		[Column(TypeName = "date")]
		public DateTime PreparationDate { get; set; }

		public void Set(IDbObject other)
		{
			throw new NotImplementedException();
		}
	}
}
