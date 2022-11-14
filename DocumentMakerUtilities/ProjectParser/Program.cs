using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace ProjectParser
{
	class Program
	{
		static string sourceFilename => "Source.xlsx";

		static Regex backNameRegex = new Regex(@"^\d+\.\s*\w+");
		static Regex hogNameRegex = new Regex(@"^[\d+\.]*\s*\w+");

		static void Main(string[] args)
		{
			if (!File.Exists(sourceFilename)) return;
			Maximize();

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

							Console.ForegroundColor = ConsoleColor.White;
							bool isAppropriate = false;
							bool isNotNull = false;

							int curColIdNum = 0;
							foreach (Cell c in r.Elements<Cell>())
							{
								if (!c.CellReference.HasValue || c.CellReference.Value != curColId + curRowId.ToString())
								{
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
									isNotNull = true;

									if (curColIdNum == 0)
									{
										isAppropriate = backNameRegex.IsMatch(text);
										if (isAppropriate) Console.ForegroundColor = ConsoleColor.Blue;
										Console.Write(text);
									}
									else if(curColIdNum == 1)
									{
										Console.CursorLeft = 50;
										if(float.TryParse(text.Replace('.',','), out float countRegions))
										{
											Console.Write(((uint)countRegions).ToString());
										}
										else
										{
											Console.ForegroundColor = ConsoleColor.Red;
											Console.Write("err reg");
										}
									}
									else if(curColIdNum == 2)
									{
										Console.CursorLeft = 60;
										if(hogNameRegex.IsMatch(text))
										{
											Console.ForegroundColor = ConsoleColor.Blue;
											Console.Write(text.Replace('\n', ';'));
										}
										else
										{
											Console.ForegroundColor = ConsoleColor.Red;
											Console.Write("err hog");
										}
									}
									else if(curColIdNum == 3)
									{
										Console.CursorLeft = 120;
										if (float.TryParse(text.Replace('.', ','), out float hogCountRegions))
										{
											Console.ForegroundColor = ConsoleColor.Blue;
											Console.Write(((uint)hogCountRegions).ToString());
										}
										else
										{
											Console.ForegroundColor = ConsoleColor.Red;
											Console.Write("err reg");
										}
									}
									else if(curColIdNum == 4)
									{
										Console.CursorLeft = 130;
										Console.ForegroundColor = ConsoleColor.Blue;
										Console.Write(text.Replace('\n', ';'));
									}
									else if(curColIdNum == 5)
									{
										if(Console.CursorLeft < 130) Console.CursorLeft = 130;
										Console.ForegroundColor = ConsoleColor.Blue;
										Console.Write(" | " + text.Replace('\n', ';'));
									}
								}
								else if(curColIdNum == 1)
								{
									Console.CursorLeft = 50;
									Console.ForegroundColor = ConsoleColor.Red;
									Console.Write("err reg");
								}
								else if(curColIdNum == 2)
								{
									Console.CursorLeft = 60;
									Console.ForegroundColor = ConsoleColor.Red;
									Console.Write("err hog");
								}
								else if (curColIdNum == 3)
								{
									Console.CursorLeft = 120;
									Console.ForegroundColor = ConsoleColor.Red;
									Console.Write("err reg");
								}
								++curColId;
								++curColIdNum;

								if (!isAppropriate)
								{
									Console.ForegroundColor = ConsoleColor.White;
									break;
								}
							}
							if(isNotNull)
							{
								Console.WriteLine();
							}

							++curRowId;
						}
					}
				}

				spreadsheetDocument.Close();
			}

			Console.ReadLine();
		}

		[DllImport("user32.dll")]
		public static extern bool ShowWindow(IntPtr hWnd, int cmdShow);

		private static void Maximize()
		{
			Process p = Process.GetCurrentProcess();
			ShowWindow(p.MainWindowHandle, 3); //SW_MAXIMIZE = 3
		}
	}
}
