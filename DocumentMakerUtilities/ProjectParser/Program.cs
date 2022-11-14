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
							if (sheet.Name == "Escape")
							{
								++curColId;
								continue;
							}

							int curColIdNum = 0;
							foreach (Cell c in r.Elements<Cell>())
							{
								bool isAppropriate = false;

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
									//if(text.Contains("15.1 Фуллскр"))
									//{
									//	int k = -9;
									//}
									if (curColIdNum == 0)
									{
										isAppropriate = regex.IsMatch(text);
										if (isAppropriate) Console.ForegroundColor = ConsoleColor.Blue;
										Console.Write(text);
										if (!isAppropriate) Console.WriteLine();
									}
									else if(curColIdNum == 1)
									{
										Console.CursorLeft = 50;
										if(float.TryParse(text.Replace('.',','), out float countRegions))
										{
											Console.WriteLine(((uint)countRegions).ToString());
										}
										else
										{
											Console.ForegroundColor = ConsoleColor.Red;
											Console.WriteLine("err reg");
										}
									}
								}
								else if(curColIdNum == 1)
								{
									Console.CursorLeft = 50;
									Console.ForegroundColor = ConsoleColor.Red;
									Console.WriteLine("err reg");
								}
								++curColId;
								++curColIdNum;

								if (!isAppropriate)
								{
									Console.ForegroundColor = ConsoleColor.White;
									break;
								}
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
