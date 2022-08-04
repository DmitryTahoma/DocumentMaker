using System.Collections.Generic;

namespace DocumentMaker.Resources
{
	internal static class DocumentResourceManager
	{
		public static IEnumerable<ResourceInfo> Items { get; }
			= new List<ResourceInfo>() {
				new ResourceInfo(ResourceType.Docx, "DocumentMakerTemplate01.docx", "Додаток [DodatokNum] тех. завдання [TTDateStr2] [HumanSecondName].docx"),
				new ResourceInfo(ResourceType.Docx, "DocumentMakerTemplate02.docx", "акт виконаних роб [ActDate2] [HumanSecondName].docx"),
				new ResourceInfo(ResourceType.Xlsx, "DocumentMakerTemplate03.xlsx", "Время затраченное на работу [ActDate2] [HumanSecondName].xlsx")
			};
	}
}
