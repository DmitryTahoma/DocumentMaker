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
using System.Runtime.InteropServices;
using System.Windows.Forms;
using MessageBox = System.Windows.Forms.MessageBox;
using Excel = Microsoft.Office.Interop.Excel;

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

		int rowValueMax = 1;

		public void Create(string path, IList<DmxFile> openedFilesList, IList<GameObject> gameObjects)
		{
			CreateXlsx(path);

			if (!File.Exists(path)) return;

			Excel.Workbooks workbooks = null;
			Excel.Workbook workbook = null;
			Excel.Application excelApp = null;
			Excel.Worksheet workSheet = null;

			try
			{
				string[] arr_EN = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };

				List<string> alphabetExcel = new List<string>(arr_EN);

				for (int i = 0; i < arr_EN.Length; i++)
				{
					for (int j = 0; j < arr_EN.Length; j++)
					{
						alphabetExcel.Add(arr_EN[i] + arr_EN[j]);
					}
				}

				excelApp = new Excel.Application();
				workbooks = excelApp.Workbooks;
				workbook = workbooks.Open(path);

				FillSheet(ref workSheet, excelApp, openedFilesList, gameObjects, alphabetExcel, 1, WorkType.Development);
				FillSheet2(ref workSheet, excelApp, openedFilesList, gameObjects, alphabetExcel, 2, WorkType.Rework);
				FillSheet(ref workSheet, excelApp, openedFilesList, gameObjects, alphabetExcel, 3, WorkType.All);

				excelApp.UserControl = true;
				foreach (Excel.Workbook book in excelApp.Workbooks)
				{
					book.Save();
					book.Close();
				}

				workbooks.Close();
				excelApp.Quit();

				MessageBox.Show("Файл збережен.",
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
				if (workSheet != null)
					Marshal.ReleaseComObject(workSheet);
				if (workbook != null)
					Marshal.ReleaseComObject(workbook);
				if (workbooks != null)
					Marshal.ReleaseComObject(workbooks);
				if (excelApp != null)
					Marshal.ReleaseComObject(excelApp);

				GC.Collect();
				GC.WaitForPendingFinalizers();
				GC.Collect();
			}
		}

		private void FillSheet(ref Excel.Worksheet workSheet, Excel.Application excelApp, IList<DmxFile> openedFilesList, IList<GameObject> gameObjects, List<string> alphabetExcel, int _indexSheet, WorkType _workType)
		{
			rowGame = 1;
			colGameMax = 4;
			colGameCurr = 4;

			rowHumanCurr = 1;
			rowHumanMax = 2;
			colHuman = 1;

			rowValueMax = 1;

			workSheet = (Excel.Worksheet)excelApp.Sheets[_indexSheet];

			((Excel.Range)workSheet.Columns[2]).Font.Name = "Calibri";
			((Excel.Range)workSheet.Columns[2]).Font.Size = 12;
			((Excel.Range)workSheet.Columns[2]).HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
			((Excel.Range)workSheet.Columns[2]).VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;

			workSheet.Cells[rowGame, 1] = "ПIБ";
			workSheet.Cells[rowGame, 2] = "Дата акту";
			workSheet.Cells[rowGame, 3] = "Не визначено";

			List<string> NameUser = new List<string>();

			foreach (DmxFile file in openedFilesList)
			{
				if (!NameUser.Contains(file.SelectedHuman))
					NameUser.Add(file.SelectedHuman);
			}

			NameUser.Sort(new NaturalStringComparer());

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
						for (int i = 2; i <= openedFilesList.Count + 1; i++)
						{
							cellValue = ((Excel.Range)workSheet.Cells[i, colHuman]).Value;
							cellActValue = ((Excel.Range)workSheet.Cells[i, colHuman + 1]).Value;
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
									cellActValue = "";
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
							workSheet.Cells[rowHumanMax, colHuman] = file.SelectedHuman;
							workSheet.Cells[rowHumanMax, colHuman + 1] = file.ActDateText;
							rowHumanCurr = rowHumanMax;
							rowHumanMax++;
						}

						FindGame(ref workSheet, rowHumanCurr, games);
					}
				}
			}

			for (int i = rowGame + 1; i < rowHumanMax; i++)
			{
				for (int j = colHuman + 1; j < colGameMax; j++)
				{
					((Excel.Range)workSheet.Cells[i, j]).Borders.Weight = Excel.XlBorderWeight.xlThin;
					((Excel.Range)workSheet.Cells[i, j]).Font.Name = "Calibri";
					((Excel.Range)workSheet.Cells[i, j]).Font.Size = 12;
					((Excel.Range)workSheet.Cells[i, j]).HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
					((Excel.Range)workSheet.Cells[i, j]).VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
				}
			}

			for (int i = 1; i < colGameMax + 1; i++)
			{
				((Excel.Range)workSheet.Cells[rowGame, i]).WrapText = true;
				((Excel.Range)workSheet.Cells[rowGame, i]).Font.Bold = true;
				((Excel.Range)workSheet.Cells[rowGame, i]).Borders.Weight = Excel.XlBorderWeight.xlMedium;
				((Excel.Range)workSheet.Cells[rowGame, i]).Font.Name = "Calibri";
				((Excel.Range)workSheet.Cells[rowGame, i]).Font.Size = 12;
				((Excel.Range)workSheet.Cells[rowGame, i]).ColumnWidth = 18.5f;
				((Excel.Range)workSheet.Cells[rowGame, i]).HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
				((Excel.Range)workSheet.Cells[rowGame, i]).VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
			}

			workSheet.Cells[rowHumanMax, colHuman] = "Всього грн.";
			((Excel.Range)workSheet.Cells[rowHumanMax, colHuman]).Borders.Weight = Excel.XlBorderWeight.xlMedium;
			((Excel.Range)workSheet.Cells[rowHumanMax, colHuman]).HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
			((Excel.Range)workSheet.Cells[rowHumanMax, colHuman]).VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
			((Excel.Range)workSheet.Cells[rowHumanMax, colHuman]).Font.Size = 12;
			((Excel.Range)workSheet.Cells[rowHumanMax, colHuman]).Font.Bold = true;

			workSheet.Cells[rowGame, colGameMax] = "Сума грн.";

			for (int i = rowGame + 1; i < rowHumanMax; i++)
			{
				string formula = "=SUM(" + alphabetExcel[2] + "" + i + ":" + alphabetExcel[colGameMax - 2] + "" + i + ")";
				((Excel.Range)workSheet.Cells[i, colGameMax]).Borders.Weight = Excel.XlBorderWeight.xlThin;
				((Excel.Range)workSheet.Cells[i, colGameMax]).Font.Name = "Calibri";
				((Excel.Range)workSheet.Cells[i, colGameMax]).Font.Size = 12;
				((Excel.Range)workSheet.Cells[i, colGameMax]).HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
				((Excel.Range)workSheet.Cells[i, colGameMax]).VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
				((Excel.Range)workSheet.Cells[i, colGameMax]).Formula = formula;
			}

			for (int i = 2; i < colGameMax + 1; i++)
			{
				string formula = "=SUM(" + alphabetExcel[i - 1] + "" + (rowGame + 1) + ":" + alphabetExcel[i - 1] + "" + (rowHumanMax - 1) + ")";
				((Excel.Range)workSheet.Cells[rowHumanMax, i]).Borders.Weight = Excel.XlBorderWeight.xlMedium;
				((Excel.Range)workSheet.Cells[rowHumanMax, i]).Font.Name = "Calibri";
				((Excel.Range)workSheet.Cells[rowHumanMax, i]).Font.Size = 12;
				((Excel.Range)workSheet.Cells[rowHumanMax, i]).HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
				((Excel.Range)workSheet.Cells[rowHumanMax, i]).VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;

				if (i > 2)
					((Excel.Range)workSheet.Cells[rowHumanMax, i]).Formula = formula;
			}

			for (int i = 2; i < rowHumanMax; i++)
			{
				((Excel.Range)workSheet.Cells[i, colHuman]).Borders.Weight = Excel.XlBorderWeight.xlMedium;
				((Excel.Range)workSheet.Cells[i, colHuman]).Font.Name = "Calibri";
				((Excel.Range)workSheet.Cells[i, colHuman]).Font.Size = 12;
				((Excel.Range)workSheet.Cells[i, colHuman]).HorizontalAlignment = Excel.XlHAlign.xlHAlignLeft;
				((Excel.Range)workSheet.Cells[i, colHuman]).VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;

				if (WorkType.Development == _workType)
				{
					string cellValue = ((Excel.Range)workSheet.Cells[i, colHuman]).Value;
					string cellActValue = ((Excel.Range)workSheet.Cells[i, colHuman + 1]).Value;
					int cellSummValue = Convert.ToInt32(((Excel.Range)workSheet.Cells[i, colGameMax]).Value);
					if (cellSummValue <= 0)
						((Excel.Range)workSheet.Cells[i, 1]).Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Yellow);
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
									if(games.Count > 0)
										((Excel.Range)workSheet.Cells[i, 1]).Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Yellow);

									break;
								}
							}
						}
					}
				}
			}

			((Excel.Range)workSheet.Columns[1]).AutoFit();
		}

		private void FillSheet2(ref Excel.Worksheet workSheet, Excel.Application excelApp, IList<DmxFile> openedFilesList, IList<GameObject> gameObjects, List<string> alphabetExcel, int _indexSheet, WorkType _workType)
		{
			rowGame = 2;
			rowGameCurr = 2;
			rowHumanCurr = 1;
			rowValueMax = 1;

			workSheet = (Excel.Worksheet)excelApp.Sheets[_indexSheet];

			List<string> NameUser = new List<string>();

			foreach (DmxFile file in openedFilesList)
			{
				if (!NameUser.Contains(file.SelectedHuman))
					NameUser.Add(file.SelectedHuman);
			}

			NameUser.Sort(new NaturalStringComparer());

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
							for (int i = 1; i <= rowValueMax + 1; i++)
							{
								cellValue = ((Excel.Range)workSheet.Cells[i, 1]).Value;
								cellActValue = ((Excel.Range)workSheet.Cells[i, 3]).Value;
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
								workSheet.Cells[rowHumanCurr, 1] = file.SelectedHuman;
								workSheet.Cells[rowHumanCurr, 3] = file.ActDateText;
							}

							rowGameCurr = rowHumanCurr + 1;
							FindGame2(ref workSheet, rowGameCurr, games);
							rowValueMax = rowGameCurr;
							if (string.IsNullOrEmpty(cellValue))
							{
								((Excel.Range)workSheet.Cells[rowHumanCurr, 1]).Borders.Weight = Excel.XlBorderWeight.xlMedium;
								((Excel.Range)workSheet.Cells[rowHumanCurr, 1]).Font.Name = "Calibri";
								((Excel.Range)workSheet.Cells[rowHumanCurr, 1]).Font.Bold = true;
								((Excel.Range)workSheet.Cells[rowHumanCurr, 1]).Font.Size = 12;
								((Excel.Range)workSheet.Cells[rowHumanCurr, 1]).HorizontalAlignment = Excel.XlHAlign.xlHAlignLeft;
								((Excel.Range)workSheet.Cells[rowHumanCurr, 1]).VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;

								((Excel.Range)workSheet.Cells[rowHumanCurr, 3]).Font.Name = "Calibri";
								((Excel.Range)workSheet.Cells[rowHumanCurr, 3]).Font.Size = 12;
								((Excel.Range)workSheet.Cells[rowHumanCurr, 3]).Borders.Weight = Excel.XlBorderWeight.xlThin;
								((Excel.Range)workSheet.Cells[rowHumanCurr, 3]).HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
								((Excel.Range)workSheet.Cells[rowHumanCurr, 3]).VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;

								((Excel.Range)workSheet.Cells[rowHumanCurr, 2]).Font.Name = "Calibri";
								((Excel.Range)workSheet.Cells[rowHumanCurr, 2]).Borders.Weight = Excel.XlBorderWeight.xlThin;
								((Excel.Range)workSheet.Cells[rowHumanCurr, 2]).Font.Size = 12;
								((Excel.Range)workSheet.Cells[rowHumanCurr, 2]).HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
								((Excel.Range)workSheet.Cells[rowHumanCurr, 2]).VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
							}

							string formula = "=SUM(" + alphabetExcel[1] + "" + (rowHumanCurr + 1) + ":" + alphabetExcel[1] + "" + (rowGameCurr - 1) + ")";
							((Excel.Range)workSheet.Cells[rowHumanCurr, 2]).Formula = formula;
							rowHumanCurr = rowValueMax + 1;
						}
					}
				}
			}

			if (WorkType.Development == _workType)
			{
				for (int i = 1; i <= rowValueMax + 1; i++)
				{
					string cellValue = ((Excel.Range)workSheet.Cells[i, 1]).Value;

					if (!string.IsNullOrEmpty(cellValue))
					{
						string cellActValue;
						foreach (DmxFile file in openedFilesList)
						{
							if (cellValue == file.SelectedHuman)
							{
								cellActValue = ((Excel.Range)workSheet.Cells[i, 3]).Value;
								if (cellActValue == file.ActDateText)
								{
									Dictionary<string, int> games = new Dictionary<string, int>();
									CalculateGamePrice.GamePrice(ref games, file.BackDataModels, gameObjects, WorkType.Rework);
									if (games.Count > 0)
										((Excel.Range)workSheet.Cells[i, 1]).Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Yellow);

									break;
								}
							}
						}
					}
					
				}
			}

			((Excel.Range)workSheet.Columns[1]).AutoFit();
			((Excel.Range)workSheet.Columns[2]).AutoFit();
			((Excel.Range)workSheet.Columns[3]).AutoFit();
		}

		private void FindGame(ref Excel.Worksheet workSheet, int _rowHumanCurr, Dictionary<string, int> games)
		{
			foreach (var game in games)
			{
				for (int i = 2; i <= colGameMax; i++)
				{
					string cellValue = ((Excel.Range)workSheet.Cells[rowGame, i]).Value;
					colGameCurr = i;
					if (string.IsNullOrEmpty(cellValue))
					{
						workSheet.Cells[rowGame, i] = game.Key;
						((Excel.Range)workSheet.Cells[rowGame, i]).Borders.Weight = Excel.XlBorderWeight.xlMedium;
						colGameMax++;
						workSheet.Cells[_rowHumanCurr, colGameCurr] = game.Value.ToString();
						((Excel.Range)workSheet.Cells[_rowHumanCurr, colGameCurr]).Font.Name = "Calibri";
						((Excel.Range)workSheet.Cells[_rowHumanCurr, colGameCurr]).Font.Size = 12;
						((Excel.Range)workSheet.Cells[_rowHumanCurr, colGameCurr]).HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
						((Excel.Range)workSheet.Cells[_rowHumanCurr, colGameCurr]).VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;

						break;
					}
					else if (cellValue == game.Key)
					{
						int _iSummCell = Convert.ToInt32(((Excel.Range)workSheet.Cells[_rowHumanCurr, colGameCurr]).Value) + game.Value;
						workSheet.Cells[_rowHumanCurr, colGameCurr] = _iSummCell.ToString();

						((Excel.Range)workSheet.Cells[_rowHumanCurr, colGameCurr]).Font.Name = "Calibri";
						((Excel.Range)workSheet.Cells[_rowHumanCurr, colGameCurr]).Font.Size = 12;
						((Excel.Range)workSheet.Cells[_rowHumanCurr, colGameCurr]).HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
						((Excel.Range)workSheet.Cells[_rowHumanCurr, colGameCurr]).VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
						break;
					}
				}
			}
		}

		private void FindGame2(ref Excel.Worksheet workSheet, int _rowGameCurr, Dictionary<string, int> games)
		{
			int _rowStartGameCurr = _rowGameCurr;
			string cellValue;

			for (int i = _rowGameCurr; i <= 100; i++)
			{
				cellValue = ((Excel.Range)workSheet.Cells[i, 1]).Value;
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
					cellValue = ((Excel.Range)workSheet.Cells[i, 1]).Value;
					if (cellValue == game.Key)
					{
						int _iSummCell = Convert.ToInt32(((Excel.Range)workSheet.Cells[i, 2]).Value) + game.Value;
						workSheet.Cells[i, 2] = _iSummCell.ToString();
						_bGameFind = true;
						break;
					}
				}

				if (!_bGameFind)
				{
					workSheet.Cells[rowGameCurr, 1] = game.Key;
					workSheet.Cells[rowGameCurr, 2] = game.Value.ToString();

					((Excel.Range)workSheet.Cells[rowGameCurr, 1]).Font.Name = "Calibri";
					((Excel.Range)workSheet.Cells[rowGameCurr, 1]).Borders.Weight = Excel.XlBorderWeight.xlThin;
					((Excel.Range)workSheet.Cells[rowGameCurr, 1]).Font.Size = 12;
					((Excel.Range)workSheet.Cells[rowGameCurr, 1]).HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
					((Excel.Range)workSheet.Cells[rowGameCurr, 1]).VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;

					((Excel.Range)workSheet.Cells[rowGameCurr, 2]).Font.Name = "Calibri";
					((Excel.Range)workSheet.Cells[rowGameCurr, 2]).Borders.Weight = Excel.XlBorderWeight.xlThin;
					((Excel.Range)workSheet.Cells[rowGameCurr, 2]).Font.Size = 12;
					((Excel.Range)workSheet.Cells[rowGameCurr, 2]).HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
					((Excel.Range)workSheet.Cells[rowGameCurr, 2]).VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
					rowGameCurr++;
				}
			}
		}

		private void CreateXlsx(string path)
		{
			//if (!File.Exists(path))
			{
				SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Create(path, SpreadsheetDocumentType.Workbook);

				WorkbookPart workbookpart = spreadsheetDocument.AddWorkbookPart();
				workbookpart.Workbook = new Workbook();

				WorksheetPart worksheetPart = workbookpart.AddNewPart<WorksheetPart>();
				worksheetPart.Worksheet = new Worksheet(new SheetData());

				Sheets sheets = spreadsheetDocument.WorkbookPart.Workbook.AppendChild(new Sheets());
				Sheet sheet = new Sheet()
				{
					Id = spreadsheetDocument.WorkbookPart.
					GetIdOfPart(worksheetPart),
					SheetId = 1,
					Name = "Розробка"
				};
				sheets.Append(sheet);

				sheet = new Sheet()
				{
					Id = spreadsheetDocument.WorkbookPart.
					GetIdOfPart(worksheetPart),
					SheetId = 2,
					Name = "Підтримка"
				};
				sheets.Append(sheet);

				sheet = new Sheet()
				{
					Id = spreadsheetDocument.WorkbookPart.
					GetIdOfPart(worksheetPart),
					SheetId = 3,
					Name = "Разом"
				};
				sheets.Append(sheet);

				workbookpart.Workbook.Save();
				spreadsheetDocument.Close();
			}
		}
	}
}
