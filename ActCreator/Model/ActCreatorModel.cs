using Dml.Model;
using Dml.Model.Session;
using Dml.Model.Template;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ActCreator.Model
{
	public class ActCreatorModel
	{
		private readonly ObservableCollection<DocumentTemplate> documentTemplates;

		public ActCreatorModel()
		{
			documentTemplates = new ObservableCollection<DocumentTemplate>
			{
				new DocumentTemplate { Name = "Скриптувальник", Type = DocumentTemplateType.Scripter, },
				new DocumentTemplate { Name = "Різник", Type = DocumentTemplateType.Cutter, },
				new DocumentTemplate { Name = "Художник", Type = DocumentTemplateType.Painter, },
				new DocumentTemplate { Name = "Моделлер", Type = DocumentTemplateType.Modeller, },
			};
		}

		public DocumentTemplateType TemplateType { get; set; }
		public IList<DocumentTemplate> DocumentTemplatesList => documentTemplates;

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
	}
}
