using Dml.Model.Back;
using Dml.Model.Template;

namespace DocumentMaker.Model.OfficeFiles
{
	public static class BackTaskStrings
	{
		public static string Generate(BackType backType, DocumentTemplateType templateType, string backNumberStr, string backName, string backRegsStr, string gameName, bool isRework, bool isSketch)
		{
			string str = GetBaseString(backType, templateType, isRework, isSketch);

			str = str.Replace("[BackNumber]", backNumberStr);
			str = str.Replace("[BackName]", backName);
			str = str.Replace("[RegionsText]", backRegsStr);
			str = str.Replace("[GameName]", gameName);

			return str;
		}

		private static string GetBaseString(BackType backType, DocumentTemplateType templateType, bool isRework, bool isSketch)
		{
			string res;
			switch (templateType)
			{
				case DocumentTemplateType.Scripter: res = isRework ? "Актуалізація логіки поведінки об’єктів " : "Послуги з розробки логіки та візуальних ефектів "; break;
				case DocumentTemplateType.Cutter: res = isRework ? "Коригування об’єктів анімацій пошарової 3D моделі та візуальних ефектів " : "Послуги з розробки пошарової 3D моделі та візуальних ефектів "; break;
				case DocumentTemplateType.Painter:
					{
						res = isRework ? "Графічні роботи з коригування кольорової гами " : "Послуги з розробки графічних матеріалів ";
						if (isSketch)
							res += "ескізу ";
					}
					break;
				case DocumentTemplateType.Modeller: res = isRework ? "Коригування текстур 3D - моделі " : "Послуги з розробки 3D - моделі "; break;
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
	}
}
