using Dml.Model.Session;
using Dml.Model.Template;
using System.Collections.Generic;
using System.IO;

namespace Dml.Model.Files
{
	public class DmxFile
	{
		public static string Extension => ".dmx";

		private List<BackDataModel> backDataModels;

		public DmxFile(string path)
		{
			FullName = path;
			Loaded = false;

			TemplateType = DocumentTemplateType.Empty;
			SelectedHuman = null;
			backDataModels = null;
		}

		public string FullName { get; private set; }
		public string Name => Path.GetFileName(FullName);
		public bool Selected { get; set; }
		public bool Loaded { get; private set; }
		public bool ShowFullName { get; set; }

		#region Loaded data

		public DocumentTemplateType TemplateType { get; set; }
		public string SelectedHuman { get; set; }
		public IList<BackDataModel> BackDataModels { get => backDataModels; }

		#endregion

		public override string ToString()
		{
			return ShowFullName ? FullName : Name;
		}

		public void Load()
		{
			XmlLoader loader = new XmlLoader();
			if(loader.TryLoad(FullName))
			{
				loader.SetLoadedProperties(this);

				backDataModels = new List<BackDataModel>();
				loader.SetLoadedBacksProperties(backDataModels);

				Loaded = true;
			}
		}

		public void SetLoadedBackData(IList<BackDataModel> backDataModels)
		{
			this.backDataModels = new List<BackDataModel>(backDataModels);
			Loaded = true;
		}
	}
}
