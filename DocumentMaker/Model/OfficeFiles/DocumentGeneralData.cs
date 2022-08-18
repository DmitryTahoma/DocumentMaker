using Dml.Model.Template;
using DocumentMaker.Model.Algorithm;
using System;
using System.Globalization;

namespace DocumentMaker.Model.OfficeFiles
{
	internal class DocumentGeneralData
	{
		private const string yearStr = "року";

		private readonly string technicalTaskDateText;

		public DocumentGeneralData(DocumentMakerModel model, bool isExportRework, uint actSum)
		{
			technicalTaskDateText = model.TechnicalTaskDateText;
			ActDate = model.ActDateText;
			DodatokNum = model.AdditionNumText;
			HumanFullName = model.SelectedHuman;
			HumanID = model.HumanIdText;
			HumanAddress = model.AddressText;
			HumanPA = model.PaymentAccountText;
			HumanBank = model.BankName;
			HumanMFO = "МФО " + model.MfoText;
			if(!isExportRework)
			{
				DogovorNum = model.ContractNumberText;
				DogovorFullDate = model.ContractDateText;
			}
			else
			{
				DogovorNum = model.ContractReworkNumberText;
				DogovorFullDate = model.ContractReworkDateText;
			}
			ActSum = actSum.ToString();
			CityName = model.CityName;
			ActSumText = NumberToWords.Str(actSum);

			switch (model.TemplateType)
			{
				case DocumentTemplateType.Scripter: AddictionInfo = "Мова програмування С++, скриптувальна мова"; break;
				case DocumentTemplateType.Cutter: AddictionInfo = "Використання графічного пакету Autodesk 3ds Max Commercial New Single-user ELD Annual Subscription Використання графічного пакету Photoshop CC ALL Multiple Platforms Multi European Languages Licensing Subscription."; break;
				case DocumentTemplateType.Painter: AddictionInfo = "Використання графічного пакету Autodesk 3ds Max  Commercial New Single-user ELD Annual Subscription"; break;
				case DocumentTemplateType.Modeller: AddictionInfo = "Використання графічного пакету Photoshop CC ALL Multiple Platforms Multi European Languages Licensing Subscription"; break;
			}
		}

		public string DodatokNum { get; }

		public string DogovorNum { get; }

		public string DogovorFullDate { get; }

		public string TTFullDate => DateToFormatCultureString(DateTime.Parse(technicalTaskDateText));

		public string TTDateY => DateTime.Parse(technicalTaskDateText).Year.ToString();

		public string TTDateM => Get2dNumber(DateTime.Parse(technicalTaskDateText).Month);

		public string TTDateD => Get2dNumber(DateTime.Parse(technicalTaskDateText).Day);

		public string DodatokNum2d => Get2dNumber(DodatokNum);

		public string ActDate { get; }

		public string CityName { get; }

		public string HumanName
		{
			get
			{
				string[] parts = HumanFullName.Split(' ');
				return parts[0] + ' ' + char.ToUpper(parts[1][0]) + ". " + char.ToUpper(parts[2][0]) + '.';
			}
		}

		public string HumanID { get; }

		public string HumanAddress { get; }

		public string HumanPA { get; }

		public string HumanBank { get; }

		public string HumanMFO { get; }

		public string ActFullDate => DateToFormatCultureString(DateTime.Parse(ActDate));

		public string HumanFullName { get; }

		public string TTDateStr2 => DateTime.Parse(technicalTaskDateText).ToString("yyyy.MM.dd");

		public string HumanSecondName => HumanFullName.Split(' ')[0];

		public string ActDate2 => DateTime.Parse(ActDate).ToString("yyyy.MM.dd");

		public string AddictionInfo { get; }

		public string ActSum { get; }

		public string ActSumText { get; }

		private string Get2dNumber(string str)
		{
			while (str.Length < 2)
			{
				str = '0' + str;
			}

			return str;
		}

		private string Get2dNumber(int number)
		{
			return Get2dNumber(number.ToString());
		}

		private string DateToFormatCultureString(DateTime date)
		{
			CultureInfo ua = CultureInfo.GetCultureInfo("uk-ua");

			string month = ua.DateTimeFormat.MonthGenitiveNames[date.Month - 1];
			return Get2dNumber(date.Day) + ' ' + month + ' ' + date.Year.ToString() + ' ' + yearStr;
		}
	}
}
