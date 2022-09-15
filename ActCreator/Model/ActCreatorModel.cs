using Dml;
using Dml.Model.Back;
using Dml.Model.Files;
using Dml.Model.Session;
using Dml.Model.Session.Attributes;
using Dml.Model.Template;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace ActCreator.Model
{
	public class ActCreatorModel
	{
		private readonly ObservableCollection<DocumentTemplate> documentTemplates;
		private readonly ObservableRangeCollection<string> humanFullNameList;
		private readonly List<GameObject> gameNameList;

		public ActCreatorModel()
		{
			documentTemplates = new ObservableCollection<DocumentTemplate>
			{
				new DocumentTemplate { Name = "Скриптувальник", Type = DocumentTemplateType.Scripter, },
				new DocumentTemplate { Name = "Технічний дизайнер", Type = DocumentTemplateType.Cutter, },
				new DocumentTemplate { Name = "Художник", Type = DocumentTemplateType.Painter, },
				new DocumentTemplate { Name = "Моделлер", Type = DocumentTemplateType.Modeller, },
			};
			humanFullNameList = new ObservableRangeCollection<string>();
			gameNameList = new List<GameObject>();

			OpenedFileName = null;
		}

		#region Window settings

		[IsNotDmxContent]
		public double WindowTop { get; set; } = 0;
		[IsNotDmxContent]
		public double WindowLeft { get; set; } = 0;
		[IsNotDmxContent]
		public double WindowHeight { get; set; } = 700;
		[IsNotDmxContent]
		public double WindowWidth { get; set; } = 1000;
		[IsNotDmxContent]
		public WindowState WindowState { get; set; } = WindowState.Maximized;

		#endregion

		public string OpenedFileName { get; set; }

		public DocumentTemplateType TemplateType { get; set; } = DocumentTemplateType.Empty;
		public string SelectedHuman { get; set; }
		public IList<DocumentTemplate> DocumentTemplatesList => documentTemplates;
		public IList<string> HumanFullNameList => humanFullNameList;
		public IList<GameObject> GameNameList => gameNameList;

		[IsNotDmxContent]
		public bool HaveUnsavedChanges { get; set; }

		public void Save(string path, IEnumerable<ShortBackDataModel> backModels)
		{
			XmlSaver saver = new XmlSaver();
			saver.AppendAllProperties(this);

			foreach (ShortBackDataModel backDataModel in backModels)
			{
				saver.CreateBackNode();
				saver.AppendAllBackProperties(backDataModel);
				saver.PushBackNode();
			}

			saver.Save(path);
		}

		public void Load(string path, out List<ShortBackDataModel> backModels)
		{
			backModels = new List<ShortBackDataModel>();

			XmlLoader loader = new XmlLoader();
			if (loader.TryLoad(path))
			{
				loader.SetLoadedProperties(this);
				loader.SetLoadedListProperties(backModels);
			}
		}

		public void LoadHumans(string path)
		{
			XmlLoader loader = new XmlLoader();
			if (loader.TryLoad(path))
			{
				loader.SetLoadedHumans(humanFullNameList);
			}
		}

		public void LoadGameNames(string path)
		{
			XmlLoader loader = new XmlLoader();
			if (loader.TryLoad(path))
			{
				loader.SetLoadedGameNames(gameNameList);
			}
		}

		public string GetDmxFileName()
		{
			return SelectedHuman + BaseDmxFile.Extension;
		}

		public string GetDmxFileName(string path)
		{
			return Path.Combine(path, GetDmxFileName());
		}

		public bool DmxExists(string path)
		{
			return File.Exists(GetDmxFileName(path));
		}

		public void ExportDmx(string path, IEnumerable<ShortBackDataModel> backModels)
		{
			XmlSaver saver = new XmlSaver();
			saver.AppendAllProperties(this, true);

			foreach (ShortBackDataModel backDataModel in backModels)
			{
				saver.CreateBackNode();
				saver.AppendAllBackProperties(backDataModel);
				saver.PushBackNode();
			}

			saver.Save(path);
		}

		public void CloseFile()
		{
			OpenedFileName = null;
		}

		public void CreateFile()
		{
			OpenedFileName = "new";
		}
	}
}
