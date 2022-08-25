using System;
using System.Text;

namespace DocumentMaker.Model.Algorithm
{
	public static class NumberToWords
	{
		//Наименования сотен
		private static readonly string[] hunds =
		{
			"", "сто ", "двісті ", "триста ", "чотириста ",
			"п'ятсот ", "шістсот ", "сімсот ", "вісімсот ", "дев'ятсот "
		};
		//Наименования десятков
		private static readonly string[] tens =
		{
			"", "десять ", "двадцять ", "тридцять ", "сорок ", "п'ятдесят ",
			"шістдесят ", "сімдесят ", "вісімдесят ", "дев'яносто "
		};
		/// <summary>
		/// Перевод в строку числа с учётом падежного окончания относящегося к числу существительного
		/// </summary>
		/// <param name="val">Число</param>
		/// <param name="male">Род существительного, которое относится к числу</param>
		/// <param name="one">Форма существительного в единственном числе</param>
		/// <param name="two">Форма существительного от двух до четырёх</param>
		/// <param name="five">Форма существительного от пяти и больше</param>
		/// <returns></returns>
		private static string Str(long val, bool male, string one, string two, string five)
		{
			string[] frac20 =
			{
				"", "одна ", "дві ", "три ", "чотири ", "п'ять ", "шість ",
				"сім ", "вісім ", "дев'ять ", "десять ", "одинадцять ",
				"дванадцять ", "тринадцять ", "чотирнадцять ", "п'ятнадцять ",
				"шістнадцять ", "сімнадцять ", "вісімнадцять ", "дев'ятнадцять "
			};

			long num = val % 1000;
			if (0 == num) return "";
			if (num < 0) throw new ArgumentOutOfRangeException("val", "Параметр не може бути негативним");
			if (!male)
			{
				frac20[1] = "один ";
				frac20[2] = "два ";
			}

			StringBuilder r = new StringBuilder(hunds[num / 100]);

			if (num % 100 < 20)
			{
				r.Append(frac20[num % 100]);
			}
			else
			{
				r.Append(tens[num % 100 / 10]);
				r.Append(frac20[num % 10]);
			}

			r.Append(Case(num, one, two, five));

			if (r.Length != 0) r.Append(" ");
			return r.ToString();
		}
		/// <summary>
		/// Выбор правильного падежного окончания сущесвительного
		/// </summary>
		/// <param name="val">Число</param>
		/// <param name="one">Форма существительного в единственном числе</param>
		/// <param name="two">Форма существительного от двух до четырёх</param>
		/// <param name="five">Форма существительного от пяти и больше</param>
		/// <returns>Возвращает существительное с падежным окончанием, которое соответсвует числу</returns>
		private static string Case(long val, string one, string two, string five)
		{
			long t = (val % 100 > 20) ? val % 10 : val % 20;

			switch (t)
			{
				case 1: return one;
				case 2: case 3: case 4: return two;
				default: return five;
			}
		}
		/// <summary>
		/// Перевод целого числа в строку
		/// </summary>
		/// <param name="val">Число</param>
		/// <returns>Возвращает строковую запись числа</returns>
		public static string Str(long val)
		{
			bool minus = false;
			if (val < 0) { val = -val; minus = true; }

			long n = val;

			StringBuilder r = new StringBuilder();
			string grn = "";

			if (0 == n) r.Append("0 ");
			if (n % 1000 != 0)
				r.Append(Str(n, true, "гривня", "гривні", "гривень"));
			else
				grn = "гривень";

			n /= 1000;

			r.Insert(0, Str(n, true, "тисяча", "тисячі", "тисяч"));
			n /= 1000;

			r.Insert(0, Str(n, false, "мільйон", "мільйона", "мільйонів"));
			n /= 1000;

			r.Insert(0, Str(n, false, "мільярд", "мільярда", "мільярдів"));
			n /= 1000;

			r.Insert(0, Str(n, false, "трильйон", "трильйона", "трильйонів"));
			n /= 1000;

			r.Insert(0, Str(n, false, "трильярд", "трильярда", "трильярдів"));

			r.Append(grn);

			if (minus) r.Insert(0, "минус ");

			//Делаем первую букву заглавной
			r[0] = char.ToUpper(r[0]);

			return r.ToString();
		}
	}
}
