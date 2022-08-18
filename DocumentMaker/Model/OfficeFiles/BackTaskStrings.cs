using Dml.Model.Back;
using Dml.Model.Template;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DocumentMaker.Model.OfficeFiles
{
	public static class BackTaskStrings
	{
		public static string Generate(BackType backType, DocumentTemplateType templateType, string workText, string backNumberStr, string backName, string backRegsStr, string gameName, bool isRework, bool isSketch)
		{
			string str = GetBaseString(backType, templateType, workText, isRework, isSketch);

			str = str.Replace("[BackNumber]", backNumberStr);
			str = str.Replace("[BackName]", backName);
			str = str.Replace("[RegionsText]", backRegsStr);
			str = str.Replace("[GameName]", gameName);

			return str;
		}

		private static string GetBaseString(BackType backType, DocumentTemplateType templateType, string workText, bool isRework, bool isSketch)
		{
			string res;
			if (isRework) res = workText + (isSketch ? " ескізу " : " ");
			else switch (templateType)
			{
				case DocumentTemplateType.Scripter: res = "Послуги з розробки логіки та візуальних ефектів "; break;
				case DocumentTemplateType.Cutter: res = "Послуги з розробки пошарової 3D моделі та візуальних ефектів "; break;
				case DocumentTemplateType.Painter:
					{
						res = "Послуги з розробки графічних матеріалів ";
						if (isSketch)
							res += "ескізу ";
					}
					break;
				case DocumentTemplateType.Modeller: res = "Послуги з розробки 3D - моделі "; break;
				default: res = string.Empty; break;
			}

			if (!string.IsNullOrEmpty(res))
			{
				switch (backType)
				{
					case BackType.Back: res += "Беку №[BackNumber]"; break;
					case BackType.Regions: res += "Регіону №[RegionsText] Беку №[BackNumber]"; break;
					case BackType.Dialog: res += "Беку Діалог №[BackNumber]"; break;
					case BackType.Mg: res += "Беку Мініігра №[BackNumber]"; break;
					case BackType.Hog: res += "Беку ХОГ №[BackNumber]"; break;
					case BackType.HogRegions: res += "Регіону №[RegionsText] Беку ХОГ №[BackNumber]"; break;
					case BackType.Craft: res += "Регіону крафтингу предмету"; break;
					default: res = string.Empty; break;
				}
			}

			if (!string.IsNullOrEmpty(res))
			{
				res += " ([BackName]) комп’ютерної гри “[GameName]”.";
			}

			return res;
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

				foreach(string part in parts)
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
				foreach(int reg in regs)
				{
					if(res != "")
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
	}
}
