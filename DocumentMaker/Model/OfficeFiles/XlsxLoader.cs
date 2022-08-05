using Dml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentMaker.Model.OfficeFiles.Human;
using System.Linq;
using System.Xml;

namespace DocumentMaker.Model.OfficeFiles
{
	class XlsxLoader
	{
		public void LoadHumans(string path, ObservableRangeCollection<HumanData> humansData)
		{
			humansData.Clear();

			const char startColId = 'A';
			const int startRowId = 2;

			using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(path, false))
			{
				WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart;

				SharedStringTablePart sharedStringTablePart = workbookPart.GetPartsOfType<SharedStringTablePart>().First();
				SharedStringTable sharedStringTable = sharedStringTablePart.SharedStringTable;

				WorksheetPart worksheetPart = workbookPart.WorksheetParts.First();
				SheetData sheetData = worksheetPart.Worksheet.Elements<SheetData>().First();

				int curRowId = 1;
				foreach (Row r in sheetData.Elements<Row>())
				{
					if (curRowId >= startRowId)
					{
						HumanData humanData = new HumanData();

						char curColId = startColId;
						foreach (Cell c in r.Elements<Cell>())
						{
							string text;
							if (c.DataType != null && c.DataType == CellValues.SharedString && int.TryParse(c.CellValue.Text, out int strId))
							{
								text = sharedStringTable.ChildElements[strId].InnerText;
							}
							else
							{
								text = c.CellValue.Text;
							}
							humanData.SetData(curColId, text);
							++curColId;
						}

						humansData.Add(humanData);
					}

					++curRowId;
				}

				spreadsheetDocument.Close();
			}
		}
	}
}
