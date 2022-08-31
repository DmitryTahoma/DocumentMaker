using DocumentMaker.Model.OfficeFiles;
using System.Collections.Generic;

namespace DocumentMaker.Resources
{
	internal static class DocumentResourceManager
	{
		public static IEnumerable<ResourceInfo> GetItems(bool isExportRework)
		{
			string addictionStr = BackTaskStrings.GetAddictionName(isExportRework);
			return new List<ResourceInfo>
			{
				new ResourceInfo(ResourceType.Docx, "DocumentMakerTemplate01.docx", addictionStr + "Додаток 1 тех. завдання [TTDateStr2] [HumanSecondName].docx"),
				new ResourceInfo(ResourceType.Docx, "DocumentMakerTemplate02.docx", addictionStr + "акт виконаних роб [ActDate2] [HumanSecondName].docx")
			};
		}
	}
}
