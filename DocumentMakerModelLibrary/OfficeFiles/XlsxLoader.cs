using Dml;
using Dml.Model.Template;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentMakerModelLibrary.Back;
using DocumentMakerModelLibrary.OfficeFiles.Human;
using System.IO;

namespace DocumentMakerModelLibrary.OfficeFiles
{
	public class XlsxLoader
	{
		public void LoadHumans(string path, ObservableRangeCollection<HumanData> humansData)
		{
			if (!File.Exists(path)) return;

			humansData.Clear();

			const int startRowId = 2;
			ExcelExporter.OpenXlsx(path, false);

			if (ExcelExporter.LoadSheet("Лист1"))
			{
				foreach (Row r in ExcelExporter.GetRows())
				{
					if (r.RowIndex >= startRowId)
					{
						HumanData humanData = new HumanData();

						foreach (Cell c in ExcelExporter.GetCells(r))
						{
							string text = ExcelExporter.GetValue(c);
							humanData.SetData(ExcelExporter.GetCellLetter(c.CellReference.ToString()), text);
						}

						humansData.Add(humanData);
					}
				}
			}

			ExcelExporter.CloseFile();
		}

		public void LoadWorkTypes(string path, DocumentTemplateType templateType, ObservableRangeCollection<WorkObject> workObjects)
		{
			if (!File.Exists(path)) return;

			workObjects.Clear();

			const int startRowId = 2;

			ExcelExporter.OpenXlsx(path, false);

			if (ExcelExporter.LoadSheet("Лист1"))
			{
				uint id = 0;
				foreach (Row r in ExcelExporter.GetRows())
				{
					if (r.RowIndex >= startRowId)
					{
						string text = ExcelExporter.GetValue(r, (int)templateType);
						if (!string.IsNullOrEmpty(text))
						{
							workObjects.Add(new WorkObject { Id = id++, Name = text });
						}
					}
				}
			}

			ExcelExporter.CloseFile();
		}
	}
}
