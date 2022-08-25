using Dml;
using Dml.Model.Session;
using DocumentMaker.Model.Controls;
using DocumentMaker.Model.Files;
using System.Collections.Generic;
using System.Xml;

namespace DocumentMaker.Model.Session
{
	internal class DmxLoader : XmlLoader
	{
		public DmxLoader() : base() { }

		public void SetLoadedDmxFiles(ObservableRangeCollection<DmxFile> dmxFiles)
		{
			List<DmxFile> tempDmxFiles = new List<DmxFile>();
			XmlElement root = xml.DocumentElement;
			XmlNodeList nodeList = root.GetElementsByTagName(XmlConfNames.DmxFileNodeName);
			foreach (XmlElement elem in nodeList)
			{
				XmlNodeList fullnames = elem.GetElementsByTagName(XmlConfNames.DmxFileFullNameNodeName);
				if (fullnames.Count > 0)
				{
					XmlElement fullNameNode = fullnames[0] as XmlElement;
					if (fullNameNode != null && fullNameNode.HasAttribute(XmlConfNames.NodeAttributeName))
					{
						DmxFile dmxFile = new DmxFile(fullNameNode.GetAttribute(XmlConfNames.NodeAttributeName));
						SetLoadedProperties(elem, dmxFile);
						List<FullBackDataModel> backDataModels = new List<FullBackDataModel>();
						SetLoadedListProperties(elem, XmlConfNames.DmxFileBackNodeName, backDataModels);
						dmxFile.SetLoadedBackData(backDataModels);
						tempDmxFiles.Add(dmxFile);
					}
				}
			}
			dmxFiles.AddRange(tempDmxFiles);
		}
	}
}
