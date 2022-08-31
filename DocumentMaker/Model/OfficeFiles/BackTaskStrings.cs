using Dml.Model.Back;
using Dml.Model.Template;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DocumentMaker.Model.OfficeFiles
{
	public static class BackTaskStrings
	{
		public static string Generate(BackType backType, DocumentTemplateType templateType, string workText, string episodeNumberStr, string backNumberStr, string backName, string backRegsStr, string gameName, bool isRework, bool isSketch, bool haveEpisodes)
		{
			string str = GetBaseString(backType, templateType, workText, isSketch, haveEpisodes);

			str = str.Replace("[BackNumber]", backNumberStr)
				.Replace("[BackName]", backName)
				.Replace("[RegionsText]", backRegsStr)
				.Replace("[GameName]", gameName)
				.Replace("[EpisodeNumber]", episodeNumberStr);

			return str;
		}

		private static string GetBaseString(BackType backType, DocumentTemplateType templateType, string workText, bool isSketch, bool haveEpisodes)
		{
			StringBuilder stringBuilder = new StringBuilder(workText + (templateType == DocumentTemplateType.Painter && isSketch ? " ескізу " : " "));
			if (stringBuilder.Length > 0)
			{
				stringBuilder[0] = char.ToUpper(stringBuilder[0]);
			}

			if (stringBuilder.Length > 0)
			{
				if (haveEpisodes) stringBuilder.Append("Епізод №[EpisodeNumber] ");

				switch (backType)
				{
					case BackType.Back: stringBuilder.Append("Беку №[BackNumber]"); break;
					case BackType.Regions: stringBuilder.Append("Регіону №[RegionsText] Беку №[BackNumber]"); break;
					case BackType.Dialog: stringBuilder.Append("Беку Діалог №[BackNumber]"); break;
					case BackType.Mg: stringBuilder.Append("Беку Мініігра №[BackNumber]"); break;
					case BackType.Hog: stringBuilder.Append("Беку ХОГ №[BackNumber]"); break;
					case BackType.HogRegions: stringBuilder.Append("Регіону №[RegionsText] Беку ХОГ №[BackNumber]"); break;
					case BackType.Craft: stringBuilder.Append("Регіону крафтингу предмету"); break;
					default: stringBuilder.Clear(); break;
				}
			}

			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(" ([BackName]) комп’ютерної гри “[GameName]”.");
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
