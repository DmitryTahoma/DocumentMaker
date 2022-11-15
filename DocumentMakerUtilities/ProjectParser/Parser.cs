using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ProjectParser
{
	class Parser
	{
		static Regex backNameRegex = new Regex(@"^\d+\.\s*\w+", RegexOptions.Compiled);
		static Regex hogNameRegex = new Regex(@"^[\d+\.]*\s*\w+", RegexOptions.Compiled);

		public List<KeyValuePair<string, List<ParsedObj>>> ParseFromFile(string filePath)
		{
			List<KeyValuePair<string, List<ParsedObj>>> result = new List<KeyValuePair<string, List<ParsedObj>>>();

			using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(filePath, false))
			{
				WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart;

				SharedStringTablePart sharedStringTablePart = workbookPart.GetPartsOfType<SharedStringTablePart>().First();
				SharedStringTable sharedStringTable = sharedStringTablePart.SharedStringTable;

				foreach (Sheet sheet in workbookPart.Workbook.Sheets)
				{
					KeyValuePair<string, List<ParsedObj>> project = new KeyValuePair<string, List<ParsedObj>>(sheet.Name, new List<ParsedObj>());
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
							ParsedObj parsedObj = new ParsedObj();

							char curColId = 'A';
							if (sheet.Name == "Escape")
							{
								++curColId;
							}

							Console.ForegroundColor = ConsoleColor.White;
							bool isAppropriate = false;
							bool isNotNull = false;
							bool addParsedObj = false;

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
										if (isAppropriate)
										{
											addParsedObj = true;
											parsedObj.BackName = text;
											Console.ForegroundColor = ConsoleColor.Blue;
										}
										Console.Write(text);
									}
									else if (curColIdNum == 1)
									{
										Console.CursorLeft = 50;
										if (float.TryParse(text.Replace('.', ','), out float countRegions))
										{
											Console.Write(((uint)countRegions).ToString());
											parsedObj.BackRegions = (uint)countRegions;
										}
										else
										{
											Console.ForegroundColor = ConsoleColor.Red;
											Console.Write("err reg");
										}
									}
									else if (curColIdNum == 2)
									{
										Console.CursorLeft = 60;
										if (hogNameRegex.IsMatch(text))
										{
											Console.ForegroundColor = ConsoleColor.Blue;
											Console.Write(text.Replace('\n', ';'));
											parsedObj.HogName = text;
										}
										else
										{
											Console.ForegroundColor = ConsoleColor.Red;
											Console.Write("err hog");
										}
									}
									else if (curColIdNum == 3)
									{
										Console.CursorLeft = 120;
										if (float.TryParse(text.Replace('.', ','), out float hogCountRegions))
										{
											Console.ForegroundColor = ConsoleColor.Blue;
											Console.Write(((uint)hogCountRegions).ToString());
											parsedObj.HogRegions = (uint)hogCountRegions;
										}
										else
										{
											Console.ForegroundColor = ConsoleColor.Red;
											Console.Write("err reg");
										}
									}
									else if (curColIdNum == 4)
									{
										Console.CursorLeft = 130;
										Console.ForegroundColor = ConsoleColor.Blue;
										Console.Write(text.Replace('\n', ';'));
										parsedObj.Minigames = text;
									}
									else if (curColIdNum == 5)
									{
										if (Console.CursorLeft < 130) Console.CursorLeft = 130;
										Console.ForegroundColor = ConsoleColor.Blue;
										Console.Write(" | " + text.Replace('\n', ';'));
										parsedObj.Dialogs = text;
									}
								}
								else if (curColIdNum == 1)
								{
									Console.CursorLeft = 50;
									Console.ForegroundColor = ConsoleColor.Red;
									Console.Write("err reg");
								}
								else if (curColIdNum == 2)
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
							if (isNotNull)
							{
								Console.WriteLine();
							}
							if(addParsedObj)
							{
								project.Value.Add(parsedObj);
							}

							++curRowId;
						}
					}

					result.Add(project);
				}

				spreadsheetDocument.Close();
			}

			return result;
		}
	}
}
