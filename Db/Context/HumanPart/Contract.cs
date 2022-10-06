using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Db.Context.HumanPart
{
	public class Contract : IDbObject
	{
		public Contract() { }

		public Contract(Contract other)
		{
			Id = other.Id;
			Number = other.Number;
			PreparationDate = other.PreparationDate;
		}

		public int Id { get; set; }
		public string Number { get; set; }
		[Column(TypeName = "date")]
		public DateTime PreparationDate { get; set; }

		public void Set(Contract obj)
		{
			Number = obj.Number;
			PreparationDate = obj.PreparationDate;
		}

		public void Set(IDbObject other)
		{
			if (other is Contract obj)
				Set(obj);
			else
				throw new InvalidOperationException();
		}
	}
}
