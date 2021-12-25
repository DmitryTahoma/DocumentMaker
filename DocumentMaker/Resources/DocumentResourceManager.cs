using System.Collections.Generic;

namespace DocumentMaker.Resources
{
    internal static class DocumentResourceManager
    {
        public static IEnumerable<ResourceInfo> Items { get; }
            = new List<ResourceInfo>() {
                new ResourceInfo("DocumentMakerTemplate01.docx", "Додаток [DodatokNum] тех. завдання [TTDateStr2] [HumanSecondName].docx"),
                new ResourceInfo("DocumentMakerTemplate02.docx", "акт виконаних роб [ActDate2] [HumanSecondName].docx")
            };
    }
}
