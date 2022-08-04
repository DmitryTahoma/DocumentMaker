using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;

namespace Dml.Model.Session
{
    public class XmlLoader
    {
        private XmlDocument xml;

        public XmlLoader()
        {
            xml = new XmlDocument();
        }

        public bool TryLoad(string path)
        {
            if (File.Exists(path))
            {
                xml.Load(path);
                return true;
            }

            return false;
        }

        public void SetLoadedProperties(object model)
        {
            SetLoadedProperties(xml.DocumentElement, model);
        }

        public void SetLoadedBacksProperties(List<BackDataModel> backModels)
        {
            backModels.Clear();

            XmlElement root = xml.DocumentElement;

            XmlNodeList nodeList = root.GetElementsByTagName(XmlConfNames.BackNodeName);
            foreach (XmlElement elem in nodeList)
            {
                BackDataModel model = new BackDataModel();
                SetLoadedProperties(elem, model);
                backModels.Add(model);
            }
        }

        private void SetLoadedProperties(XmlElement root, object model)
        {
            PropertyInfo[] properties = model.GetType().GetProperties();
            foreach (PropertyInfo property in properties)
            {
                XmlNodeList nodeList = root.GetElementsByTagName(property.Name);
                if (nodeList.Count > 0 && (nodeList[0] as XmlElement).HasAttribute(XmlConfNames.NodeAttributeName))
                {
                    string value = (nodeList[0] as XmlElement).GetAttribute(XmlConfNames.NodeAttributeName);

                    if (property.CanWrite)
                    {
                        if (property.PropertyType == typeof(string))
                        {
                            property.SetValue(model, value);
                        }
                        else if (property.PropertyType.IsEnum)
                        {
                            if (int.TryParse(value, out int val))
                            {
                                property.SetValue(model, val);
                            }
                        }
                        else
                        {
                            MethodInfo tryParseMethod = property.PropertyType.GetMethod(nameof(int.TryParse),
                                                                                        BindingFlags.Static | BindingFlags.Public,
                                                                                        null,
                                                                                        new Type[] { typeof(string), property.PropertyType.MakeByRefType() },
                                                                                        null);
                            if (tryParseMethod != null)
                            {
                                object[] parameters = new object[] { value, null };
                                if ((bool)tryParseMethod.Invoke(null, parameters))
                                {
                                    property.SetValue(model, parameters[1]);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
