using Dml.Controller.Validation;
using Dml.Model.Template;
using System.Collections.Generic;

namespace DocumentMakerModelLibrary.OfficeFiles.Human
{
	public class HumanData
	{
		public string Name { get; set; }
		public DocumentType DocType { get; set; } = DocumentType.Empty;
		public string HumanIdText { get; set; }
		public string BankName { get; set; }
		public string PaymentAccountText { get; set; }
		public string ContractNumberText { get; set; }
		public string ContractDateText { get; set; }
		public string ContractReworkNumberText { get; set; }
		public string ContractReworkDateText { get; set; }
		public string AddressText { get; set; }
		public string ContractFile { get; set; }
		public string CityName { get; set; }
		public string RegionName { get; set; }
		public string MfoText { get; set; }
		public string DefaultTemplate { get; set; }
		public string AccountNumberText { get; set; }

		public void SetData(char type, string value)
		{
			if (value != null)
			{
				value = StringValidator.Trim(value);

				switch (type)
				{
					case 'A': Name = value; break;
					case 'B':
					{
						switch(value)
						{
							case "ГИГ": DocType = DocumentType.GIG; break;
							case "ФОП": DocType = DocumentType.FOP; break;
							case "ФОПФ": DocType = DocumentType.FOPF; break;
							case "Штат": DocType = DocumentType.Staff; break;
							default: DocType = DocumentType.GIG; break;
						};
						break;
					}
					case 'C': HumanIdText = value; break;
					case 'D': BankName = value; break;
					case 'E': PaymentAccountText = value; break;
					case 'F': ContractNumberText = value; break;
					case 'G': ContractDateText = value; break;
					case 'H': ContractReworkNumberText = value; break;
					case 'I': ContractReworkDateText = value; break;
					case 'J': RegionName = value; break;
					case 'K': CityName = value; break;
					case 'L': AddressText = value; break;
					case 'M': MfoText = value; break;
					case 'N': DefaultTemplate = value; break;
					case 'O': AccountNumberText = value; break;
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
