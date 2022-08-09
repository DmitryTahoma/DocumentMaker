using Dml.Controller.Validation;
using Dml.Model.Files;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace Dml.Model.Session
{
	public class XmlLoader
	{
		private readonly XmlDocument xml;

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
			SetLoadedBacksProperties(xml.DocumentElement, XmlConfNames.BackNodeName, backModels);
		}

		private void SetLoadedBacksProperties(XmlElement root, string backNodeName, List<BackDataModel> backModels)
		{
			backModels.Clear();

			XmlNodeList nodeList = root.GetElementsByTagName(backNodeName);
			foreach (XmlElement elem in nodeList)
			{
				BackDataModel model = new BackDataModel();
				SetLoadedProperties(elem, model);
				backModels.Add(model);
			}
		}

		public void SetLoadedHumans(ObservableRangeCollection<string> humans)
		{
			List<string> tempHumans = new List<string>();

			XmlElement root = xml.DocumentElement;
			XmlNodeList nodeList = root.GetElementsByTagName(XmlConfNames.HumanNodeName);
			foreach(XmlElement elem in nodeList)
			{
				tempHumans.Add(StringValidator.Trim(elem.InnerText));
			}

			humans.Clear();
			humans.AddRange(tempHumans.OrderBy(x => x));
		}

		public void SetLoadedDmxFiles(ObservableRangeCollection<DmxFile> dmxFiles)
		{
			List<DmxFile> tempDmxFiles = new List<DmxFile>();
			XmlElement root = xml.DocumentElement;
			XmlNodeList nodeList = root.GetElementsByTagName(XmlConfNames.DmxFileNodeName);
			foreach(XmlElement elem in nodeList)
			{
				XmlNodeList fullnames = elem.GetElementsByTagName(XmlConfNames.DmxFileFullNameNodeName);
				if(fullnames.Count > 0)
				{
					XmlElement fullNameNode = fullnames[0] as XmlElement;
					if(fullNameNode != null && fullNameNode.HasAttribute(XmlConfNames.NodeAttributeName))
					{
						DmxFile dmxFile = new DmxFile(fullNameNode.GetAttribute(XmlConfNames.NodeAttributeName));
						SetLoadedProperties(elem, dmxFile);
						List<BackDataModel> backDataModels = new List<BackDataModel>();
						SetLoadedBacksProperties(elem, XmlConfNames.DmxFileBackNodeName, backDataModels);
						dmxFile.SetLoadedBackData(backDataModels);
						tempDmxFiles.Add(dmxFile);
					}
				}
			}
			dmxFiles.AddRange(tempDmxFiles);
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

					if (property.CanWrite && (property.SetMethod.Attributes & MethodAttributes.Public) == MethodAttributes.Public)
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
