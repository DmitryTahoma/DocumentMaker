using Dml.Model.Back;
using Dml.Model.Template;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DocumentMakerModelLibrary.OfficeFiles
{
	public static class BackTaskStrings
	{
		public static string Generate(BackType backType, DocumentTemplateType templateType, string workText, string episodeNumberStr, string backNumberStr, string backName, string backRegsStr, string gameName, bool isRework, bool isSketch, bool haveEpisodes)
		{
			string str = GetBaseString(backType, templateType, workText, isSketch, haveEpisodes, episodeNumberStr);

			str = str.Replace("[BackNumber]", backNumberStr)
				.Replace("[BackName]", backName)
				.Replace("[RegionsText]", backRegsStr)
				.Replace("[GameName]", gameName)
				.Replace("[EpisodeNumber]", episodeNumberStr);

			return str;
		}

		private static string GetBaseString(BackType backType, DocumentTemplateType templateType, string workText, bool isSketch, bool haveEpisodes, string episodeNumber)
		{
			StringBuilder stringBuilder = new StringBuilder(workText + (templateType == DocumentTemplateType.Painter && isSketch ? " ескізу" : ""));
			if (stringBuilder.Length > 0)
			{
				stringBuilder[0] = char.ToUpper(stringBuilder[0]);
			}

			if (stringBuilder.Length > 0)
			{
				switch (backType)
				{
					case BackType.Predmets: stringBuilder.Append(" Предметів на панель"); break;
					case BackType.Morf: stringBuilder.Append(" Морфів"); break;
					case BackType.Collection: stringBuilder.Append(" Коллекцій"); break;
					case BackType.Marketing: stringBuilder.Append(" Маркетингового типу для платформи ([BackName])"); break;
					default: break;
				}

				if (haveEpisodes && episodeNumber != "Всі") stringBuilder.Append(" Епізоду №[EpisodeNumber]");

				switch (backType)
				{
					case BackType.Back: stringBuilder.Append(" Беку №[BackNumber]"); break;
					case BackType.Regions: stringBuilder.Append(" Регіону №[RegionsText] Беку №[BackNumber]"); break;
					case BackType.Dialog: stringBuilder.Append(" Беку Діалог №[BackNumber]"); break;
					case BackType.Mg: stringBuilder.Append(" Беку Мініігра №[BackNumber]"); break;
					case BackType.Hog: stringBuilder.Append(" Беку ХОГ №[BackNumber]"); break;
					case BackType.HogRegions: stringBuilder.Append(" Регіону №[RegionsText] Беку ХОГ №[BackNumber]"); break;
					case BackType.Craft: stringBuilder.Append(" Регіону крафтингу предмету"); break;
					case BackType.Predmets: break;
					case BackType.Predmet: stringBuilder.Append(" Предмету"); break;
					case BackType.Morf: break;
					case BackType.Collection: break;
					case BackType.Character: stringBuilder.Append(" Персонажу"); break;
					case BackType.Interface: stringBuilder.Append(" Вікна інтерфейсу"); break;
					case BackType.Marketing: break;
					case BackType.VideoCadr: stringBuilder.Append(" Кадрів відеоролика"); break;
					case BackType.VideoObject: stringBuilder.Append(" Кадрів iстоти"); break;
					case BackType.VideoPredmet: stringBuilder.Append(" Кадрів предмету"); break;
					default: stringBuilder.Clear(); break;
				}
			}

			if (stringBuilder.Length > 0)
			{
				switch (backType)
				{
					case BackType.Morf:
					case BackType.Collection:
					case BackType.Marketing:
					case BackType.Predmets: stringBuilder.Append(" комп’ютерної гри “[GameName]”."); break;
					case BackType.Character: stringBuilder.Append(" ([BackName]) для Беків Діалогів комп’ютерної гри “[GameName]”."); break;
					default: stringBuilder.Append(" ([BackName]) комп’ютерної гри “[GameName]”."); break;
				}
				
			}

			return stringBuilder.ToString();
		}

		public static string GetRegionString(BackType backType, string countRegs)
		{
			if (backType != BackType.Regions && backType != BackType.HogRegions)
				return string.Empty;

			if (countRegs.Contains(",") || countRegs.Contains("-"))
			{
				List<int> regs = new List<int>();

				countRegs = Regex.Replace(countRegs, @"\s+", "");
				string[] parts = countRegs.Split(',');

				foreach (string part in parts)
				{
					if (!string.IsNullOrEmpty(part))
					{
						if (part.Contains("-"))
						{
							string[] subparts = part.Split('-');
							if (subparts.Length > 1
								&& int.TryParse(subparts[0], out int st)
								&& int.TryParse(subparts[1], out int end))
							{
								for (int i = st; i <= end; ++i)
								{
									regs.Add(i);
								}
							}
						}
						else if (int.TryParse(part, out int r))
						{
							regs.Add(r);
						}
					}
				}

				regs = regs.OrderBy(x => x).Distinct().ToList();

				string res = "";
				foreach (int reg in regs)
				{
					if (res != "")
					{
						res += ", ";
					}
					res += reg.ToString();
				}
				return res;
			}
			else
			{
				string regs = "";

				if (int.TryParse(countRegs, out int count))
				{
					regs = "1";

					for (int i = 2; i <= count; ++i)
					{
						regs += ", " + i.ToString();
					}
				}

				return regs;
			}
		}

		public static string GetAddictionName(bool isExportRework) => isExportRework ? "Підтримка_" : "Розробка_";

		public static string GetRandomString()
		{
			return "temp_" + DateTime.Now.ToString("yyyyMMddHHmmssfffffff");
		}
	}
}
