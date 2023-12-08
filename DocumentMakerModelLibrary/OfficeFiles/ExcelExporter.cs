using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DocumentMakerModelLibrary.OfficeFiles
{
	internal class ExcelExporter
	{
		private static List<string> alphabetExcel;
		private static List<Sheet> lSheets;
		private static WorksheetPart worksheetPart;
		private static Sheets sheets;
		private static SheetData sheetData;
		private static Stylesheet stylesheet;
		private static WorkbookPart workbookpart;
		private static SpreadsheetDocument spreadsheetDocument;
		private static SharedStringTable sharedStringTable;
		private static Columns columns;
		private static bool isEditable;

		public ExcelExporter()
		{
			string[] arr_EN = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };

			alphabetExcel = new List<string>(arr_EN);

			for (int i = 0; i < arr_EN.Length; i++)
			{
				for (int j = 0; j < arr_EN.Length; j++)
				{
					alphabetExcel.Add(arr_EN[i] + arr_EN[j]);
				}
			}
			Zero();
		}

		private static void Zero()
		{
			isEditable = true;

			ZeroFile();
			ZeroSheet();
		}

		private static void ZeroSheet()
		{
			worksheetPart = null;
			sheetData = null;
			sharedStringTable = null;
			columns = null;
		}

		private static void ZeroFile()
		{
			lSheets = null;
			spreadsheetDocument = null;
			workbookpart = null;
			sheets = null;
			stylesheet = null;
		}

		public static int RowLength()
		{
			uint iLength = 0;
			foreach (Row itemRow in sheetData.Elements<Row>())
				iLength = itemRow.RowIndex;

			return (int)iLength;
		}

		public static int CellLength(int rowIndex)
		{
			int iLength = 0;
			int rowIndexTemp = rowIndex + 1;
			foreach (Row itemRow in sheetData.Elements<Row>())
			{
				if (itemRow.RowIndex == rowIndexTemp)
				{
					foreach (Cell itemCell in itemRow.Elements<Cell>())
					{
						iLength = GetCellId(itemCell.CellReference);
					}
					break;
				}
			}

			return iLength;
		}

		private static EnumValue<CellValues> ResolveCellDataTypeOnValue(string text)
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

		private static Cell CreateCell(Row row, string cellName)
		{
			Cell cell = null;
			int indexCellId = GetCellId(cellName);
			int indexLastCell = -1;
			int indexAdd = 0;
			int i = 0;
			foreach (Cell itemCell in row.Elements<Cell>())
			{
				if (itemCell.CellReference == cellName)
				{
					cell = itemCell;
					break;
				}

				int cellId = GetCellId(itemCell.CellReference);
				if (cellId < indexCellId)
				{
					if (cellId > indexLastCell)
					{
						indexLastCell = cellId;
						indexAdd = i + 1;
					}
				}
				else if (cellId >= indexCellId)
					break;

				i++;
			}

			if (cell == null)
			{
				cell = new Cell() { DataType = CellValues.String, InlineString = null, CellReference = cellName };
				row.InsertAt(cell, indexAdd);
			}

			return cell;
		}

		private static Cell CreateRow(int rowIndex, int cellIndex)
		{
			Row row = null;
			Cell cell;
			int indexLastRow = 0;
			int rowIndexTemp = rowIndex + 1;
			int indexAdd = 0;
			int i = 0;
			foreach (Row itemRow in sheetData.Elements<Row>())
			{
				if (itemRow.RowIndex == rowIndexTemp)
				{
					row = itemRow;
					break;
				}

				if (itemRow.RowIndex < rowIndexTemp)
				{
					if (itemRow.RowIndex > indexLastRow)
					{
						indexLastRow = (int)(uint)itemRow.RowIndex;
						indexAdd = i + 1;
					}
				}
				else if (itemRow.RowIndex >= rowIndexTemp)
					break;

				i++;
			}

			if (row == null)
			{
				row = new Row() { RowIndex = (uint)rowIndexTemp };
				cell = CreateCell(row, GetCellName(rowIndex, cellIndex));
				sheetData.InsertAt(row, indexAdd);
			}
			else
				cell = CreateCell(row, GetCellName(rowIndex, cellIndex));

			return cell;
		}

		public static void SetCellValue(int rowIndex, int cellIndex, string value, bool formula = false)
		{
			Cell cell = CreateRow(rowIndex, cellIndex);

			if (cell != null)
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

		public static int GetCellId(string cellName)
		{
			int iNumber = alphabetExcel.IndexOf(GetCellLetter(cellName)) + 1;

			return iNumber;
		}

		public static string GetCellLetter(string cellName)
		{
			string sLetter = "";
			for (int i = 0; i < cellName.Length; i++)
			{
				char c = cellName[i];
				if (char.IsLetter(c))
					sLetter += c;
			}

			return sLetter;
		}

		public static int GetCellNumber(string cellName)
		{
			string sNumber = "";
			int iNumber;
			for (int i = 0; i < cellName.Length; i++)
			{
				char c = cellName[i];
				if (char.IsDigit(c))
					sNumber += c;
			}

			int.TryParse(sNumber, out iNumber);

			return iNumber;
		}

		public static Cell GetCell(int rowIndex, int cellIndex)
		{
			int rowIndexTemp = rowIndex + 1;
			foreach (Row itemRow in sheetData.Elements<Row>())
			{
				if (itemRow.RowIndex == rowIndexTemp)
					return GetCell(itemRow, GetCellName(rowIndex, cellIndex));
			}
			return null;
		}

		public static Cell GetCell(Row row, int cellIndex)
		{
			foreach (Cell itemCell in row.Elements<Cell>())
			{
				if (itemCell.CellReference == GetCellName((int)(uint)row.RowIndex - 1, cellIndex))
				{
					return itemCell;
				}
			}
			return null;
		}

		private static Cell GetCell(Row row, string cellName)
		{
			if (row == null)
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

		private static Row GetRow(int rowIndex)
		{
			int rowIndexTemp = rowIndex + 1;
			foreach (Row itemRow in sheetData.Elements<Row>())
			{
				if (itemRow.RowIndex == rowIndexTemp)
					return itemRow;
			}

			return null;
		}

		public static string GetValue(int rowIndex, int cellIndex)
		{
			Cell cell = GetCell(rowIndex, cellIndex);
			return GetValue(cell);
		}

		public static string GetValue(Row row, string cellName)
		{
			Cell cell = GetCell(row, cellName);
			return GetValue(cell);
		}

		public static string GetValue(Row row, int cellIndex)
		{
			Cell cell = GetCell(row, cellIndex);
			return GetValue(cell);
		}

		public static string GetValue(Cell cell)
		{
			string value = "";
			if (cell != null)
			{
				if (sharedStringTable != null && cell.DataType != null && cell.DataType == CellValues.SharedString && int.TryParse(cell.CellValue.Text, out int strId))
					value = sharedStringTable.ChildElements[strId]?.InnerText;
				else
					value = cell.CellValue == null ? cell.InnerText : cell.CellValue.InnerText;
			}

			return value;
		}

		public static IEnumerable<Row> GetRows()
		{
			return sheetData.Elements<Row>();
		}

		public static IEnumerable<Cell> GetCells(int rowIndex)
		{
			Row row = GetRow(rowIndex);
			if (row != null)
				return row.Elements<Cell>();
			else
				return null;
		}

		public static void SetStyle(int rowIndex, int cellIndex, int styleIndex)
		{
			Cell cell = CreateRow(rowIndex, cellIndex);
			if (cell != null)
				cell.StyleIndex = (uint)styleIndex;
		}

		public static void SetStyleColumn(int cellIndex, int styleIndex)
		{
			int sizeRow = RowLength();
			for (int i = 0; i < sizeRow; i++)
			{
				Cell cell = CreateRow(i, cellIndex);
				if (cell != null)
					cell.StyleIndex = (uint)styleIndex;
			}
		}

		public static void SetStyleRow(int rowIndex, int styleIndex)
		{
			int sizeCell = CellLength(rowIndex);
			for (int i = 0; i < sizeCell; i++)
			{
				Cell cell = CreateRow(rowIndex, i);
				if (cell != null)
					cell.StyleIndex = (uint)styleIndex;
			}
		}

		private static int GetMaxCharacterWidth(int cellIndex)
		{
			int maxColWidth = 0;
			int rowIndex = 0;
			foreach (Row row in sheetData.Elements<Row>())
			{
				Cell cell = GetCell(row, GetCellName((int)(uint)row.RowIndex - 1, cellIndex));
				string cellValue = GetValue(cell);
				int cellTextLength = cellValue.Length;

				if (cell != null)
				{
					if (cell.StyleIndex != null)
						cellTextLength += 1;
				}

				if (cellTextLength > maxColWidth)
					maxColWidth = cellTextLength;

				rowIndex++;
			}

			return maxColWidth;
		}

		public static void AutoFitCell(int cellIndex)
		{
			if (columns != null)
			{
				var maxColWidth = GetMaxCharacterWidth(cellIndex);

				double maxWidth = 5;
				double colWidth = Math.Truncate((maxColWidth * maxWidth + 5) / maxWidth * 256) / 256;
				SetColumnWidth(cellIndex, colWidth);
			}
		}

		public static void SetColumnWidth(int cellIndex, double colWidth)
		{
			if (columns != null)
			{
				int idx = 0;
				foreach (var item in columns.Elements<Column>())
				{
					if (idx == cellIndex)
					{
						item.Width = colWidth;
						break;
					}
					idx++;
				}
			}
		}

		public static bool LoadSheet(string nameSheet)
		{
			ZeroSheet();

			if (lSheets == null)
				return false;

			Sheet sheet = null;

			foreach (var item in lSheets)
			{
				if (string.Compare(item.Name, nameSheet, StringComparison.InvariantCultureIgnoreCase) == 0)
				{
					sheet = item;
					break;
				}
			}

			if (sheet != null)
			{
				worksheetPart = (WorksheetPart)workbookpart.GetPartById(sheet.Id);
				sheetData = worksheetPart.Worksheet.Elements<SheetData>().First();
				if (workbookpart.SharedStringTablePart != null)
				{
					SharedStringTablePart sharedStringTablePart = workbookpart.GetPartsOfType<SharedStringTablePart>().First();
					sharedStringTable = sharedStringTablePart.SharedStringTable;
				}

				CreateColumns();

				return true;
			}

			return false;
		}

		public static void CloseFile()
		{
			if (spreadsheetDocument != null)
				spreadsheetDocument.Close();
		}

		public static void SaveFile()
		{
			if (spreadsheetDocument != null)
				spreadsheetDocument.Save();
		}

		public static void OpenXlsx(string path, bool _isEditable = true)
		{
			Zero();
			isEditable = _isEditable;
			spreadsheetDocument = SpreadsheetDocument.Open(path, isEditable);
			workbookpart = spreadsheetDocument.WorkbookPart;
			lSheets = new List<Sheet>(workbookpart.Workbook.Descendants<Sheet>());
			sheets = workbookpart.Workbook.Sheets;
			CreateStyleSheet();
		}

		public static void CreateXlsx(string path, List<string> lSheetsName)
		{
			Zero();

			if (lSheetsName == null)
				lSheetsName = new List<string>();

			if (lSheetsName.Count == 0)
				lSheetsName.Add("Лист1");

			spreadsheetDocument = SpreadsheetDocument.Create(path, SpreadsheetDocumentType.Workbook);
			workbookpart = spreadsheetDocument.AddWorkbookPart();
			workbookpart.Workbook = new Workbook();

			CreateStyleSheet();
			CreateSheets(lSheetsName);
			workbookpart.Workbook.Save();
			CloseFile();
		}

		private static void CreateSheets(List<string> sheetsName)
		{
			if (workbookpart == null)
				return;

			if (workbookpart.Workbook == null)
				return;

			sheets = workbookpart.Workbook.AppendChild(new Sheets());
			uint index = 1;
			foreach (string name in sheetsName)
			{
				ZeroSheet();
				worksheetPart = workbookpart.AddNewPart<WorksheetPart>();
				worksheetPart.Worksheet = new Worksheet();
				sheetData = new SheetData();
				CreateColumns();
				worksheetPart.Worksheet.Append(sheetData);
				sheets.Append(new Sheet()
				{
					Id = workbookpart.GetIdOfPart(worksheetPart),
					SheetId = index,
					Name = name
				});

				index++;
			}
		}

		private static void CreateColumns()
		{
			if (worksheetPart.Worksheet == null)
				return;

			foreach (var item in worksheetPart.Worksheet.Elements())
			{
				if (item is Columns)
				{
					columns = item as Columns;
					break;
				}
			}

			if (isEditable)
			{
				if (columns == null)
				{
					columns = new Columns();
					worksheetPart.Worksheet.Append(columns);
				}


				int sizeColumn = columns.Count();
				double maxWidth = 8;
				for (int i = sizeColumn; i < alphabetExcel.Count; i++)
				{
					Column col = new Column() { BestFit = true, Min = (uint)(i + 1), Max = (uint)(i + 1), CustomWidth = true, Width = maxWidth };
					columns.Append(col);
				}
			}
		}

		private static void CreateStyleSheet()
		{
			WorkbookStylesPart stylesPart = null;
			if (workbookpart.WorkbookStylesPart != null)
			{
				stylesPart = workbookpart.WorkbookStylesPart;
				stylesheet = workbookpart.WorkbookStylesPart.Stylesheet;
				return;
			}
			else if (isEditable)
				stylesPart = workbookpart.AddNewPart<WorkbookStylesPart>();

			if (!isEditable)
				return;

			stylesheet = new Stylesheet();

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

			stylesPart.Stylesheet = stylesheet;
			stylesPart.Stylesheet.Save();
		}

		private static ForegroundColor TranslateForeground(System.Drawing.Color fillColor)
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
	}
}
