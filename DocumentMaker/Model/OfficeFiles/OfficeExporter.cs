using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentMaker.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace DocumentMaker.Model.OfficeFiles
{
	internal class OfficeExporter
	{
		public static string TempFolderName => "export-temp";

		private XmlDocument xml;
		private string rowPartXml;
		private readonly List<KeyValuePair<string, string>> notMovedFiles;

		public OfficeExporter()
		{
			xml = new XmlDocument();
			rowPartXml = "";
			notMovedFiles = new List<KeyValuePair<string, string>>();

			TemplateLoaded = false;
		}

		public bool TemplateLoaded { get; private set; }
		public bool HasNoMovedFiles => notMovedFiles.Count > 0;

		public void Clear()
		{
			xml = new XmlDocument();
			rowPartXml = "";

			TemplateLoaded = false;
		}

		public string ExportWordTemplate(string name, bool isExportRework)
		{
			if (!TemplateLoaded)
			{
				ExportTemplate(name, isExportRework, out string nearFullname);

				xml.LoadXml(GetXmlFromWordFile(nearFullname));
				if (FindRowPart())
				{
					TemplateLoaded = true;
					return nearFullname;
				}
				else
				{
					File.Delete(nearFullname);
				}
			}
			return null;
		}

		public void FillWordGeneralData(DocumentGeneralData data)
		{
			if (!TemplateLoaded) throw new Exception("Template file didn't loaded!");

			FillInnerPropertiesData(data);
			string str = xml.InnerXml;
			FillPropertiesData(data, ref str);
			xml.InnerXml = str;
		}

		public void FillWordTableData(DocumentTableData data)
		{
			if (!TemplateLoaded) throw new Exception("Template file didn't loaded!");

			List<string> xmlRows = new List<string>();
			foreach (DocumentTableRowData row in data)
			{
				string xmlTemplate = rowPartXml;
				FillPropertiesData(row, ref xmlTemplate);
				xmlRows.Add(xmlTemplate);
			}

			AddXmlRows(xmlRows);
		}

		public void FillPropertiesData(object data, ref string xmlStr)
		{
			PropertyInfo[] properties = data.GetType().GetProperties();
			foreach (PropertyInfo property in properties)
			{
				string replaceStr = '[' + property.Name + ']';
				string value = (string)property.GetValue(data);

				xmlStr = xmlStr.Replace(replaceStr, value);
			}
		}

		public void FillInnerPropertiesData(object data)
		{
			PropertyInfo[] properties = data.GetType().GetProperties();
			foreach(PropertyInfo property in properties)
			{
				MethodInfo setMethod = property.GetSetMethod();
				if (setMethod != null)
				{
					foreach(PropertyInfo innerProperty in properties)
					{
						if(innerProperty != property)
						{
							string replaceStr = '[' + innerProperty.Name + ']';
							string value = (string)innerProperty.GetValue(data);

							setMethod.Invoke(data, new object[] { ((string)property.GetValue(data)).Replace(replaceStr, value) });
						}
					}
				}
			}
		}

		public void SaveWordContent(string nearFullname)
		{
			SetXmlToWordFile(nearFullname);
		}

		public void SaveTemplate(DocumentGeneralData data, string path, string nearFullname, string templateName)
		{
			FillPropertiesData(data, ref templateName);

			string fullpath = Path.Combine(path, templateName);
			if (File.Exists(fullpath))
			{
				notMovedFiles.Add(new KeyValuePair<string, string>(nearFullname, fullpath));
			}
			else
			{
				File.Move(nearFullname, fullpath);
			}
		}

		public IEnumerable<KeyValuePair<string, string>> GetNoMovedFiles()
		{
			return notMovedFiles;
		}

		public void ReplaceCreatedFiles()
		{
			List<KeyValuePair<string, string>> files = new List<KeyValuePair<string, string>>(notMovedFiles);
			notMovedFiles.Clear();

			foreach (KeyValuePair<string, string> file in files)
			{
				try
				{
					File.Delete(file.Value);
					File.Move(file.Key, file.Value);
				}
				catch
				{
					notMovedFiles.Add(file);
				}
			}
		}

		public void RemoveTemplates()
		{
			string tempFolderPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), TempFolderName);
			if (Directory.Exists(tempFolderPath))
			{
				Directory.Delete(tempFolderPath, true);
			}

			notMovedFiles.Clear();
		}

		public void ExportExcelTemplate(string name)
		{
			ExportTemplate(name, false, out _);
		}

		public void FillExcelTableData(string name, DocumentTableData data)
		{
			string filepath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), name);
			using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(filepath, true))
			{
				WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart;
				WorksheetPart worksheetPart = workbookPart.WorksheetParts.First();
				SheetData sheetData = worksheetPart.Worksheet.Elements<SheetData>().First();

				xml.LoadXml(sheetData.OuterXml);
				FillExcelData(data);
				sheetData.InnerXml = xml.DocumentElement.InnerXml;

				FlushCachedValues(spreadsheetDocument);
				spreadsheetDocument.Save();
				spreadsheetDocument.Close();
			}
		}

		private string GetXmlFromWordFile(string fullpath)
		{
			string res = "";

			using (WordprocessingDocument doc = WordprocessingDocument.Open(fullpath, true))
			{
				Body docBody = doc.MainDocumentPart.Document.Body;
				res = docBody.OuterXml;
				doc.Close();
			}

			return res;
		}

		private void SetXmlToWordFile(string fullpath)
		{
			using (WordprocessingDocument doc = WordprocessingDocument.Open(fullpath, true))
			{
				Body docBody = doc.MainDocumentPart.Document.Body;
				docBody.InnerXml = xml.DocumentElement.InnerXml;
				doc.Save();
				doc.Close();
			}
		}

		private bool FindRowPart()
		{
			XmlNodeList tables = xml.DocumentElement.GetElementsByTagName(OfficeStrings.WordTableTagName);
			if (tables.Count <= 0) return false;

			XmlElement table = (XmlElement)tables[0];
			XmlNodeList rows = table.GetElementsByTagName(OfficeStrings.WordTableRowTagName);
			if (rows.Count <= 0) return false;

			XmlElement row = (XmlElement)rows[rows.Count - 1];
			rowPartXml = row.OuterXml;
			table.InnerXml = table.InnerXml.Replace(rowPartXml, "");

			return true;
		}

		private void AddXmlRows(IEnumerable<string> xmlRows)
		{
			XmlNodeList tables = xml.DocumentElement.GetElementsByTagName(OfficeStrings.WordTableTagName);
			XmlElement table = (XmlElement)tables[0];

			foreach (string row in xmlRows)
			{
				table.InnerXml += row;
			}
		}

		private void ExportTemplate(string name, bool isExportRework, out string nearFullname)
		{
			string addictionStr = BackTaskStrings.GetAddictionName(isExportRework);
			string nearPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), TempFolderName);
			nearFullname = Path.Combine(nearPath, BackTaskStrings.GetRandomString() + "_" + addictionStr + name);
			if (File.Exists(nearFullname))
			{
				File.Delete(nearFullname);
			}
			if (!Directory.Exists(nearPath))
			{
				Directory.CreateDirectory(nearPath);
			}

			ResourceUnloader.Unload(name, nearFullname);
		}

		private void FillExcelData(DocumentTableData data)
		{
			XmlElement root = xml.DocumentElement;

			const char startColId = 'B';
			const int startRowId = 2;

			int curValue = 0;

			foreach (DocumentTableRowData rowData in data)
			{
				string curRowId = (startRowId + curValue).ToString();
				string curCellId = startColId.ToString() + curRowId;

				XmlNodeList rows = root.GetElementsByTagName(OfficeStrings.ExcelRowTagName);
				XmlElement curRow = GetXmlElementById(rows, OfficeStrings.ExcelArgIdName, curRowId);

				if (curRow != null)
				{
					XmlNodeList cells = curRow.GetElementsByTagName(OfficeStrings.ExcelCellTagName);
					XmlElement curCell = GetXmlElementById(cells, OfficeStrings.ExcelArgIdName, curCellId);

					if (curCell != null)
					{
						XmlElement valueTag = GetFirstXmlElement(curCell.GetElementsByTagName(OfficeStrings.ExcelCellValueTagName));
						if (valueTag != null)
						{
							valueTag.InnerXml = rowData.GetSpentTime();
						}
						else
						{
							curCell.InnerXml = "<" + OfficeStrings.ExcelCellValueTagName + ">" + rowData.GetSpentTime() + "</" + OfficeStrings.ExcelCellValueTagName + ">";
						}
					}
					else
					{
						throw new Exception("End of excel template!");
					}
				}
				else
				{
					throw new Exception("End of excel template!");
				}

				++curValue;
			}
		}

		public static XmlElement GetXmlElementById(XmlNodeList nodes, string argName, string id)
		{
			foreach (XmlElement element in nodes)
			{
				if (element.HasAttribute(argName) && element.GetAttribute(argName) == id)
				{
					return element;
				}
			}

			return null;
		}

		public static XmlElement GetFirstXmlElement(XmlNodeList nodes)
		{
			if (nodes.Count > 0)
				return (XmlElement)nodes[0];

			return null;
		}

		private void FlushCachedValues(SpreadsheetDocument doc)
		{
			doc.WorkbookPart.WorksheetParts
				.SelectMany(part => part.Worksheet.Elements<SheetData>())
				.SelectMany(data => data.Elements<Row>())
				.SelectMany(row => row.Elements<Cell>())
				.Where(cell => cell.CellFormula != null)
				.Where(cell => cell.CellValue != null)
				.ToList()
				.ForEach(cell => cell.CellValue.Remove());
		}
	}
}
