﻿using Dml.Model.Session.Attributes;
using System;
using System.Reflection;
using System.Windows.Documents;
using System.Xml;

namespace Dml.Model.Session
{
	public class XmlSaver
	{
		protected readonly XmlDocument xml;
		protected readonly XmlNode rootNode;
		protected XmlNode backNode;

		public XmlSaver()
		{
			xml = new XmlDocument();
			rootNode = xml.CreateElement(XmlConfNames.RootNodeName);
			xml.AppendChild(rootNode);
			backNode = null;
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

		public void Save(string path)
		{
			xml.Save(path);
		}

		public void AppendAllProperties(object obj, bool isOnlyDmxContent = false)
		{
			AppendAllProperties(rootNode, obj, isOnlyDmxContent);
		}

		public void AppendAllBackProperties(object obj)
		{
			AppendAllProperties(backNode, obj);
		}

		protected void AppendAllProperties(XmlNode root, object obj, bool isOnlyDmxContent = false)
		{
			PropertyInfo[] properties = obj.GetType().GetProperties();
			foreach (PropertyInfo prop in properties)
			{
				if (prop.CanWrite
					&& (prop.SetMethod.Attributes & MethodAttributes.Public) == MethodAttributes.Public
					&& Attribute.GetCustomAttribute(prop, typeof(IsNotSavingContentAttribute)) == null
					&& (!isOnlyDmxContent || (Attribute.GetCustomAttribute(prop, typeof(IsNotDmxContentAttribute)) == null)))
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

		protected void AppendTagWithValue(XmlNode root, string name, string value, string description = null)
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
