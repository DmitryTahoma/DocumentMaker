namespace DocumentMaker.Model.OfficeFiles
{
    public static class BackTaskStrings
    {
        public static string Generate(BackType backType, string backNumberStr, string backName, string backRegsStr, bool isRework)
        {
            string str = GetBaseString(backType, isRework);

            str = str.Replace("[BackNumber]", backNumberStr);
            str = str.Replace("[BackName]", backName);
            str = str.Replace("[RegionsText]", backRegsStr);
            //str = str.Replace("[GameName]", gameName);

            return str;
        }

        private static string GetBaseString(BackType backType, bool isRework)
        {
            string res = "Відповідно до вимог Замовника, за допомоги скриптувальної мови, наданою Замовником, та мови програмування С++, провести [IsRework] логіки та візуальних ефектів ";

            switch (backType)
            {
                case BackType.Back: res += "Беку №[BackNumber]"; break;
                case BackType.Regions: res += "Регіону №[RegionsText] Беку №[BackNumber]"; break;
                case BackType.Dialog: res += "Беку Діалог №[BackNumber]"; break;
                case BackType.Mg: res += "Беку Мініігра №[BackNumber]"; break;
                case BackType.Hog: res += "Беку ХОГ №[BackNumber]"; break;
                case BackType.HogRegions: res += "Регіону №[RegionsText] Беку ХОГ №[BackNumber]"; break;
                case BackType.Craft: res += "Регіону крафтингу предмету"; break;
                default: res = ""; break;
            }

            if (!string.IsNullOrEmpty(res))
            {
                res += " ([BackName]) комп’ютерної гри “[GameName]”, наданих Замовником.";

                string actionStr = isRework ? "дообробку" : "роботи по створенню";
                res = res.Replace("[IsRework]", actionStr);
            }

            return res;
        }
    }
}
