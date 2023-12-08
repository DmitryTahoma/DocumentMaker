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

		public XlsxCreateGamePrice()
		{
			NameUser = new List<string>();
			Zero();
		}

		public void Zero()
		{
			rowGame = 1;
			rowGameCurr = 1;
			colGameMax = 4;
			colGameCurr = 4;

			rowHumanCurr = 1;
			rowHumanMax = 2;

			colHuman = 1;
			colDate = 1;
			colSumma = 1;
			colGame = 1;

			rowValueMax = 1;

			iDevelopmentSum = 0;
			iReworkSum = 0;
			iAllSum = 0;
			NameUser.Clear();
		}

		public void CreateXlsxXml(string path, IList<DmxFile> openedFilesList, IList<GameObject> gameObjects, Dictionary<string, int> dFillSheet)
		{
			List<string> lSheetsName = new List<string>();
			foreach (KeyValuePair<string, int> name in dFillSheet)
			{
				if (name.Value != -1)
					lSheetsName.Add(name.Key);
			}

			ExcelExporter.CreateXlsx(path, lSheetsName);
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
				ExcelExporter.OpenXlsx(path);

				foreach (KeyValuePair<string, int> name in dFillSheet)
				{
					if (name.Value != -1)
					{
						if (ExcelExporter.LoadSheet(name.Key))
						{
							WorkType typeWork;
							FillSheetType typeSheet = (FillSheetType)name.Value;

							if (name.Key == "Розробка")
								typeWork = WorkType.Development;
							else if (name.Key == "Пiдтримка")
								typeWork = WorkType.Rework;
							else if (name.Key == "Разом")
								typeWork = WorkType.All;
							else
								typeWork = WorkType.Empty;

							if (typeWork != WorkType.Empty)
							{
								if (typeSheet == FillSheetType.Table)
									FillSheetExcelTable(openedFilesList, gameObjects, typeWork);
								else if (typeSheet == FillSheetType.List)
									FillSheetExcelList(openedFilesList, gameObjects, typeWork);
								else if (typeSheet == FillSheetType.Line)
									FillSheetExcelLine(openedFilesList, gameObjects, typeWork);
							}
						}
					}
				}

				ExcelExporter.SaveFile();
				ExcelExporter.CloseFile();

				MessageBox.Show("Файл збережено.",
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
						string cellActValue;

						for (int i = rowGame + 1; i <= openedFilesList.Count; i++)
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

			for (int i = 1; i < rowHumanMax; i++)
			{
				ExcelExporter.SetStyle(i, colHuman, 12);
				string formula = "=SUM(" + ExcelExporter.GetCellName(i, 2) + ":" + ExcelExporter.GetCellName(i, colGameMax - 1) + ")";
				ExcelExporter.SetCellValue(i, colGameMax, formula, true);
			}

			for (int i = rowGame + 1; i < rowHumanMax; i++)
			{
				for (int j = colHuman + 1; j < colGameMax + 1; j++)
				{
					ExcelExporter.SetStyle(i, j, 4);
				}
			}

			ExcelExporter.SetCellValue(rowHumanMax, colHuman, "Всього грн.");
			ExcelExporter.SetCellValue(rowGame, colGameMax, "Сума грн.");
			ExcelExporter.SetStyle(rowHumanMax, colHuman, 5);

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
						ExcelExporter.SetStyle(i, colHuman, 13);
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
										ExcelExporter.SetStyle(i, colHuman, 13);

									break;
								}
							}
						}
					}
				}
			}

			ExcelExporter.SetStyleRow(rowGame, 11);
			ExcelExporter.SetStyleRow(rowHumanMax, 6);

			CalculateSumma(openedFilesList, gameObjects);

			ExcelExporter.SetCellValue(rowHumanMax + 1, 0, "Розробка грн.");
			ExcelExporter.SetCellValue(rowHumanMax + 1, 1, iDevelopmentSum.ToString());
			ExcelExporter.SetCellValue(rowHumanMax + 2, 0, "Пiдтримка грн.");
			ExcelExporter.SetCellValue(rowHumanMax + 2, 1, iReworkSum.ToString());
			ExcelExporter.SetCellValue(rowHumanMax + 3, 0, "Всього грн.");
			string formulaSum = "=SUM(" + ExcelExporter.GetCellName(rowHumanMax + 1, 1) + ":" + ExcelExporter.GetCellName(rowHumanMax + 2, 1) + ")";
			ExcelExporter.SetCellValue(rowHumanMax + 3, 1, formulaSum, true);

			for (int i = 1; i < 4; i++)
			{
				ExcelExporter.SetStyle(rowHumanMax + i, colHuman, 5);
				ExcelExporter.SetStyle(rowHumanMax + i, colHuman + 1, 6);
			}

			for (int i = 1; i < colGameMax + 1; i++)
				ExcelExporter.SetColumnWidth(i, 19);

			ExcelExporter.AutoFitCell(colHuman);
		}

		private void FillSheetExcelList(IList<DmxFile> openedFilesList, IList<GameObject> gameObjects, WorkType _workType)
		{
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
							string cellActValue;
							for (int i = 0; i <= rowValueMax; i++)
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
										cellValue = "";
										rowHumanCurr = rowValueMax + 1;
									}
								}
								else
								{
									cellValue = "";
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
								ExcelExporter.SetStyle(rowHumanCurr, colHuman, 9);
								ExcelExporter.SetStyle(rowHumanCurr, colDate, 4);
								ExcelExporter.SetStyle(rowHumanCurr, colSumma, 4);
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
										ExcelExporter.SetStyle(i, colHuman, 10);

									break;
								}
							}
						}
					}

				}
			}

			CalculateSumma(openedFilesList, gameObjects);
			for (int i = 0; i < 3; i++)
			{
				ExcelExporter.SetStyle(i, 4, 5);
				ExcelExporter.SetStyle(i, 5, 6);
			}

			ExcelExporter.SetCellValue(0, 4, "Розробка грн.");
			ExcelExporter.SetCellValue(0, 5, iDevelopmentSum.ToString());
			ExcelExporter.SetCellValue(1, 4, "Пiдтримка грн.");
			ExcelExporter.SetCellValue(1, 5, iReworkSum.ToString());
			ExcelExporter.SetCellValue(2, 4, "Всього грн.");
			string formulaSum = "=SUM(" + ExcelExporter.GetCellName(0, 5) + ":" + ExcelExporter.GetCellName(1, 5) + ")";
			ExcelExporter.SetCellValue(2, 5, formulaSum, true);

			ExcelExporter.AutoFitCell(colDate);
			ExcelExporter.AutoFitCell(colHuman);
			ExcelExporter.AutoFitCell(colSumma);
			ExcelExporter.AutoFitCell(4);
			ExcelExporter.AutoFitCell(5);
		}

		private void FillSheetExcelLine(IList<DmxFile> openedFilesList, IList<GameObject> gameObjects, WorkType _workType)
		{
			rowGame = 1;
			rowValueMax = 1;

			colDate = 0;
			colHuman = 1;
			colGame = 2;
			colSumma = 3;

			ExcelExporter.SetCellValue(0, colDate, "Дата акту");
			ExcelExporter.SetCellValue(0, colHuman, "ПIБ");
			ExcelExporter.SetCellValue(0, colGame, "Назва гри");
			ExcelExporter.SetCellValue(0, colSumma, "Сума грн.");

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
									for (int i = rowGame; i <= rowValueMax; i++)
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
										rowValueMax++;
									}
								}
							}
						}
					}
				}
			}


			ExcelExporter.SetStyleColumn(colDate, 3);
			ExcelExporter.SetStyleColumn(colHuman, 3);
			ExcelExporter.SetStyleColumn(colGame, 3);
			ExcelExporter.SetStyleColumn(colSumma, 4);

			CalculateSumma(openedFilesList, gameObjects);
			for (int i = 0; i < 3; i++)
			{
				ExcelExporter.SetStyle(i, colSumma + 2, 5);
				ExcelExporter.SetStyle(i, colSumma + 3, 6);
			}

			for (int i = 0; i < 4; i++)
				ExcelExporter.SetStyle(0, i, 11);

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
										ExcelExporter.SetStyle(i, colHuman, 2);

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
			int sizeRow = ExcelExporter.RowLength();
			for (int i = _rowGameCurr; i <= sizeRow; i++)
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
					ExcelExporter.SetStyle(rowGameCurr, colGame, 8);
					ExcelExporter.SetStyle(rowGameCurr, colSumma, 4);
					rowGameCurr++;
				}
			}
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
