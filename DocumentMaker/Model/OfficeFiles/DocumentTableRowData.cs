namespace DocumentMaker.Model.OfficeFiles
{
    class DocumentTableRowData
    {
        private readonly uint id;
        private readonly BackType type;
        private readonly string backNumberText;
        private readonly string backName;
        private readonly string backCountRegionsText;
        private readonly string gameName;
        private readonly bool isRework;
        private readonly string spentTimeText;

        public DocumentTableRowData(BackDataModel model)
        {
            id = model.Id;
            type = model.Type;
            backNumberText = model.BackNumberText;
            backName = model.BackName;
            backCountRegionsText = model.BackCountRegionsText;
            gameName = model.GameName;
            isRework = model.IsRework;
            spentTimeText = model.SpentTimeText;
        }

        public string BackDataId => id.ToString();
        public string BackDataText => GenerateBackTask();

        private string GenerateBackTask()
        {
            string regs = "";
            if ((type == BackType.Regions || type == BackType.HogRegions)
              && int.TryParse(backCountRegionsText, out int count))
            {
                regs = "1";

                for (int i = 2; i <= count; ++i)
                {
                    regs += ", " + i.ToString();
                }
            }

            return BackTaskStrings.Generate(type, backNumberText, backName, regs, gameName, isRework);
        }
    }
}
