using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentMaker.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;

namespace DocumentMaker.Model.OfficeFiles
{
    class OfficeExporter
    {
        private XmlDocument xml;
        private string rowPartXml;
        private List<KeyValuePair<string, string>> notMovedFiles;

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

        public void ExportWordTemplate(string name)
        {
            if (!TemplateLoaded)
            {
                string nearPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string nearFullname = Path.Combine(nearPath, name);
                if (File.Exists(nearFullname))
                {
                    File.Delete(nearFullname);
                }

                ResourceUnloader.Unload(name, nearPath);

                xml.LoadXml(GetXmlFromWordFile(nearFullname));
                if (FindRowPart())
                {
                    TemplateLoaded = true;
                }
                else
                {
                    File.Delete(nearFullname);
                }
            }
        }

        public void FillGeneralData(DocumentGeneralData data)
        {
            if (!TemplateLoaded) throw new Exception("Template file didn't loaded!");

            string str = xml.InnerXml;
            FillPropertiesData(data, ref str);
            xml.InnerXml = str;
        }

        public void FillTableData(DocumentTableData data)
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

        public void SaveTemplate(DocumentGeneralData data, string path, string projectName, string templateName)
        {
            string nearFullname = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), projectName);
            SetXmlToWordFile(nearFullname);
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
            foreach (KeyValuePair<string, string> file in notMovedFiles)
            {
                File.Delete(file.Key);
            }

            notMovedFiles.Clear();
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
                docBody.InnerXml = xml.InnerXml;
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
    }
}
