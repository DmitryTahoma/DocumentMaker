using FBNUtils;

namespace DocumentMaker.Settings
{
	internal class ProgramSettings : BaseProgramSettings
	{
		static ProgramSettings()
		{
			Init(typeof(ProgramSettings));
			Reload();
		}

		public static string DirectoryPath => GetDirectoryPath();
		public static string TempDirectoryPath => GetTempDirectoryPath();
		public static string UniquePostfix => GetUniquePostfix();

		public static void Reload()
		{
			InnerReload();
		}

		public static void Save()
		{
			InnerSave();
		}
	}
}
