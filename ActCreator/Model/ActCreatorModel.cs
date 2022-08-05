using Dml;
using Dml.Model;
using Dml.Model.Files;
using Dml.Model.Session;
using Dml.Model.Template;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace ActCreator.Model
{
	public class ActCreatorModel
	{
		private readonly ObservableCollection<DocumentTemplate> documentTemplates;
		private ObservableRangeCollection<string> humanFullNameList;

		public ActCreatorModel()
		{
			documentTemplates = new ObservableCollection<DocumentTemplate>
			{
				new DocumentTemplate { Name = "Скриптувальник", Type = DocumentTemplateType.Scripter, },
				new DocumentTemplate { Name = "Різник", Type = DocumentTemplateType.Cutter, },
				new DocumentTemplate { Name = "Художник", Type = DocumentTemplateType.Painter, },
				new DocumentTemplate { Name = "Моделлер", Type = DocumentTemplateType.Modeller, },
			};
			humanFullNameList = new ObservableRangeCollection<string>();
		}

		public DocumentTemplateType TemplateType { get; set; }
		public string SelectedHuman { get; set; }
		public IList<DocumentTemplate> DocumentTemplatesList => documentTemplates;
		public IList<string> HumanFullNameList => humanFullNameList;

		public void Save(string path, IEnumerable<BackDataModel> backModels)
		{
			XmlSaver saver = new XmlSaver();
			saver.AppendAllProperties(this);

			foreach (BackDataModel backDataModel in backModels)
			{
				saver.CreateBackNode();
				saver.AppendAllBackProperties(backDataModel);
				saver.PushBackNode();
			}

			saver.Save(path);
		}

		public void Load(string path, out List<BackDataModel> backModels)
		{
			backModels = new List<BackDataModel>();

			XmlLoader loader = new XmlLoader();
			if (loader.TryLoad(path))
			{
				loader.SetLoadedProperties(this);
				loader.SetLoadedBacksProperties(backModels);
			}
		}

		public void LoadHumans(string path)
		{
			XmlLoader loader = new XmlLoader();
			if(loader.TryLoad(path))
			{
				loader.SetLoadedHumans(humanFullNameList);
			}
		}

		public string GetDmxFileName()
		{
			return SelectedHuman + DmxFile.Extension;
		}

		public string GetDmxFileName(string path)
		{
			return Path.Combine(path, GetDmxFileName());
		}

		public bool DmxExists(string path)
		{
			return File.Exists(GetDmxFileName(path));
		}

		public void ExportDmx(string path, IEnumerable<BackDataModel> backModels)
		{
			XmlSaver saver = new XmlSaver();
			saver.AppendAllProperties(this);

			foreach(BackDataModel backDataModel in backModels)
			{
				saver.CreateBackNode();
				saver.AppendAllBackProperties(backDataModel);
				saver.PushBackNode();
			}

			saver.Save(GetDmxFileName(path));
		}
	}
}
