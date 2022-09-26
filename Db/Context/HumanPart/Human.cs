using Db.Context.ActPart;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Db.Context.HumanPart
{
	public class Human
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Surname { get; set; }
		public string Secondname { get; set; }
		/// <summary>
		/// Taxpayer Identification Number (ИНН Индивидуальный номер налогоплательщика)
		/// </summary>
		public long TIN { get; set; }
		public int? BankId { get; set; }
		public Bank Bank { get; set; }
		public string CheckingAccount { get; set; }
		public int? DevelopmentContractId { get; set; }
		[ForeignKey("DevelopmentContractId")]
		public Contract DevelopmentContract { get; set; }
		public int? SupportContractId { get; set; }
		[ForeignKey("SupportContractId")]
		public Contract SupportContract { get; set; }
		public int? AddressId { get; set; }
		public Address Address { get; set; }
		public bool IsFired { get; set; }

		[InverseProperty("Human")]
		public List<Act> Acts { get; set; }
	}
}
