using Dml.Controller.Validation;
using Dml.Model.Back;
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
		protected readonly XmlDocument xml;

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

		public void SetLoadedListProperties<T>(List<T> models) where T : class, new()
		{
			SetLoadedListProperties<T>(xml.DocumentElement, XmlConfNames.BackNodeName, models);
		}

		protected void SetLoadedListProperties<T>(XmlElement root, string backNodeName, List<T> models) where T : class, new()
		{
			models.Clear();

			XmlNodeList nodeList = root.GetElementsByTagName(backNodeName);
			foreach (XmlElement elem in nodeList)
			{
				T model = new T();
				SetLoadedProperties(elem, model);
				models.Add(model);
			}
		}

		public void SetLoadedHumans(ObservableRangeCollection<string> humans)
		{
			List<string> tempHumans = new List<string>();

			XmlElement root = xml.DocumentElement;
			XmlNodeList nodeList = root.GetElementsByTagName(XmlConfNames.HumanNodeName);
			foreach (XmlElement elem in nodeList)
			{
				tempHumans.Add(StringValidator.Trim(elem.InnerText));
			}

			humans.Clear();
			humans.AddRange(tempHumans.OrderBy(x => x));
		}

		public void SetLoadedGameNames(List<GameObject> gameNameList)
		{
			List<GameObject> tempGameNames = new List<GameObject>();

			XmlElement root = xml.DocumentElement;
			XmlNodeList nodeList = root.GetElementsByTagName(XmlConfNames.ProjectNameNodeName);
			foreach (XmlElement elem in nodeList)
			{
				uint countEpisodes = 0;
				if (elem.HasAttribute(XmlConfNames.CountEpisodesAttributeName) && uint.TryParse(elem.GetAttribute(XmlConfNames.CountEpisodesAttributeName), out uint c))
				{
					countEpisodes = c;
				}

				GameObject gameObj = new GameObject
				{
					Name = StringValidator.Trim(elem.InnerText)
				};
				gameObj.SetEpisodes(countEpisodes);

				tempGameNames.Add(gameObj);
			}

			gameNameList.AddRange(tempGameNames.OrderBy(x => x.Name));
		}

		protected void SetLoadedProperties(XmlElement root, object model)
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
