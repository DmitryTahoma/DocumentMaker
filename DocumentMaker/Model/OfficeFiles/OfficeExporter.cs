using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentMaker.Resources;
using System;
using System.IO;
using System.Reflection;
using System.Xml;

namespace DocumentMaker.Model.OfficeFiles
{
    class OfficeExporter
    {
        private XmlDocument xml;
        private string rowPartXml;

        public OfficeExporter()
        {
            xml = new XmlDocument();
            rowPartXml = "";

            TemplateLoaded = false;
        }

        public bool TemplateLoaded { get; private set; }

        public void ExportWordTemplate(string name)
        {
            if (!TemplateLoaded)
            {
                string nearPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string nearFullname = Path.Combine(nearPath, name);
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

            PropertyInfo[] properties = data.GetType().GetProperties();
            foreach (PropertyInfo property in properties)
            {
                string replaceStr = '[' + property.Name + ']';
                string value = (string)property.GetValue(data);

                xml.InnerXml = xml.InnerXml.Replace(replaceStr, value);
            }
        }

        public void SaveTemplate(string path, string name)
        {
            string nearFullname = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), name);
            SetXmlToWordFile(nearFullname);

            File.Move(nearFullname, Path.Combine(path, name));
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
            table.InnerText = table.InnerText.Replace(rowPartXml, "");

            return true;
        }
    }
}
