using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace DocxToXml
{
	internal class Program
	{
		[STAThread]
		private static void Main(string[] args)
		{
			try
			{
				OpenFileDialog openDialog = new OpenFileDialog { Multiselect = true, Filter = "xml files (*.xml)|*.xml|docx files (*.docx)|*.docx" };
				if (openDialog.ShowDialog() == DialogResult.OK && openDialog.FileNames.Length > 0)
				{
					foreach (string filename in openDialog.FileNames)
					{
						if (File.Exists(filename))
						{
							if (filename.EndsWith(".docx"))
							{
								string newFileName = filename.Substring(0, filename.Length - 5) + ".xml";
								string xml = GetXmlFromDocxFile(filename);

								using (FileStream stream = new FileStream(newFileName, FileMode.OpenOrCreate))
								{
									using (StreamWriter writer = new StreamWriter(stream))
									{
										writer.Write(xml);
									}
								}
							}
							else
							{
								string newFileName = filename.Substring(0, filename.Length - 4) + ".docx";
								XmlDocument xml = new XmlDocument();
								xml.Load(filename);

								using (WordprocessingDocument doc = WordprocessingDocument.Open(newFileName, true))
								{
									Body docBody = doc.MainDocumentPart.Document.Body;
									docBody.InnerXml = xml.DocumentElement.InnerXml;
									doc.Save();
									doc.Clone();
								}
							}
						}
					}
				}
				MessageBox.Show("Done!", "DocxToXml", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			catch (Exception e)
			{
				MessageBox.Show(e.ToString(), "DocxToXml", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private static string GetXmlFromDocxFile(string fullpath)
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
	}
}
