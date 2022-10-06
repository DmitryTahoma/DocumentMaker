using Db.Context.ActPart;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Db.Context.HumanPart
{
	public class Human : IDbObject
	{
		public Human() { }

		public Human(Human other)
		{
			Id = other.Id;
			SetWithoutKeys(other);

			BankId = other.BankId;
			if (other.Bank != null) Bank = new Bank(other.Bank);
			DevelopmentContractId = other.DevelopmentContractId;
			if (other.DevelopmentContract != null) DevelopmentContract = new Contract(other.DevelopmentContract);
			SupportContractId = other.SupportContractId;
			if (other.SupportContract != null) SupportContract = new Contract(other.SupportContract);
			AddressId = other.AddressId;
			if (other.Address != null) Address = new Address(other.Address);
		}

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
		[Column(TypeName = "date")]
		public DateTime? FiredDate { get; set; }
		[Column(TypeName = "date")]
		public DateTime? EmploymentDate { get; set; }

		[InverseProperty("Human")]
		public List<Act> Acts { get; set; }

		public string FullName => Surname + ' ' + Name + ' ' + Secondname;

		public void Set(Human other)
		{
			SetWithoutKeys(other);
			SetBank(other.Bank, other.BankId);
			SetDevelopmentContract(other.DevelopmentContract, other.DevelopmentContractId);
			SetSupportContract(other.SupportContract, other.SupportContractId);
			SetAddress(other.Address);
		}

		public void Set(IDbObject other)
		{
			if (other is Human obj)
				Set(obj);
			else
				throw new InvalidOperationException();
		}

		private void SetWithoutKeys(Human other)
		{
			Name = other.Name;
			Surname = other.Surname;
			Secondname = other.Secondname;
			TIN = other.TIN;
			CheckingAccount = other.CheckingAccount;
			IsFired = other.IsFired;
			FiredDate = other.FiredDate;
			EmploymentDate = other.EmploymentDate;
		}

		private void SetBank(Bank bank, int? bankId)
		{
			if (bank != null)
			{
				Bank = bank;
				BankId = bank.Id;
			}
			else if (bankId != null)
			{
				BankId = bankId;
			}
		}

		private void SetDevelopmentContract(Contract contract, int? contractId)
		{
			if (contract != null)
			{
				if (DevelopmentContract == null) DevelopmentContract = new Contract();

				DevelopmentContract.Set(contract);
			}
			else if (contractId != null)
			{
				DevelopmentContractId = contractId;
			}
		}

		private void SetSupportContract(Contract contract, int? contractId)
		{
			if (contract != null)
			{
				if (SupportContract == null) SupportContract = new Contract();

				SupportContract.Set(contract);
			}
			else if(contractId != null)
			{
				SupportContractId = contractId;
			}
		}

		private void SetAddress(Address address)
		{
			if (address != null)
			{
				if (Address == null) Address = new Address();

				Address.Set(address);
			}
		}
	}
}
