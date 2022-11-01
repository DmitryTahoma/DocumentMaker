using Dml.Model.Session;
using DocumentMakerModelLibrary.Controls;
using DocumentMakerModelLibrary.Files;
using System.Xml;

namespace DocumentMakerModelLibrary.Session
{
	internal class DmxSaver : XmlSaver
	{
		private XmlNode dmxFileNode;

		public DmxSaver() : base()
		{
			dmxFileNode = null;
		}

		public void CreateDmxFileNode()
		{
			dmxFileNode = xml.CreateElement(XmlConfNames.DmxFileNodeName);
		}

		public void PushDmxFileNode()
		{
			rootNode.AppendChild(dmxFileNode);
			dmxFileNode = null;
		}

		public void AppendAllDmxFileProperties(DmxFile obj)
		{
			AppendTagWithValue(dmxFileNode, XmlConfNames.DmxFileFullNameNodeName, obj.FullName);
			AppendAllProperties(dmxFileNode, obj);

			foreach (FullBackDataModel backData in obj.BackDataModels)
			{
				backNode = xml.CreateElement(XmlConfNames.DmxFileBackNodeName);
				AppendAllProperties(backNode, backData);
				dmxFileNode.AppendChild(backNode);
				backNode = null;
			}
		}
	}
}
