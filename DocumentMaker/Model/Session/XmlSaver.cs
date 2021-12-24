using System.Reflection;
using System.Xml;

namespace DocumentMaker.Model.Session
{
    class XmlSaver
    {
        private const string rootNodeName = "Session";
        private const string backNodeName = "BackData";
        private const string nodeAttributeName = "Value";
        private const string nodeAttributeDescriptionName = "Description";

        private XmlDocument xml;
        private XmlNode rootNode;
        private XmlNode backNode;

        public XmlSaver()
        {
            xml = new XmlDocument();
            rootNode = xml.CreateElement(rootNodeName);
            xml.AppendChild(rootNode);
            backNode = null;
        }

        public void AppendTagWithValue(string name, string value, string description = null)
        {
            AppendTagWithValue(rootNode, name, value, description);
        }

        public void CreateBackNode()
        {
            backNode = xml.CreateElement(backNodeName);
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

        private void AppendAllProperties(XmlNode root, object obj)
        {
            PropertyInfo[] properties = obj.GetType().GetProperties();
            foreach (PropertyInfo prop in properties)
            {
                object value = prop.GetValue(obj);
                if (value != null)
                {
                    if(value.GetType().IsEnum)
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

        private void AppendTagWithValue(XmlNode root, string name, string value, string description = null)
        {
            XmlNode node = xml.CreateElement(name);

            XmlAttribute attribute = xml.CreateAttribute(nodeAttributeName);
            attribute.Value = value;
            node.Attributes.Append(attribute);

            if (description != null)
            {
                attribute = xml.CreateAttribute(nodeAttributeDescriptionName);
                attribute.Value = description;
                node.Attributes.Append(attribute);
            }

            root.AppendChild(node);
        }
    }
}
