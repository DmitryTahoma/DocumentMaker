using Dml.Model.Session;
using Dml.Model.Template;
using System.IO;

namespace Dml.Model.Files
{
	public abstract class BaseDmxFile
	{
		public static string Extension => ".dmx";

		public BaseDmxFile(string path)
		{
			FullName = path;
			Loaded = false;

			TemplateType = DocumentTemplateType.Empty;
			SelectedHuman = null;
		}

		public string FullName { get; private set; }
		public string Name => Path.GetFileName(FullName);
		public bool Selected { get; set; }
		public bool Loaded { get; protected set; }
		public bool ShowFullName { get; set; }

		#region Loaded data

		public DocumentTemplateType TemplateType { get; set; }
		public string SelectedHuman { get; set; }

		#endregion

		public override string ToString()
		{
			return ShowFullName ? FullName : Name;
		}

		public virtual void Load()
		{
			XmlLoader loader = new XmlLoader();
			if(loader.TryLoad(FullName))
			{
				loader.SetLoadedProperties(this);

				Loaded = true;
			}
		}
	}
}
