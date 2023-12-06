using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DocumentMakerModelLibrary.OfficeFiles
{
	internal class ExcelExporter
	{
		private static List<string> alphabetExcel;
		private static SheetData sheetData;
		private static SharedStringTable sharedStringTable;
		private static Columns columns;

		public ExcelExporter()
		{
			sheetData = null;
			sharedStringTable = null;

			string[] arr_EN = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };

			alphabetExcel = new List<string>(arr_EN);

			for (int i = 0; i < arr_EN.Length; i++)
			{
				for (int j = 0; j < arr_EN.Length; j++)
				{
					alphabetExcel.Add(arr_EN[i] + arr_EN[j]);
				}
			}
		}

		public static void SetData(SheetData _sheetData, SharedStringTable _sharedStringTable, Columns _columns)
		{
			sheetData = _sheetData;
			sharedStringTable = _sharedStringTable;
			columns = _columns;
		}

		public static void InsertEmptyCell(Row row, Cell cell, int rowIndex, int cellIndex)
		{
			if (cell == null)
				row.InsertAt(new Cell() { DataType = CellValues.InlineString, InlineString = null, CellReference = GetCellName(rowIndex, cellIndex) }, cellIndex);
			else
			{
				cell.DataType = CellValues.String;
				cell.CellValue = null;
				cell.InlineString = null;
			}
		}

		private static DocumentFormat.OpenXml.EnumValue<CellValues> ResolveCellDataTypeOnValue(string text)
		{
			if (int.TryParse(text, out _) || double.TryParse(text, out double _))
			{
				return CellValues.Number;
			}
			else
			{
				return CellValues.String;
			}
		}

		private static void InsertFomulaCell(Cell cell, string content)
		{
			cell.CellFormula = new CellFormula { CalculateCell = true, Text = content };
			cell.DataType = CellValues.Number;
		}

		private static void InsertCell(Cell cell, string content)
		{
			cell.DataType = ResolveCellDataTypeOnValue(content);
			cell.CellValue = new CellValue(content);
		}

		private static void InsertRowCell(int rowIndex, int cellIndex)
		{
			int sizeRow = sheetData.Elements<Row>().Count();
			if (rowIndex >= sizeRow)
			{
				for (int i = sizeRow; i <= rowIndex; i++)
				{
					Row row = new Row();
					for (int j = 0; j <= cellIndex; j++)
					{
						InsertEmptyCell(row, null, i,j);
					}

					sheetData.InsertAt(row, i);

				}
			}
			else
			{
				int idxRow = 0;
				foreach (Row itemRow in sheetData.Elements<Row>())
				{
					if (idxRow == rowIndex)
					{
						int sizeCell = itemRow.Elements<Cell>().Count();
						if (cellIndex >= sizeCell)
						{
							for (int i = sizeCell; i <= cellIndex; i++)
							{
								InsertEmptyCell(itemRow, null, idxRow, i);
							}
						}

						break;
					}
					idxRow++;
				}
			}
		}
		public static void SetCellValue(int rowIndex, int cellIndex, string value, bool formula = false)
		{
			InsertRowCell(rowIndex, cellIndex);

			Cell cell = GetCell(rowIndex, cellIndex);

			if(cell != null)
			{
				if (formula)
					InsertFomulaCell(cell, value);
				else
					InsertCell(cell, value);
			}
		}

		public static string GetCellName(int rowIndex, int cellIndex)
		{
			string name = alphabetExcel[cellIndex] + (rowIndex + 1);
			return name;
		}

		public static Cell GetCell(int rowIndex, int cellIndex)
		{
			InsertRowCell(rowIndex, cellIndex);
			int idxRow = 0;
			foreach (Row itemRow in sheetData.Elements<Row>())
			{
				if (idxRow == rowIndex)
				{
					return GetCell(itemRow, GetCellName(rowIndex, cellIndex));
				}
				idxRow++;
			}

			return null;
		}

		private static Cell GetCell(Row row, string cellName)
		{
			if(row == null)
				return null;

			foreach (Cell itemCell in row.Elements<Cell>())
			{
				if (itemCell.CellReference == cellName)
				{
					return itemCell;
				}
			}

			return null;
		}

		public static string GetValue(int rowIndex, int cellIndex)
		{
			InsertRowCell(rowIndex, cellIndex);

			Cell cell = null;
			string value = "";
			int idxRow = 0;
			foreach (Row itemRow in sheetData.Elements<Row>())
			{
				if (idxRow == rowIndex)
				{
					cell = GetCell(itemRow, GetCellName(rowIndex, cellIndex));
					break;
				}
				idxRow++;
			}

			if (cell != null)
			{
				value = cell.CellValue == null ? cell.InnerText : cell.CellValue.InnerText;
			}

			return value;
		}

		private static int GetMaxCharacterWidth(int cellIndex)
		{
			if (sheetData == null)
				return 7;
			//iterate over all cells getting a max char value for each column
			uint[] numberStyles = new uint[] { 5, 6, 7, 8 }; //styles that will add extra chars
			uint[] boldStyles = new uint[] { 1, 2, 3, 4, 6, 7, 8 }; //styles that will bold
			int maxColWidth = 0;
			int rowIndex = 0;
			foreach (Row row in sheetData.Elements<Row>())
			{
				//using cell index as my column
				Cell cell = GetCell(row, GetCellName(rowIndex, cellIndex));

				if (cell != null)
				{
					string cellValue = cell.CellValue == null ? cell.InnerText : cell.CellValue.InnerText;
					int cellTextLength = cellValue.Length;

					//if (cell.StyleIndex != null && numberStyles.Contains(cell.StyleIndex))
					//{
					//	int thousandCount = (int)Math.Truncate((double)cellTextLength / 4);

					//	//add 3 for '.00' 
					//	cellTextLength += 3 + thousandCount;
					//}

					if (cell.StyleIndex != null)
					{
						//add an extra char for bold - not 100% acurate but good enough for what i need.
						cellTextLength += 1;
					}

					if (cellTextLength > maxColWidth)
					{
						maxColWidth = cellTextLength;
					}
				}

				rowIndex++;
			}

			return maxColWidth;
		}

		public static void AutoFitCell(int cellIndex)
		{
			var maxColWidth = GetMaxCharacterWidth(cellIndex);

			double maxWidth = 7;
			int idx = 0;
			foreach(var item in columns.Elements<Column>())
			{
				if(idx == cellIndex)
				{
					double width = Math.Truncate((maxColWidth * maxWidth + 5) / maxWidth * 256) / 256;
					item.Max = (uint)width;
					item.Width = width;
					break;
				}
				idx++;
			}
		}

		public static void SetColumnWidth(int cellIndex, int colWidth)
		{
			int idx = 0;
			foreach (var item in columns.Elements<Column>())
			{
				if (idx == cellIndex)
				{
					item.Max = (uint)colWidth;
					item.Width = colWidth;
					break;
				}
				idx++;
			}
		}
	}
}
