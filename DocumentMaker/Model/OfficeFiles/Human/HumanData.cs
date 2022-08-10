﻿using Dml.Controller.Validation;
using System.Collections.Generic;

namespace DocumentMaker.Model.OfficeFiles.Human
{
	public class HumanData
	{
		public string Name { get; set; }
		public string HumanIdText { get; set; }
		public string BankName { get; set; }
		public string PaymentAccountText { get; set; }
		public string ContractNumberText { get; set; }
		public string ContractDateText { get; set; }
		public string ContractReworkNumberText { get; set; }
		public string ContractReworkDateText { get; set; }
		public string AddressText { get; set; }
		public string MfoText { get; set; }

		public void SetData(char type, string value)
		{
			if (value != null)
			{
				value = StringValidator.Trim(value);

				switch (type)
				{
					case 'A': Name = value; break;
					case 'B': HumanIdText = value; break;
					case 'C': BankName = value; break;
					case 'D': PaymentAccountText = value; break;
					case 'E': ContractNumberText = value; break;
					case 'F': ContractDateText = value; break;
					case 'G': ContractReworkNumberText = value; break;
					case 'H': ContractReworkDateText = value; break;
					case 'I': AddressText = value; break;
					case 'J': MfoText = value; break;
				}
			}
		}

		public override bool Equals(object obj)
		{
			return obj is HumanData data &&
				   Name == data.Name;
		}

		public override int GetHashCode()
		{
			int hashCode = -718311778;
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(HumanIdText);
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(BankName);
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(PaymentAccountText);
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ContractNumberText);
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ContractDateText);
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ContractReworkNumberText);
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ContractReworkDateText);
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(AddressText);
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(MfoText);
			return hashCode;
		}

		public override string ToString()
		{
			return Name;
		}
	}
}
