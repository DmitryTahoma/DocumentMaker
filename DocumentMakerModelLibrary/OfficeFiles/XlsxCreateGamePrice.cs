using Dml;
using Dml.Model.Back;
using Dml.Model.Template;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentMakerModelLibrary.Algorithm;
using DocumentMakerModelLibrary.Files;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using MessageBox = System.Windows.Forms.MessageBox;
using System.Xml;
using System.Linq;

namespace DocumentMakerModelLibrary.OfficeFiles
{
	public class XlsxCreateGamePrice
	{
		int rowGame = 1;
		int rowGameCurr = 1;
		int colGameMax = 4;
		int colGameCurr = 4;

		int rowHumanCurr = 1;
		int rowHumanMax = 2;

		int colHuman = 1;
		int colDate = 1;
		int colSumma = 1;
		int colGame = 1;

		int rowValueMax = 1;

		int iDevelopmentSum = 0;
		int iReworkSum = 0;
		int iAllSum = 0;
		List<string> NameUser;
		XmlDocument xml;

		public XlsxCreateGamePrice()
		{
			NameUser = new List<string>();
			xml = new XmlDocument();
		}

		public void CreateXlsxXml(string path, IList<DmxFile> openedFilesList, IList<GameObject> gameObjects, Dictionary<string, int> dFillSheet)
		{
			CreateFileXlsx(path, dFillSheet);

			if (!File.Exists(path))
				return;

			try
			{
				foreach (DmxFile file in openedFilesList)
				{
					if (!NameUser.Contains(file.SelectedHuman))
						NameUser.Add(file.SelectedHuman);
				}

				NameUser.Sort(new NaturalStringComparer());
				var sw = new System.Diagnostics.Stopwatch();
				sw.Start();
				using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(path, true))
				{
					WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart;
					List <Sheet> sheets = new List<Sheet>(workbookPart.Workbook.Descendants<Sheet>());
					Sheet sheet = null;
					WorkType typeWork = WorkType.Empty;
					foreach (KeyValuePair<string, int> name in dFillSheet)
					{
						if (name.Value != -1)
						{
							FillSheetType typeSheet = (FillSheetType)name.Value;

							if (name.Key == "Розробка")
								typeWork = WorkType.Development;
							else if (name.Key == "Пiдтримка")
								typeWork = WorkType.Rework;
							else if (name.Key == "Разом")
								typeWork = WorkType.All;

							foreach (var item in sheets)
							{
								if (string.Compare(item.Name, name.Key, StringComparison.InvariantCultureIgnoreCase) == 0)
								{
									sheet = item;
									break;
								}
							}

							if (sheet != null)
							{
								WorksheetPart worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id);
								SheetData sheetData = worksheetPart.Worksheet.Elements<SheetData>().First();
								Columns columns = null;
								SharedStringTable sharedStringTable = null;
								if (workbookPart.SharedStringTablePart != null)
								{
									SharedStringTablePart sharedStringTablePart = workbookPart.GetPartsOfType<SharedStringTablePart>().First();
									sharedStringTable = sharedStringTablePart.SharedStringTable;
								}

								foreach(var item in worksheetPart.Worksheet.Elements())
								{
									if(item is Columns)
									{
										columns = item as Columns;
										break;
									}
								}
								//if(worksheetPart.Worksheet.Elements<Columns>() != null)
								//{
								//	columns = worksheetPart.Worksheet.Elements<Columns>().First();
								//}

								ExcelExporter.SetData(sheetData, sharedStringTable, columns);

								if (typeSheet == FillSheetType.Table)
									FillSheetExcelTable(openedFilesList, gameObjects, typeWork);
								else if (typeSheet == FillSheetType.List)
									FillSheetExcelList(openedFilesList, gameObjects, typeWork);
								else if (typeSheet == FillSheetType.Line)
									FillSheetExcelLine(openedFilesList, gameObjects, typeWork);

								sheet = null;
							}
						}
					}

					spreadsheetDocument.Save();
					spreadsheetDocument.Close();
					
				}
				sw.Stop();
				MessageBox.Show("Файл збережено. " + sw.Elapsed.ToString(),
										"DocumentMaker | Export",
										MessageBoxButtons.OK,
										MessageBoxIcon.Information);
			}

			catch (Exception exc)
			{
				MessageBox.Show("Виникла непередбачена помилка під час експорту! Надішліть, будь ласка, скріншот помилки розробнику.\n" + exc.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally
			{
				GC.Collect();
				GC.WaitForPendingFinalizers();
				GC.Collect();
			}
		}

		private void FillSheetExcelTable(IList<DmxFile> openedFilesList, IList<GameObject> gameObjects, WorkType _workType)
		{
			rowGame = 0;
			colGameMax = 3;
			colGameCurr = 3;

			rowHumanCurr = 0;
			rowHumanMax = 1;

			rowValueMax = 0;

			colDate = 1;
			colHuman = 0;
			colGame = 0;
			colSumma = 1;

			ExcelExporter.SetCellValue(rowGame, colHuman, "ПIБ");
			ExcelExporter.SetCellValue(rowGame, colDate, "Дата акту");
			ExcelExporter.SetCellValue(rowGame, 2, "Не визначено");

			foreach (string name in NameUser)
			{
				foreach (DmxFile file in openedFilesList)
				{
					if (name == file.SelectedHuman)
					{
						Dictionary<string, int> games = new Dictionary<string, int>();
						CalculateGamePrice.GamePrice(ref games, file.BackDataModels, gameObjects, _workType);
						string cellValue = "";
						string cellActValue = "";

						for (int i = 1; i <= openedFilesList.Count; i++)
						{
							cellValue = ExcelExporter.GetValue(i, colHuman);
							cellActValue = ExcelExporter.GetValue(i, colDate);
							rowHumanCurr = i;
							if (cellValue == file.SelectedHuman)
							{
								if (cellActValue == file.ActDateText)
								{
									rowHumanCurr = i;
									break;
								}
								else
								{
									cellValue = "";
								}
							}
							else
							{
								cellValue = "";
							}
						}

						if (string.IsNullOrEmpty(cellValue))
						{
							ExcelExporter.SetCellValue(rowHumanMax, colHuman, file.SelectedHuman);
							ExcelExporter.SetCellValue(rowHumanMax, colDate, file.ActDateText);
							rowHumanCurr = rowHumanMax;
							rowHumanMax++;
						}

						FindGameExcelTable(rowHumanCurr, games);
					}
				}
			}

			for (int i = 0; i < colGameMax + 1; i++)
			{
				ExcelExporter.GetCell(rowGame, i).StyleIndex = 11;
				ExcelExporter.GetCell(rowHumanMax, i).StyleIndex = 6;
			}

			for (int i = 1; i < colGameMax + 1; i++)
			{
				ExcelExporter.SetColumnWidth(i, 19);
			}

			for (int i = 1; i < rowHumanMax; i++)
			{
				ExcelExporter.GetCell(i, colHuman).StyleIndex = 12;
				string formula = "=SUM(" + ExcelExporter.GetCellName(i, 2) + ":" + ExcelExporter.GetCellName(i, colGameMax - 1) + ")";
				ExcelExporter.SetCellValue(i, colGameMax, formula, true);
			}

			for (int i = rowGame + 1; i < rowHumanMax; i++)
			{
				for (int j = colHuman + 1; j < colGameMax + 1; j++)
				{
					ExcelExporter.GetCell(i, j).StyleIndex = 4;
				}
			}

			ExcelExporter.SetCellValue(rowHumanMax, colHuman, "Всього грн.");
			ExcelExporter.SetCellValue(rowGame, colGameMax, "Сума грн.");
			ExcelExporter.GetCell(rowHumanMax, colHuman).StyleIndex = 5;

			for (int i = 2; i < colGameMax + 1; i++)
			{
				string formula = "=SUM(" + ExcelExporter.GetCellName(1, i) + ":" + ExcelExporter.GetCellName(rowHumanMax - 1, i) + ")";
				ExcelExporter.SetCellValue(rowHumanMax, i, formula, true);
			}

			if (colGameMax > 0)
			{
				if (WorkType.Development == _workType && iDevelopmentSum == 0)
					int.TryParse(ExcelExporter.GetValue(rowHumanMax, colGameMax), out iDevelopmentSum);
				else if (WorkType.Rework == _workType && iReworkSum == 0)
					int.TryParse(ExcelExporter.GetValue(rowHumanMax, colGameMax), out iReworkSum);
			}

			for (int i = 1; i < rowHumanMax; i++)
			{
				if (WorkType.Development == _workType)
				{
					string cellValue = ExcelExporter.GetValue(i, colHuman);
					string cellActValue = ExcelExporter.GetValue(i, colDate);
					if (int.TryParse(ExcelExporter.GetValue(i, colGameMax), out int cellSummValue) && cellSummValue <= 0)
						ExcelExporter.GetCell(i, colHuman).StyleIndex = 13;
					else
					{
						foreach (DmxFile file in openedFilesList)
						{
							if (cellValue == file.SelectedHuman)
							{
								if (cellActValue == file.ActDateText)
								{
									Dictionary<string, int> games = new Dictionary<string, int>();
									CalculateGamePrice.GamePrice(ref games, file.BackDataModels, gameObjects, WorkType.Rework);
									if (games.Count > 0)
										ExcelExporter.GetCell(i, colHuman).StyleIndex = 13;

									break;
								}
							}
						}
					}
				}
			}

			CalculateSumma(openedFilesList, gameObjects);
			for (int i = 1; i < 4; i++)
			{
				ExcelExporter.GetCell(rowHumanMax + i, colHuman).StyleIndex = 5;
				ExcelExporter.GetCell(rowHumanMax + i, colHuman + 1).StyleIndex = 6;
			}

			ExcelExporter.SetCellValue(rowHumanMax + 1, 0, "Розробка грн.");
			ExcelExporter.SetCellValue(rowHumanMax + 1, 1, iDevelopmentSum.ToString());
			ExcelExporter.SetCellValue(rowHumanMax + 2, 0, "Пiдтримка грн.");
			ExcelExporter.SetCellValue(rowHumanMax + 2, 1, iReworkSum.ToString());
			ExcelExporter.SetCellValue(rowHumanMax + 3, 0, "Всього грн.");
			string formulaSum = "=SUM(" + ExcelExporter.GetCellName(rowHumanMax + 1, 1) + ":" + ExcelExporter.GetCellName(rowHumanMax + 2, 1) + ")";
			ExcelExporter.SetCellValue(rowHumanMax + 3, 1, formulaSum, true);

			ExcelExporter.AutoFitCell(colHuman);
		}

		private void FillSheetExcelList(IList<DmxFile> openedFilesList, IList<GameObject> gameObjects, WorkType _workType)
		{
			rowGame = 1;
			rowGameCurr = 1;
			rowHumanCurr = 0;
			rowValueMax = 0;

			colDate = 2;
			colHuman = 0;
			colGame = 0;
			colSumma = 1;

			foreach (string name in NameUser)
			{
				foreach (DmxFile file in openedFilesList)
				{
					if (name == file.SelectedHuman)
					{
						Dictionary<string, int> games = new Dictionary<string, int>();
						CalculateGamePrice.GamePrice(ref games, file.BackDataModels, gameObjects, _workType);
						if (games.Count > 0)
						{
							string cellValue = "";
							string cellActValue = "";
							for (int i = 0; i <= rowValueMax + 1; i++)
							{
								cellValue = ExcelExporter.GetValue(i, colHuman);
								cellActValue = ExcelExporter.GetValue(i, colDate);
								if (cellValue == file.SelectedHuman)
								{
									if (cellActValue == file.ActDateText)
									{
										rowHumanCurr = i;
										break;
									}
									else
									{
										cellActValue = "";
										cellValue = "";
										rowHumanCurr = rowValueMax + 1;
									}
								}
								else
								{
									cellValue = "";
									cellActValue = "";
								}
							}

							if (string.IsNullOrEmpty(cellValue))
							{
								ExcelExporter.SetCellValue(rowHumanCurr, colHuman, file.SelectedHuman);
								ExcelExporter.SetCellValue(rowHumanCurr, colDate, file.ActDateText);
							}

							rowGameCurr = rowHumanCurr + 1;
							FindGameExcelList(rowGameCurr, games);
							rowValueMax = rowGameCurr;
							if (string.IsNullOrEmpty(cellValue))
							{
								ExcelExporter.GetCell(rowHumanCurr, colHuman).StyleIndex = 9;
								ExcelExporter.GetCell(rowHumanCurr, colDate).StyleIndex = 4;
								ExcelExporter.GetCell(rowHumanCurr, colSumma).StyleIndex = 4;
							}

							string formula = "=SUM(" + ExcelExporter.GetCellName(rowHumanCurr + 1, colSumma) + ":" + ExcelExporter.GetCellName(rowGameCurr - 1, colSumma) + ")";
							ExcelExporter.SetCellValue(rowHumanCurr, colSumma, formula, true);
							rowHumanCurr = rowValueMax + 1;
						}
					}
				}
			}

			if (WorkType.Development == _workType)
			{
				for (int i = 0; i <= rowValueMax; i++)
				{
					string cellValue = ExcelExporter.GetValue(i, colHuman);

					if (!string.IsNullOrEmpty(cellValue))
					{
						string cellActValue;
						foreach (DmxFile file in openedFilesList)
						{
							if (cellValue == file.SelectedHuman)
							{
								cellActValue = ExcelExporter.GetValue(i, colDate);
								if (cellActValue == file.ActDateText)
								{
									Dictionary<string, int> games = new Dictionary<string, int>();
									CalculateGamePrice.GamePrice(ref games, file.BackDataModels, gameObjects, WorkType.Rework);
									if (games.Count > 0)
										ExcelExporter.GetCell(i, colHuman).StyleIndex = 10;

									break;
								}
							}
						}
					}

				}
			}

			CalculateSumma(openedFilesList, gameObjects);
			for (int i = 1; i < 4; i++)
			{
				ExcelExporter.GetCell(rowValueMax + i, colHuman).StyleIndex = 5;
				ExcelExporter.GetCell(rowValueMax + i, colSumma).StyleIndex = 6;
			}

			ExcelExporter.SetCellValue(rowValueMax + 1, 0, "Розробка грн.");
			ExcelExporter.SetCellValue(rowValueMax + 1, 1, iDevelopmentSum.ToString());
			ExcelExporter.SetCellValue(rowValueMax + 2, 0, "Пiдтримка грн.");
			ExcelExporter.SetCellValue(rowValueMax + 2, 1, iReworkSum.ToString());
			ExcelExporter.SetCellValue(rowValueMax + 3, 0, "Всього грн.");
			string formulaSum = "=SUM(" + ExcelExporter.GetCellName(rowValueMax + 1, colSumma) + ":" + ExcelExporter.GetCellName(rowValueMax + 2, colSumma) + ")";
			ExcelExporter.SetCellValue(rowValueMax + 3, 1, formulaSum, true);

			ExcelExporter.AutoFitCell(colDate);
			ExcelExporter.AutoFitCell(colHuman);
			ExcelExporter.AutoFitCell(colGame);
		}

		private void FillSheetExcelLine(IList<DmxFile> openedFilesList, IList<GameObject> gameObjects, WorkType _workType)
		{
			rowGame = 0;
			rowGameCurr = 0;
			rowHumanCurr = 0;
			rowValueMax = 0;

			colDate = 0;
			colHuman = 1;
			colGame = 2;
			colSumma = 3;

			foreach (string name in NameUser)
			{
				foreach (DmxFile file in openedFilesList)
				{
					if (name == file.SelectedHuman)
					{
						Dictionary<string, int> games = new Dictionary<string, int>();
						CalculateGamePrice.GamePrice(ref games, file.BackDataModels, gameObjects, _workType);
						if (games.Count > 0)
						{
							string cellValue = "";
							string cellActValue = "";
							string cellGameValue = "";

							foreach (var game in games)
							{
								if (game.Value > 0)
								{
									bool _bGameFind = false;
									for (int i = 0; i <= rowValueMax; i++)
									{
										cellValue = ExcelExporter.GetValue(i, colHuman);
										cellActValue = ExcelExporter.GetValue(i, colDate);
										cellGameValue = ExcelExporter.GetValue(i, colGame);
										if (cellValue == file.SelectedHuman)
										{
											if (cellActValue == file.ActDateText)
											{
												if (cellGameValue == game.Key)
												{
													int _iSummGame = game.Value;
													if (int.TryParse(ExcelExporter.GetValue(i, colSumma), out int _iSummCell))
														_iSummGame += _iSummCell;

													ExcelExporter.SetCellValue(i, colSumma, _iSummGame.ToString());
													_bGameFind = true;
													break;
												}
											}
										}
									}

									if (!_bGameFind)
									{
										ExcelExporter.SetCellValue(rowValueMax, colHuman, file.SelectedHuman);
										ExcelExporter.SetCellValue(rowValueMax, colDate, file.ActDateText);
										ExcelExporter.SetCellValue(rowValueMax, colGame, game.Key);
										ExcelExporter.SetCellValue(rowValueMax, colSumma, game.Value.ToString());
										ExcelExporter.GetCell(rowValueMax, colDate).StyleIndex = 3;
										ExcelExporter.GetCell(rowValueMax, colHuman).StyleIndex = 3;
										ExcelExporter.GetCell(rowValueMax, colGame).StyleIndex = 3;
										ExcelExporter.GetCell(rowValueMax, colSumma).StyleIndex = 4;
										rowValueMax++;
									}
								}
							}
						}
					}
				}
			}

			CalculateSumma(openedFilesList, gameObjects);
			for (int i = 0; i < 3; i++)
			{
				ExcelExporter.GetCell(i, colSumma + 2).StyleIndex = 5;
				ExcelExporter.GetCell(i, colSumma + 3).StyleIndex = 6;
			}

			ExcelExporter.SetCellValue(0, colSumma + 2, "Розробка грн.");
			ExcelExporter.SetCellValue(0, colSumma + 3, iDevelopmentSum.ToString());
			ExcelExporter.SetCellValue(1, colSumma + 2, "Пiдтримка грн.");
			ExcelExporter.SetCellValue(1, colSumma + 3, iReworkSum.ToString());
			ExcelExporter.SetCellValue(2, colSumma + 2, "Всього грн.");
			string formulaSum = "=SUM(" + ExcelExporter.GetCellName(0, colSumma + 3) + ":" + ExcelExporter.GetCellName(1, colSumma + 3) + ")";
			ExcelExporter.SetCellValue(2, colSumma + 3, formulaSum, true);

			ExcelExporter.AutoFitCell(colDate);
			ExcelExporter.AutoFitCell(colHuman);
			ExcelExporter.AutoFitCell(colGame);
			ExcelExporter.AutoFitCell(colSumma);
			ExcelExporter.AutoFitCell(colSumma + 2);
			ExcelExporter.AutoFitCell(colSumma + 3);

			if (WorkType.Development == _workType)
			{
				for (int i = 0; i < rowValueMax; i++)
				{
					string cellValue = ExcelExporter.GetValue(i, colHuman);

					if (!string.IsNullOrEmpty(cellValue))
					{
						string cellActValue;
						foreach (DmxFile file in openedFilesList)
						{
							if (cellValue == file.SelectedHuman)
							{
								cellActValue = ExcelExporter.GetValue(i, colDate);
								if (cellActValue == file.ActDateText)
								{
									Dictionary<string, int> games = new Dictionary<string, int>();
									CalculateGamePrice.GamePrice(ref games, file.BackDataModels, gameObjects, WorkType.Rework);
									if (games.Count > 0)
										ExcelExporter.GetCell(i, colHuman).StyleIndex = 2;

									break;
								}
							}
						}
					}

				}
			}
		}

		private void FindGameExcelTable(int _rowHumanCurr, Dictionary<string, int> games)
		{
			foreach (var game in games)
			{
				for (int i = 1; i <= colGameMax; i++)
				{
					string cellValue = ExcelExporter.GetValue(rowGame, i);
					colGameCurr = i;
					if (string.IsNullOrEmpty(cellValue))
					{
						ExcelExporter.SetCellValue(rowGame, i, game.Key);
						ExcelExporter.SetCellValue(_rowHumanCurr, colGameCurr, game.Value.ToString());
						colGameMax++;

						break;
					}
					else if (cellValue == game.Key)
					{
						int _iSummGame = game.Value;
						if (int.TryParse(ExcelExporter.GetValue(_rowHumanCurr, colGameCurr), out int _iSummCell))
							_iSummGame += _iSummCell;
						ExcelExporter.SetCellValue(_rowHumanCurr, colGameCurr, _iSummGame.ToString());
						break;
					}
				}
			}
		}

		private void FindGameExcelList(int _rowGameCurr, Dictionary<string, int> games)
		{
			int _rowStartGameCurr = _rowGameCurr;
			string cellValue;

			for (int i = _rowGameCurr; i <= 100; i++)
			{
				cellValue = ExcelExporter.GetValue(i, colGame);
				rowGameCurr = i;
				if (string.IsNullOrEmpty(cellValue))
				{
					_rowGameCurr = rowGameCurr;
					break;
				}
			}

			foreach (var game in games)
			{
				bool _bGameFind = false;
				for (int i = _rowStartGameCurr; i <= _rowGameCurr - 1; i++)
				{
					cellValue = ExcelExporter.GetValue(i, colGame);
					if (cellValue == game.Key)
					{
						int _iSummGame = game.Value;
						if (int.TryParse(ExcelExporter.GetValue(i, colSumma), out int _iSummCell))
							_iSummGame += _iSummCell;
						ExcelExporter.SetCellValue(i, colSumma, _iSummGame.ToString());

						_bGameFind = true;
						break;
					}
				}

				if (!_bGameFind)
				{
					ExcelExporter.SetCellValue(rowGameCurr, colGame, game.Key);
					ExcelExporter.SetCellValue(rowGameCurr, colSumma, game.Value.ToString());
					ExcelExporter.GetCell(rowGameCurr, colGame).StyleIndex = 8;
					ExcelExporter.GetCell(rowGameCurr, colSumma).StyleIndex = 4;
					rowGameCurr++;
				}
			}
		}

		private void CreateFileXlsx(string path, Dictionary<string, int> dFillSheet)
		{
			//if (!File.Exists(path))
			{
				SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Create(path, SpreadsheetDocumentType.Workbook);
				WorkbookPart workbookpart = spreadsheetDocument.AddWorkbookPart();
				workbookpart.Workbook = new Workbook();

				WorkbookStylesPart stylesPart = workbookpart.AddNewPart<WorkbookStylesPart>();
				stylesPart.Stylesheet = CreateStyleSheet();
				stylesPart.Stylesheet.Save();

				Sheets sheets = spreadsheetDocument.WorkbookPart.Workbook.AppendChild(new Sheets());

				uint index = 1;
				foreach (KeyValuePair<string, int> name in dFillSheet)
				{
					if (name.Value != -1)
					{
						WorksheetPart worksheetPart = workbookpart.AddNewPart<WorksheetPart>();
						worksheetPart.Worksheet = new Worksheet();
						SheetData sheetData = new SheetData();
	
						Columns columns = new Columns();
						double maxWidth = 7;
						for (int i = 0; i < 702; i++)
						{
							Column col = new Column() { BestFit = true, Min = (uint)(i + 1), Max = (uint)(i + 1), CustomWidth = true, Width = maxWidth };
							columns.Append(col);
						}
						worksheetPart.Worksheet.Append(columns);
						worksheetPart.Worksheet.Append(sheetData);
						sheets.Append(new Sheet()
						{
							Id = spreadsheetDocument.WorkbookPart.GetIdOfPart(worksheetPart),
							SheetId = index,
							Name = name.Key
						});
						index++;
					}
				}
				workbookpart.Workbook.Save();
				spreadsheetDocument.Close();
			}
		}

		private ForegroundColor TranslateForeground(System.Drawing.Color fillColor)
		{
			return new ForegroundColor()
			{
				Rgb = new HexBinaryValue()
				{
					Value =
							  System.Drawing.ColorTranslator.ToHtml(
							  System.Drawing.Color.FromArgb(
								  fillColor.A,
								  fillColor.R,
								  fillColor.G,
								  fillColor.B)).Replace("#", "")
				}
			};
		}

		private Stylesheet CreateStyleSheet()
		{
			Stylesheet stylesheet = new Stylesheet();
			#region Number format
			var numberingFormats = new NumberingFormats();
			numberingFormats.Append(new NumberingFormat
			{
				NumberFormatId = 0,
				FormatCode = StringValue.FromString("")
			});
			numberingFormats.Count = UInt32Value.FromUInt32((uint)numberingFormats.ChildElements.Count);
			#endregion

			#region Fonts
			var fonts = new Fonts();
			fonts.Append(new Font()  // Font index 0 - default
			{
				FontName = new FontName { Val = StringValue.FromString("Calibri") },
				FontSize = new FontSize { Val = DoubleValue.FromDouble(12) }
			});
			fonts.Append(new Font()  // Font index 1
			{
				FontName = new FontName { Val = StringValue.FromString("Calibri") },
				FontSize = new FontSize { Val = DoubleValue.FromDouble(12) },
				Bold = new Bold()
			});
			fonts.Count = UInt32Value.FromUInt32((uint)fonts.ChildElements.Count);
			#endregion

			#region Fills
			var fills = new Fills();
			fills.Append(new Fill() // Fill index 0
			{
				PatternFill = new PatternFill { PatternType = PatternValues.None }
			});
			fills.Append(new Fill() // Fill index 1
			{
				PatternFill = new PatternFill { PatternType = PatternValues.Gray125 }
			});
			fills.Append(new Fill() // Fill index 2
			{
				PatternFill = new PatternFill
				{
					PatternType = PatternValues.Solid,
					ForegroundColor = TranslateForeground(System.Drawing.Color.Yellow),
					BackgroundColor = new BackgroundColor { Rgb = TranslateForeground(System.Drawing.Color.Yellow).Rgb }
				}
			});
			fills.Count = UInt32Value.FromUInt32((uint)fills.ChildElements.Count);
			#endregion

			#region Borders
			var borders = new Borders();
			borders.Append(new Border   // Border index 0: no border
			{
				LeftBorder = new LeftBorder(),
				RightBorder = new RightBorder(),
				TopBorder = new TopBorder(),
				BottomBorder = new BottomBorder(),
				DiagonalBorder = new DiagonalBorder()
			});
			borders.Append(new Border    //Boarder Index 1: All
			{
				LeftBorder = new LeftBorder { Style = BorderStyleValues.Thin },
				RightBorder = new RightBorder { Style = BorderStyleValues.Thin },
				TopBorder = new TopBorder { Style = BorderStyleValues.Thin },
				BottomBorder = new BottomBorder { Style = BorderStyleValues.Thin },
				DiagonalBorder = new DiagonalBorder()
			});
			borders.Append(new Border    //Boarder Index 2: All
			{
				LeftBorder = new LeftBorder { Style = BorderStyleValues.Medium },
				RightBorder = new RightBorder { Style = BorderStyleValues.Medium },
				TopBorder = new TopBorder { Style = BorderStyleValues.Medium },
				BottomBorder = new BottomBorder { Style = BorderStyleValues.Medium },
				DiagonalBorder = new DiagonalBorder()
			});
			borders.Count = UInt32Value.FromUInt32((uint)borders.ChildElements.Count);
			#endregion

			#region Cell Style Format
			var cellStyleFormats = new CellStyleFormats();
			cellStyleFormats.Append(new CellFormat  // Cell style format index 0: no format
			{
				NumberFormatId = 0,
				FontId = 0,
				FillId = 0,
				BorderId = 0,
				FormatId = 0
			});
			cellStyleFormats.Append(new CellFormat  // Cell style format index 1: no format
			{
				NumberFormatId = 0,
				FontId = 0,
				FillId = 0,
				BorderId = 0,
				FormatId = 0,
				Alignment = new Alignment() { Horizontal = HorizontalAlignmentValues.Center, Vertical = VerticalAlignmentValues.Center }
			});
			cellStyleFormats.Append(new CellFormat  // Cell style format index 2: no format
			{
				NumberFormatId = 0,
				FontId = 0,
				FillId = 0,
				BorderId = 0,
				FormatId = 0,
				Alignment = new Alignment() { Horizontal = HorizontalAlignmentValues.Left, Vertical = VerticalAlignmentValues.Center }
			});
			cellStyleFormats.Append(new CellFormat  // Cell style format index 2: no format
			{
				NumberFormatId = 0,
				FontId = 0,
				FillId = 0,
				BorderId = 0,
				FormatId = 0,
				Alignment = new Alignment() { Horizontal = HorizontalAlignmentValues.Right, Vertical = VerticalAlignmentValues.Center }
			});
			cellStyleFormats.Count = UInt32Value.FromUInt32((uint)cellStyleFormats.ChildElements.Count);
			#endregion

			#region Cell format
			var cellFormats = new CellFormats();
			cellFormats.Append(new CellFormat());    // Cell format index 0
			cellFormats.Append(new CellFormat   // Cell format index 1: Cell header
			{
				NumberFormatId = 0,
				FontId = 0,
				FillId = 2,//Color
				BorderId = 1,
				FormatId = 0,

				Alignment = new Alignment() { Horizontal = HorizontalAlignmentValues.Right, Vertical = VerticalAlignmentValues.Center }
			});
			cellFormats.Append(new CellFormat   // Cell format index 2: Cell header
			{
				NumberFormatId = 0,
				FontId = 0,
				FillId = 2,//Color
				BorderId = 1,
				FormatId = 0,

				Alignment = new Alignment() { Horizontal = HorizontalAlignmentValues.Left, Vertical = VerticalAlignmentValues.Center }
			});
			cellFormats.Append(new CellFormat   // Cell format index 3: Cell header
			{
				NumberFormatId = 0,
				FontId = 0,
				FillId = 0,
				BorderId = 1,
				FormatId = 0,

				Alignment = new Alignment() { Horizontal = HorizontalAlignmentValues.Left, Vertical = VerticalAlignmentValues.Center }
			});
			cellFormats.Append(new CellFormat   // Cell format index 4: Cell header
			{
				NumberFormatId = 0,
				FontId = 0,
				FillId = 0,
				BorderId = 1,
				FormatId = 0,

				Alignment = new Alignment() { Horizontal = HorizontalAlignmentValues.Right, Vertical = VerticalAlignmentValues.Center }
			});
			cellFormats.Append(new CellFormat   // Cell format index 5: Cell header
			{
				NumberFormatId = 0,
				FontId = 1,
				FillId = 0,
				BorderId = 2,
				FormatId = 0,

				Alignment = new Alignment() { Horizontal = HorizontalAlignmentValues.Right, Vertical = VerticalAlignmentValues.Center }
			});
			cellFormats.Append(new CellFormat   // Cell format index 6: Cell header
			{
				NumberFormatId = 0,
				FontId = 0,
				FillId = 0,
				BorderId = 2,
				FormatId = 0,

				Alignment = new Alignment() { Horizontal = HorizontalAlignmentValues.Right, Vertical = VerticalAlignmentValues.Center }
			});
			cellFormats.Append(new CellFormat   // Cell format index 7: Cell header
			{
				NumberFormatId = 0,
				FontId = 0,
				FillId = 0,
				BorderId = 2,
				FormatId = 0,

				Alignment = new Alignment() { Horizontal = HorizontalAlignmentValues.Left, Vertical = VerticalAlignmentValues.Center }
			});
			cellFormats.Append(new CellFormat   // Cell format index 8: Cell header
			{
				NumberFormatId = 0,
				FontId = 0,
				FillId = 0,
				BorderId = 1,
				FormatId = 0,

				Alignment = new Alignment() { Horizontal = HorizontalAlignmentValues.Center, Vertical = VerticalAlignmentValues.Center }
			});
			cellFormats.Append(new CellFormat   // Cell format index 9: Cell header
			{
				NumberFormatId = 0,
				FontId = 1,
				FillId = 0,
				BorderId = 2,
				FormatId = 0,

				Alignment = new Alignment() { Horizontal = HorizontalAlignmentValues.Left, Vertical = VerticalAlignmentValues.Center }
			});
			cellFormats.Append(new CellFormat   // Cell format index 10: Cell header
			{
				NumberFormatId = 0,
				FontId = 1,
				FillId = 2,
				BorderId = 2,
				FormatId = 0,

				Alignment = new Alignment() { Horizontal = HorizontalAlignmentValues.Left, Vertical = VerticalAlignmentValues.Center }
			});
			cellFormats.Append(new CellFormat   // Cell format index 11: Cell header
			{
				NumberFormatId = 0,
				FontId = 1,
				FillId = 0,
				BorderId = 2,
				FormatId = 0,

				Alignment = new Alignment() { Horizontal = HorizontalAlignmentValues.Center, Vertical = VerticalAlignmentValues.Center }
			});
			cellFormats.Append(new CellFormat   // Cell format index 12: Cell header
			{
				NumberFormatId = 0,
				FontId = 0,
				FillId = 0,
				BorderId = 2,
				FormatId = 0,

				Alignment = new Alignment() { Horizontal = HorizontalAlignmentValues.Left, Vertical = VerticalAlignmentValues.Center }
			});
			cellFormats.Append(new CellFormat   // Cell format index 13: Cell header
			{
				NumberFormatId = 0,
				FontId = 0,
				FillId = 2,//Color
				BorderId = 2,
				FormatId = 0,

				Alignment = new Alignment() { Horizontal = HorizontalAlignmentValues.Left, Vertical = VerticalAlignmentValues.Center }
			});
			cellFormats.Count = UInt32Value.FromUInt32((uint)cellFormats.ChildElements.Count);
			#endregion

			stylesheet.Append(numberingFormats);
			stylesheet.Append(fonts);
			stylesheet.Append(fills);
			stylesheet.Append(borders);
			stylesheet.Append(cellStyleFormats);
			stylesheet.Append(cellFormats);

			#region Cell styles
			var css = new CellStyles();
			css.Append(new CellStyle
			{
				Name = StringValue.FromString("Normal"),
				FormatId = 0,
				BuiltinId = 0
			});
			css.Count = UInt32Value.FromUInt32((uint)css.ChildElements.Count);
			stylesheet.Append(css);
			#endregion

			var dfs = new DifferentialFormats { Count = 0 };
			stylesheet.Append(dfs);
			var tss = new TableStyles
			{
				Count = 0,
				DefaultTableStyle = StringValue.FromString("TableStyleMedium9"),
				DefaultPivotStyle = StringValue.FromString("PivotStyleLight16")
			};
			stylesheet.Append(tss);

			return stylesheet;
		}

		private void CalculateSumma(IList<DmxFile> openedFilesList, IList<GameObject> gameObjects)
		{
			bool bCalcDevelopment = iDevelopmentSum == 0;
			bool bCalcRework = iReworkSum == 0;

			foreach (DmxFile file in openedFilesList)
			{
				if (bCalcDevelopment)
				{
					CalculateDevelopmentSumma(file, gameObjects);
				}

				if (bCalcRework)
				{
					CalculateReworkSumma(file, gameObjects);
				}
			}

			iAllSum = iDevelopmentSum + iReworkSum;
		}

		private void CalculateDevelopmentSumma(DmxFile file, IList<GameObject> gameObjects)
		{
			Dictionary<string, int> games = new Dictionary<string, int>();
			CalculateGamePrice.GamePrice(ref games, file.BackDataModels, gameObjects, WorkType.Development);
			foreach (var game in games)
			{
				iDevelopmentSum += game.Value;
			}
		}

		private void CalculateReworkSumma(DmxFile file, IList<GameObject> gameObjects)
		{
			Dictionary<string, int> games = new Dictionary<string, int>();
			CalculateGamePrice.GamePrice(ref games, file.BackDataModels, gameObjects, WorkType.Rework);

			foreach (var game in games)
			{
				iReworkSum += game.Value;
			}
		}

	}
}
