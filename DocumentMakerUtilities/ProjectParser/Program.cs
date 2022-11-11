using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ProjectParser
{
	class Program
	{
		static string sourceFilename => "Source.xlsx";

		static Regex regex = new Regex(@"^\d+\.\s*\w+");

		static void Main(string[] args)
		{
			if (!File.Exists(sourceFilename)) return;

			using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(sourceFilename, false))
			{
				WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart;

				SharedStringTablePart sharedStringTablePart = workbookPart.GetPartsOfType<SharedStringTablePart>().First();
				SharedStringTable sharedStringTable = sharedStringTablePart.SharedStringTable;

				foreach (Sheet sheet in workbookPart.Workbook.Sheets)
				{
					Console.ForegroundColor = ConsoleColor.Green;
					Console.WriteLine(sheet.Name);
					Console.ForegroundColor = ConsoleColor.White;

					string relationshipId = sheet.Id.Value;
					WorksheetPart worksheetPart = (WorksheetPart)workbookPart.GetPartById(relationshipId);
				
					foreach (SheetData sheetData in worksheetPart.Worksheet.Elements<SheetData>())
					{
						int curRowId = 1;
						foreach (Row r in sheetData.Elements<Row>())
						{
							char curColId = 'A';
							foreach (Cell c in r.Elements<Cell>())
							{
								if (sheet.Name == "Escape")
								{
									++curColId;
									continue;
								}

								string text;
								if (c.DataType != null && c.DataType == CellValues.SharedString && int.TryParse(c.CellValue.Text, out int strId))
								{
									text = sharedStringTable.ChildElements[strId]?.InnerText;
								}
								else
								{
									text = c.CellValue?.Text;
								}
								if (!string.IsNullOrWhiteSpace(text))
								{
									bool isAppropriate = regex.IsMatch(text);
									if (isAppropriate) Console.ForegroundColor = ConsoleColor.Blue;
									Console.WriteLine(text);
									if (isAppropriate) Console.ForegroundColor = ConsoleColor.White;
									
								}
								++curColId;

								break;
							}

							++curRowId;
						}
					}
				}

				spreadsheetDocument.Close();
			}

			Console.ReadLine();
		}
	}
}
