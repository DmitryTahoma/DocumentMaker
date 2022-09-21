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
		private const string newFileName = "new";

		private readonly ObservableCollection<DocumentTemplate> documentTemplates;
		private readonly ObservableRangeCollection<string> humanFullNameList;
		private readonly List<GameObject> gameNameList;

		private string openedFileName;

		public ActCreatorModel()
		{
			documentTemplates = new ObservableCollection<DocumentTemplate>
			{
				new DocumentTemplate("Скриптувальник", DocumentTemplateType.Scripter),
				new DocumentTemplate("Технічний дизайнер", DocumentTemplateType.Cutter),
				new DocumentTemplate("Художник", DocumentTemplateType.Painter),
				new DocumentTemplate("Моделлер", DocumentTemplateType.Modeller),
				new DocumentTemplate("Тестувальник", DocumentTemplateType.Tester),
				new DocumentTemplate("Програміст", DocumentTemplateType.Programmer),
				new DocumentTemplate("Звукорежисер", DocumentTemplateType.Soundman),
				new DocumentTemplate("Аніматор", DocumentTemplateType.Animator),
				new DocumentTemplate("Перекладач", DocumentTemplateType.Translator),
				new DocumentTemplate("Підтримка", DocumentTemplateType.Support),
			};
			humanFullNameList = new ObservableRangeCollection<string>();
			gameNameList = new List<GameObject>();

			openedFileName = null;
		}

		#region Window settings

		[IsNotDmxContent]
		public string SessionVersion { get; set; }
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

		public string OpenedFileName => openedFileName;

		public DocumentTemplateType TemplateType { get; set; } = DocumentTemplateType.Empty;
		public string SelectedHuman { get; set; }
		public IList<DocumentTemplate> DocumentTemplatesList => documentTemplates;
		public IList<string> HumanFullNameList => humanFullNameList;
		public IList<GameObject> GameNameList => gameNameList;
		public bool IsNewFile => openedFileName == newFileName;

		[IsNotSavingContent]
		public bool HaveUnsavedChanges { get; set; }

		public void Save(string path, IEnumerable<ShortBackDataModel> backModels)
		{
			XmlSaver saver = new XmlSaver();
			SessionVersion = "1.1";
			saver.AppendAllProperties(this);

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

			if (SessionVersion == null)
			{
				backModels.Clear();
				SessionVersion = "1.1";
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
			openedFileName = path;
		}

		public void SetOpenedFileName(string fileName)
		{
			openedFileName = fileName;
		}

		public void CloseFile()
		{
			openedFileName = null;
		}

		public void CreateFile()
		{
			openedFileName = newFileName;
		}
	}
}
