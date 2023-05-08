using Dml.Model.Back;
using Dml.Model.Template;
using DocumentMakerModelLibrary.Controls;
using System.Linq;

namespace DocumentMakerModelLibrary.OfficeFiles
{
	internal class DocumentTableRowData
	{
		private readonly DocumentTemplateType templateType;
		private readonly uint id;
		private readonly BackType type;
		private readonly string episodeNumberText;
		private readonly string backNumberText;
		private readonly string backName;
		private readonly string backCountRegionsText;
		private readonly string gameName;
		private readonly bool isRework;
		private readonly bool isSketch;
		private readonly bool haveEpisodes;
		private readonly string spentTimeText;
		private readonly string otherText;
		private readonly string workText;

		public DocumentTableRowData(FullBackDataModel model, DocumentTemplateType templateType)
		{
			this.templateType = templateType;
			id = model.Id;
			type = model.Type;
			episodeNumberText = model.EpisodeNumberText;
			backNumberText = model.BackNumberText;
			backName = model.BackName;
			backCountRegionsText = model.BackCountRegionsText;
			gameName = model.GameName;
			isRework = model.IsRework;
			isSketch = model.IsSketch;
			haveEpisodes = model.GameNameList.FirstOrDefault(x => x.Name == model.GameName)?.HaveEpisodes ?? !string.IsNullOrEmpty(model.EpisodeNumberText);
			spentTimeText = model.SpentTimeText;
			otherText = model.OtherText;
			workText = model.WorkTypesList.Count > 0 ? model.WorkTypesList[(int)(model.WorkObjectId % model.WorkTypesList.Count)]?.Name : null;
			SumText = model.SumText;
		}

		public DocumentTableRowData(FullBackDataModel model, DocumentMakerModel docModel, uint id)
		{
			this.id = id;
			type = BackType.Other;
			if(model.IsRework)
				otherText = "Послуги підтримки ";
			else
				otherText = "Послуги розробки ";

			switch(docModel.TemplateType)
			{
				case DocumentTemplateType.Scripter: otherText += "програмного продукту, "; break;
				case DocumentTemplateType.Cutter: otherText += "програмного продукту, "; break;
				case DocumentTemplateType.Painter: otherText += "графічного матеріалу, "; break;
				case DocumentTemplateType.Modeller: otherText += "програмного продукту, "; break;
				case DocumentTemplateType.Tester: otherText += "програмного продукту, "; break;
				case DocumentTemplateType.Programmer: otherText += "програмного продукту, "; break;
				case DocumentTemplateType.Soundman: otherText += "програмного продукту, "; break;
				case DocumentTemplateType.Animator: otherText += "графічного та відео матеріалу, "; break;
				case DocumentTemplateType.Translator: otherText += "програмного продукту, "; break;
				case DocumentTemplateType.Support: otherText += "програмного продукту, "; break;
			}

			if (model.IsRework)
				otherText += "зг.дог. №" + docModel.ContractReworkNumberText + " ";
			else
				otherText += "зг.дог. №" + docModel.ContractNumberText + " ";

			string[] dateMonth;
			if (model.IsRework)
				dateMonth = docModel.ContractReworkDateText.Split(' ');
			else
				dateMonth = docModel.ContractDateText.Split(' ');

			if(dateMonth.Length == 4)
			{
				string month;
				switch (dateMonth[1])
				{
					case "січня": month = "01"; break;
					case "лютого": month = "02"; break;
					case "березня": month = "03"; break;
					case "квітня": month = "04"; break;
					case "травня": month = "05"; break;
					case "червня": month = "06"; break;
					case "липня": month = "07"; break;
					case "серпня": month = "08"; break;
					case "вересня": month = "09"; break;
					case "жовтня": month = "10"; break;
					case "листопада": month = "11"; break;
					case "грудня": month = "12"; break;
					default: month = "Невірний номер"; break;
				}

				otherText += "від "+ dateMonth[0] + "." + month + "." + dateMonth[2] + "р";
			}
			else
				otherText += "від 111.111.1111р";

			SumText = model.SumText;
		}

		public string BackDataId => id.ToString();
		public string BackDataText => GenerateBackTask();
		public string SumText { get; }

		private string GenerateBackTask()
		{
			if (type == BackType.Other)
			{
				return otherText;
			}

			string regs = BackTaskStrings.GetRegionString(type, backCountRegionsText);
			return BackTaskStrings.Generate(type, templateType, workText, episodeNumberText, backNumberText, backName, regs, gameName, isRework, isSketch, haveEpisodes);
		}

		public string GetSpentTime()
		{
			return spentTimeText;
		}
	}
}
