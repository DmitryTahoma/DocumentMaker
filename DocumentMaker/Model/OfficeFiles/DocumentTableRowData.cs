using Dml.Model.Back;
using Dml.Model.Template;
using DocumentMaker.Model.Controls;

namespace DocumentMaker.Model.OfficeFiles
{
	internal class DocumentTableRowData
	{
		private readonly DocumentTemplateType templateType;
		private readonly uint id;
		private readonly BackType type;
		private readonly string backNumberText;
		private readonly string backName;
		private readonly string backCountRegionsText;
		private readonly string gameName;
		private readonly bool isRework;
		private readonly bool isSketch;
		private readonly string spentTimeText;
		private readonly string otherText;

		public DocumentTableRowData(FullBackDataModel model, DocumentTemplateType templateType)
		{
			this.templateType = templateType;
			id = model.Id;
			type = model.Type;
			backNumberText = model.BackNumberText;
			backName = model.BackName;
			backCountRegionsText = model.BackCountRegionsText;
			gameName = model.GameName;
			isRework = model.IsRework;
			isSketch = model.IsSketch;
			spentTimeText = model.SpentTimeText;
			otherText = model.OtherText;
		}

		public string BackDataId => id.ToString();
		public string BackDataText => GenerateBackTask();

		private string GenerateBackTask()
		{
			if (type == BackType.Other)
			{
				return otherText;
			}

			string regs = BackTaskStrings.GetRegionString(type, backCountRegionsText);
			return BackTaskStrings.Generate(type, templateType, backNumberText, backName, regs, gameName, isRework, isSketch);
		}

		public string GetSpentTime()
		{
			return spentTimeText;
		}
	}
}
