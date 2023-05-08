using Dml.Model.Template;
using DocumentMakerModelLibrary.Algorithm;
using System;
using System.Globalization;

namespace DocumentMakerModelLibrary.OfficeFiles
{
	internal class DocumentGeneralData
	{
		private const string yearStr = "року";

		public DocumentGeneralData(DocumentMakerModel model, bool isExportRework, uint actSum, int countWorks)
		{
			CountWorks = countWorks.ToString();
			ActDate = model.ActDateText;
			HumanFullName = model.SelectedHuman;
			HumanID = model.HumanIdText;
			HumanAddress = model.AddressText;
			HumanBank = model.BankName;

			if (!string.IsNullOrEmpty(model.PaymentAccountText))
			{
				HumanPA = "р/р" + model.PaymentAccountText;
				HumanPA2 = model.PaymentAccountText;
			}

			if (!string.IsNullOrEmpty(model.MfoText))
			{
				HumanMFO = "МФО " + model.MfoText;
				HumanMFO2 = model.MfoText;
			}

			if (!isExportRework)
			{
				DogovorNum = model.ContractNumberText;
				DogovorFullDate = model.ContractDateText;
				NameOfWorkText = "розробки";
				if (model.TemplateType != DocumentTemplateType.Soundman)
					FirstPartText = " розробки ігрових прикладних програм Замовника";
			}
			else
			{
				DogovorNum = model.ContractReworkNumberText;
				DogovorFullDate = model.ContractReworkDateText;
				NameOfWorkText = "підтримки";
				if (model.TemplateType != DocumentTemplateType.Soundman)
					FirstPartText = " підтримки ігрових прикладних програм Замовника в придатному для використання стані в межах післяпродажного обслуговування без розширення чи поліпшення їх функціональних характеристик";
			}
			ActSum = actSum.ToString();
			CityName = model.CityName;
			RegionName = model.RegionName;
			ActSumText = NumberToWords.Str(actSum)?.TrimEnd();

			if (!(model.TemplateType == DocumentTemplateType.Support || model.TemplateType == DocumentTemplateType.Translator || model.TemplateType == DocumentTemplateType.Tester || model.TemplateType == DocumentTemplateType.Soundman))
				FirstPartText2 = "Замовник надав віддалений доступ до ігрових комп’ютерних програм, а ";

			if (!(model.TemplateType == DocumentTemplateType.Support || model.TemplateType == DocumentTemplateType.Translator || model.TemplateType == DocumentTemplateType.Tester))
				FirstPartText3 = "з надання послуг[FirstPartText] ";

			SecondPartText = (model.TemplateType == DocumentTemplateType.Support || model.TemplateType == DocumentTemplateType.Translator) ?
				"Виконавцем були виконані наступні роботи (надані такі послуги):" :
				"Договір виконано відповідно до технічного завдання №[TTDateY]/[TTDateM]/[TTDateD]/[TTNum2d] від [TTFullDate] (Додаток №1 до [DogovorType2] від [DogovorFullDate]) в наступних обсягах:";

			ThirdPartText = (model.TemplateType == DocumentTemplateType.Support || model.TemplateType == DocumentTemplateType.Translator) ?
				"По виконанню зазначених вище обсягів робіт Сторони претензій не мають." :
				"Результат надання послуг прийнято Замовником, послуги надані у відповідності до договору з застосуванням власного обладнання та програмного забезпечення, по виконанню зазначених вище обсягів послуг по Договору Сторони претензій не мають.";

			switch (model.TemplateType)
			{
				case DocumentTemplateType.Scripter: AddictionInfo = "Мова програмування С++, скриптувальна мова"; break;
				case DocumentTemplateType.Cutter: AddictionInfo = "Використання графічного пакету Autodesk 3ds Max Commercial New Single-user ELD Annual Subscription Використання графічного пакету Photoshop CC ALL Multiple Platforms Multi European Languages Licensing Subscription."; break;
				case DocumentTemplateType.Painter: AddictionInfo = "Використання графічного пакету Photoshop CC ALL Multiple Platforms Multi European Languages Licensing Subscription"; break;
				case DocumentTemplateType.Modeller: AddictionInfo = "Використання графічного пакету Autodesk 3ds Max Commercial New Single-user ELD Annual Subscription"; break;
				case DocumentTemplateType.Programmer: AddictionInfo = "Мова програмування С++, Java, Python, PHP, скриптувальна мова"; break;
				case DocumentTemplateType.Animator: AddictionInfo = "Використання графічного пакету Autodesk 3ds Max Commercial New Single-user ELD Annual Subscription"; break;
			}

			TTFullDate = DateToFormatCultureString(DateTime.Parse(model.TechnicalTaskDateText));
			TTDateY = DateTime.Parse(model.TechnicalTaskDateText).Year.ToString();
			TTDateM = Get2dNumber(DateTime.Parse(model.TechnicalTaskDateText).Month);
			TTDateD = Get2dNumber(DateTime.Parse(model.TechnicalTaskDateText).Day);
			TTNum2d = Get2dNumber(model.TechnicalTaskNumText);
			HumanName = GetHumanName();
			ActFullDate = DateToFormatCultureString(DateTime.Parse(ActDate));
			TTDateStr2 = DateTime.Parse(model.TechnicalTaskDateText).ToString("yyyy.MM.dd");
			HumanSecondName = HumanFullName.Split(' ')[0];
			ActDate2 = DateTime.Parse(ActDate).ToString("yyyy.MM.dd");
			ActSumTextPart1 = ActSumText.Substring(0, ActSumText.LastIndexOf(' ')).ToLower();
			ActSumTextPart2 = GetActSumTextPart2();

			if (model.DocType == DocumentType.FOP || model.DocType == DocumentType.FOPF)
			{
				HumanNameFOP = "ФОП " + HumanName;
				HumanNameFOP2 = "Фізична особа - підприємець " + HumanFullName;
			}
			else
			{
				HumanNameFOP = HumanName;
				HumanNameFOP2 = "Фізична особа " + HumanFullName;
			}

			if (model.DocType == DocumentType.FOP || model.DocType == DocumentType.FOPF || model.DocType == DocumentType.GIG)
			{
				DogovorType = "Договору №" + DogovorNum;
				DogovorType2 = "Договору №" + DogovorNum;
			}
			else
			{
				DogovorType = "Трудового договору";
				DogovorType2 = "Договору";
			}


			string[] dateMonth = ActDate.Split('.');
			if (dateMonth.Length == 3)
			{
				if (model.AccountNumberText != null)
				{
					string year = dateMonth[2][2].ToString() + dateMonth[2][3].ToString();
					AccountNumberText = model.AccountNumberText;
					AccountNumberText = AccountNumberText.Replace("#y", dateMonth[2]);
					AccountNumberText = AccountNumberText.Replace("#m", dateMonth[1]);
					AccountNumberText = AccountNumberText.Replace("#d", dateMonth[0]);
					AccountNumberText = AccountNumberText.Replace("#sy", year);
				}
			}
			else
				AccountNumberText += "Невірний номер";


			if (model.DocType == DocumentType.FOPF)
			{
				CustomerText = "ФОП Фролов О. В.";
				CustomerText2 = "Фізична особа-підприємець Фролов Олександр Вікторович";
				CustomerText3 = "Фролов О. В.";
				Director = "";
				DirectorID = "ІН 2978003352";
				DirectorAddress = "пр-зд. Білоруський, буд. 14, кв. 22";
				DirectorPA = "р/рUA323052990000026006050298289";
			}
			else
			{
				CustomerText = "ТОВ \"ФАЙВ - БН СТУДІЯ\"";
				CustomerText2 = "Товариство з обмеженою відповідальністю «ФАЙВ-БН СТУДІЯ», в особі директора Фролова Олександра Вікторовича";
				CustomerText3 = "Фролов О. В.";
				Director = "Директор ";
				DirectorID = "ЄДРПОУ 38187315";
				DirectorAddress = "вул. Менделєєва, буд 46, прим.9";
				DirectorPA = "р/рUA953052990000026008050250430";
			}
		}

		public string NameOfWorkText { get; }

		public string FirstPartText { get; } = string.Empty;

		public string FirstPartText2 { get; } = string.Empty;

		public string FirstPartText3 { get; set; } = string.Empty;

		public string SecondPartText { get; set; }

		public string ThirdPartText { get; }

		public string DogovorNum { get; }
		public string DogovorType { get; }
		public string DogovorType2 { get; }

		public string DogovorFullDate { get; }
		public string DogovorFullDate2 { get; }

		public string TTFullDate { get; }

		public string TTDateY { get; }

		public string TTDateM { get; }

		public string TTDateD { get; }

		public string TTNum2d { get; }

		public string ActDate { get; }

		public string CityName { get; }
		public string RegionName { get; }

		public string HumanName { get; }

		public string HumanID { get; }

		public string HumanAddress { get; }

		public string HumanPA { get; }
		public string HumanPA2 { get; }

		public string HumanBank { get; }

		public string HumanMFO { get; }
		public string HumanMFO2 { get; }

		public string ActFullDate { get; }

		public string HumanFullName { get; }

		public string TTDateStr2 { get; }

		public string HumanSecondName { get; }
		public string HumanNameFOP { get; }
		public string HumanNameFOP2 { get; }

		public string ActDate2 { get; }

		public string AddictionInfo { get; } = string.Empty;

		public string ActSum { get; }

		public string ActSumText { get; }

		public string ActSumTextPart1 { get; }

		public string ActSumTextPart2 { get; }
		public string CountWorks { get; }
		public string AccountNumberText { get; } = string.Empty;
		public string CustomerText { get; } = string.Empty;
		public string CustomerText2 { get; }
		public string CustomerText3 { get; }
		public string Director { get; }
		public string DirectorID { get; }
		public string DirectorAddress { get; }
		public string DirectorPA { get; }


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

		private string GetHumanName()
		{
			string[] parts = HumanFullName.Split(' ');
			return parts[0] + ' ' + char.ToUpper(parts[1][0]) + ". " + char.ToUpper(parts[2][0]) + '.';
		}

		private string GetActSumTextPart2()
		{
			string res = ActSumText;
			int index = res.LastIndexOf(' ');
			return res.Substring(index + 1, res.Length - index - 1);
		}
	}
}
