using Dml.Model.Files;
using System.Reflection;
using System.Xml;

namespace Dml.Model.Session
{
	public class XmlSaver
	{
		private readonly XmlDocument xml;
		private readonly XmlNode rootNode;
		private XmlNode backNode;
		private XmlNode dmxFileNode;

		public XmlSaver()
		{
			xml = new XmlDocument();
			rootNode = xml.CreateElement(XmlConfNames.RootNodeName);
			xml.AppendChild(rootNode);
			backNode = null;
			dmxFileNode = null;
		}

		public void AppendTagWithValue(string name, string value, string description = null)
		{
			AppendTagWithValue(rootNode, name, value, description);
		}

		public void CreateBackNode()
		{
			backNode = xml.CreateElement(XmlConfNames.BackNodeName);
		}

		public void AppendTagWithValueToBack(string name, string value, string description = null)
		{
			AppendTagWithValue(backNode, name, value, description);
		}

		public void PushBackNode()
		{
			rootNode.AppendChild(backNode);
			backNode = null;
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

		public void Save(string path)
		{
			xml.Save(path);
		}

		public void AppendAllProperties(object obj)
		{
			AppendAllProperties(rootNode, obj);
		}

		public void AppendAllBackProperties(object obj)
		{
			AppendAllProperties(backNode, obj);
		}

		public void AppendAllDmxFileProperties(DmxFile obj)
		{
			AppendTagWithValue(dmxFileNode, XmlConfNames.DmxFileFullNameNodeName, obj.FullName);
			AppendAllProperties(dmxFileNode, obj);

			foreach(BackDataModel backData in obj.BackDataModels)
			{
				backNode = xml.CreateElement(XmlConfNames.DmxFileBackNodeName);
				AppendAllProperties(backNode, backData);
				dmxFileNode.AppendChild(backNode);
				backNode = null;
			}
		}

		private void AppendAllProperties(XmlNode root, object obj)
		{
			PropertyInfo[] properties = obj.GetType().GetProperties();
			foreach (PropertyInfo prop in properties)
			{
				if (prop.CanWrite && (prop.SetMethod.Attributes & MethodAttributes.Public) == MethodAttributes.Public)
				{
					object value = prop.GetValue(obj);
					if (value != null)
					{
						if (value.GetType().IsEnum)
						{
							AppendTagWithValue(root, prop.Name, ((int)value).ToString(), value.ToString());
						}
						else
						{
							AppendTagWithValue(root, prop.Name, value.ToString());
						}
					}
				}
			}
		}

		private void AppendTagWithValue(XmlNode root, string name, string value, string description = null)
		{
			XmlNode node = xml.CreateElement(name);

			XmlAttribute attribute = xml.CreateAttribute(XmlConfNames.NodeAttributeName);
			attribute.Value = value;
			node.Attributes.Append(attribute);

			if (description != null)
			{
				attribute = xml.CreateAttribute(XmlConfNames.NodeAttributeDescriptionName);
				attribute.Value = description;
				node.Attributes.Append(attribute);
			}

			root.AppendChild(node);
		}
	}
}
