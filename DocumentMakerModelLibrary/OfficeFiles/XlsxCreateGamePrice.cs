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
		int colGameMax = 4;
		int colGameCurr = 4;

		int rowHumanCurr = 1;
		int rowHumanMax = 2;
		int colHuman = 1;

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
				FillSheet(ref workSheet, excelApp, openedFilesList, gameObjects, alphabetExcel, 2, WorkType.Rework);
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
			workSheet = (Excel.Worksheet)excelApp.Sheets[_indexSheet];

			((Excel.Range)workSheet.Columns[2]).Font.Name = "Calibri";
			((Excel.Range)workSheet.Columns[2]).Font.Size = 14;
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

						for (int i = 2; i <= openedFilesList.Count + 1; i++)
						{
							string cellValue = ((Excel.Range)workSheet.Cells[i, colHuman]).Value;
							string cellActValue = ((Excel.Range)workSheet.Cells[i, colHuman + 1]).Value;
							rowHumanCurr = i;
							if (string.IsNullOrEmpty(cellValue))
							{
								workSheet.Cells[i, colHuman] = file.SelectedHuman;
								workSheet.Cells[i, colHuman + 1] = file.ActDateText;
								rowHumanMax++;
								FindGame(ref workSheet, rowHumanCurr, games);
								break;
							}
							else if (cellValue == file.SelectedHuman)
							{
								if (cellActValue == file.ActDateText)
								{
									FindGame(ref workSheet, rowHumanCurr, games);
									break;
								}
							}
						}
					}
				}
			}

			for (int i = rowGame + 1; i < rowHumanMax; i++)
			{
				for (int j = colHuman + 1; j < colGameMax; j++)
				{
					((Excel.Range)workSheet.Cells[i, j]).Borders.Weight = Excel.XlBorderWeight.xlThin;
					((Excel.Range)workSheet.Cells[i, j]).Font.Name = "Calibri";
					((Excel.Range)workSheet.Cells[i, j]).Font.Size = 14;
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
				((Excel.Range)workSheet.Cells[rowGame, i]).Font.Size = 14;
				((Excel.Range)workSheet.Cells[rowGame, i]).ColumnWidth = 18.5f;
				((Excel.Range)workSheet.Cells[rowGame, i]).HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
				((Excel.Range)workSheet.Cells[rowGame, i]).VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
			}

			for (int i = 2; i < rowHumanMax; i++)
			{
				((Excel.Range)workSheet.Cells[i, colHuman]).Borders.Weight = Excel.XlBorderWeight.xlMedium;
				((Excel.Range)workSheet.Cells[i, colHuman]).Font.Name = "Calibri";
				((Excel.Range)workSheet.Cells[i, colHuman]).Font.Size = 14;
				((Excel.Range)workSheet.Cells[i, colHuman]).HorizontalAlignment = Excel.XlHAlign.xlHAlignLeft;
				((Excel.Range)workSheet.Cells[i, colHuman]).VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
			}

			workSheet.Cells[rowHumanMax, colHuman] = "Всього грн.";
			((Excel.Range)workSheet.Cells[rowHumanMax, colHuman]).Borders.Weight = Excel.XlBorderWeight.xlMedium;
			((Excel.Range)workSheet.Cells[rowHumanMax, colHuman]).HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
			((Excel.Range)workSheet.Cells[rowHumanMax, colHuman]).VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
			((Excel.Range)workSheet.Cells[rowHumanMax, colHuman]).Font.Size = 14;
			((Excel.Range)workSheet.Cells[rowHumanMax, colHuman]).Font.Bold = true;


			workSheet.Cells[rowGame, colGameMax] = "Сума грн.";

			for (int i = rowGame + 1; i < rowHumanMax; i++)
			{
				string formula = "=SUM(" + alphabetExcel[2] + "" + i + ":" + alphabetExcel[colGameMax - 2] + "" + i + ")";
				((Excel.Range)workSheet.Cells[i, colGameMax]).Borders.Weight = Excel.XlBorderWeight.xlThin;
				((Excel.Range)workSheet.Cells[i, colGameMax]).Font.Name = "Calibri";
				((Excel.Range)workSheet.Cells[i, colGameMax]).Font.Size = 14;
				((Excel.Range)workSheet.Cells[i, colGameMax]).HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
				((Excel.Range)workSheet.Cells[i, colGameMax]).VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
				((Excel.Range)workSheet.Cells[i, colGameMax]).Formula = formula;
			}

			for (int i = 2; i < colGameMax + 1; i++)
			{
				string formula = "=SUM(" + alphabetExcel[i - 1] + "" + (rowGame + 1) + ":" + alphabetExcel[i - 1] + "" + (rowHumanMax - 1) + ")";
				((Excel.Range)workSheet.Cells[rowHumanMax, i]).Borders.Weight = Excel.XlBorderWeight.xlMedium;
				((Excel.Range)workSheet.Cells[rowHumanMax, i]).Font.Name = "Calibri";
				((Excel.Range)workSheet.Cells[rowHumanMax, i]).Font.Size = 14;
				((Excel.Range)workSheet.Cells[rowHumanMax, i]).HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
				((Excel.Range)workSheet.Cells[rowHumanMax, i]).VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;

				if (i > 2)
					((Excel.Range)workSheet.Cells[rowHumanMax, i]).Formula = formula;
			}

			((Excel.Range)workSheet.Columns[1]).AutoFit();

			rowGame = 1;
			colGameMax = 4;
			colGameCurr = 4;

			rowHumanCurr = 1;
			rowHumanMax = 2;
			colHuman = 1;
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
						((Excel.Range)workSheet.Cells[_rowHumanCurr, colGameCurr]).Font.Size = 14;
						((Excel.Range)workSheet.Cells[_rowHumanCurr, colGameCurr]).HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
						((Excel.Range)workSheet.Cells[_rowHumanCurr, colGameCurr]).VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;

						break;
					}
					else if (cellValue == game.Key)
					{
						workSheet.Cells[_rowHumanCurr, colGameCurr] = game.Value.ToString();

						((Excel.Range)workSheet.Cells[_rowHumanCurr, colGameCurr]).Font.Name = "Calibri";
						((Excel.Range)workSheet.Cells[_rowHumanCurr, colGameCurr]).Font.Size = 14;
						((Excel.Range)workSheet.Cells[_rowHumanCurr, colGameCurr]).HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
						((Excel.Range)workSheet.Cells[_rowHumanCurr, colGameCurr]).VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
						break;
					}
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
