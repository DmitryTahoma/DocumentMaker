using ActCreator.Controller.Controls;
using ActCreator.Model;
using Dml.Controller.Validation;
using Dml.Model.Back;
using Dml.Model.Files;
using Dml.Model.Template;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace ActCreator.Controller
{
	public class MainWindowController
	{
		private const string saveFile = "session.xml";
		private const string humansFile = "employees.xml";
		private const string gameNamesFile = "projectnames.xml";

		private readonly ActCreatorModel model;
		private readonly StringValidator validator;

		private readonly string openLaterFilename = null;

		public MainWindowController(string[] args)
		{
			model = new ActCreatorModel();
			validator = new StringValidator();
			BackDataControllers = new List<ShortBackDataController>();

			FileInfo file;
			foreach(string name in args)
			{
				file = new FileInfo(name);
				if(file.Extension == BaseDmxFile.Extension)
				{
					openLaterFilename = name;
					break;
				}
			}
		}

		#region Window settings

		public double WindowTop { get => model.WindowTop; set => model.WindowTop = value; }
		public double WindowLeft { get => model.WindowLeft; set => model.WindowLeft = value; }
		public double WindowHeight { get => model.WindowHeight; set => model.WindowHeight = value; }
		public double WindowWidth { get => model.WindowWidth; set => model.WindowWidth = value; }
		public WindowState WindowState { get => model.WindowState; set => model.WindowState = value; }

		#endregion

		public DocumentTemplateType TemplateType { get => model.TemplateType; set => model.TemplateType = value; }
		public string SelectedHuman { get => model.SelectedHuman; set => model.SelectedHuman = value; }
		public List<ShortBackDataController> BackDataControllers { get; set; }
		public IList<DocumentTemplate> DocumentTemplatesList => model.DocumentTemplatesList;
		public IList<string> HumanFullNameList => model.HumanFullNameList;
		public IList<BackDataType> CurrentBackDataTypesList => DocumentTemplatesList.FirstOrDefault(x => x.Type == TemplateType)?.DataTypesList;
		public IList<GameObject> GameNameList => model.GameNameList;
		public bool HaveOpenLaterFiles => openLaterFilename != null;
		public string OpenedFile => model.OpenedFileName;
		public bool IsOpenedFile => OpenedFile != null;
		public bool HaveUnsavedChanges { get => model.HaveUnsavedChanges; set => model.HaveUnsavedChanges = value; }
		public bool IsOpeningFile { get; set; } = false;
		public bool IsLoadingLastSession { get; set; } = false;
		public bool IsNewFile => model.IsNewFile;

		public void Save()
		{
			string fullpath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), saveFile);

			List<ShortBackDataModel> backDataModels = new List<ShortBackDataModel>();
			foreach (ShortBackDataController controller in BackDataControllers)
			{
				backDataModels.Add(controller.GetModel());
			}

			model.Save(fullpath, backDataModels);
		}

		public void Load()
		{
			string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			string saveFullpath = Path.Combine(path, saveFile);
			string humansFullpath = Path.Combine(path, humansFile);
			string gameNamesFullpath = Path.Combine(path, gameNamesFile);

			model.Load(saveFullpath, out List<ShortBackDataModel> backModels);
			model.LoadHumans(humansFullpath);
			foreach (ShortBackDataModel model in backModels)
			{
				BackDataControllers.Add(new ShortBackDataController(model));
			}
			model.LoadGameNames(gameNamesFullpath);
		}

		public bool DmxExists(string path) => model.DmxExists(path);

		public string GetDmxFileName() => model.GetDmxFileName();

		public void ExportDmx(string path)
		{
			List<ShortBackDataModel> backDataModels = new List<ShortBackDataModel>();
			foreach (ShortBackDataController controller in BackDataControllers)
			{
				backDataModels.Add(controller.GetModel());
			}

			model.ExportDmx(path, backDataModels);
		}

		public bool Validate(out string errorText)
		{
			errorText = "";
			bool isValidGeneralData = false;

			if (TemplateType == DocumentTemplateType.Empty)
				errorText = "Оберіть тип шаблону.";
			else if (!validator.IsFullName(SelectedHuman))
				errorText = "Не правильно введене ПІБ.";
			else
				isValidGeneralData = true;

			if (isValidGeneralData)
			{
				foreach (ShortBackDataController backDataController in BackDataControllers)
				{
					errorText = string.Empty;
					if (!backDataController.Validate(ref errorText))
					{
						return false;
					}
				}

				return true;
			}

			return false;
		}

		public string GetOpenLaterFile()
		{
			return openLaterFilename;
		}

		public bool OpenFile(string filename)
		{
			if (!filename.EndsWith(BaseDmxFile.Extension))
			{
				return false;
			}
			else
			{
				model.Load(filename, out List<ShortBackDataModel> backModels);
				BackDataControllers = new List<ShortBackDataController>(backModels.Select(x => new ShortBackDataController(x)));
				model.SetOpenedFileName(filename);
				return true;
			}
		}

		public void CloseFile()
		{
			model.CloseFile();
		}

		public void CreateFile()
		{
			model.CreateFile();
		}

		public bool HaveUnsavedChangesAtAll()
		{
			if (HaveUnsavedChanges)
				return true;

			foreach (ShortBackDataController backDataController in BackDataControllers)
			{
				if (backDataController.HaveUnsavedChanges)
					return true;
			}
			return false;
		}

		public void ResetHaveUnsavedChanges()
		{
			HaveUnsavedChanges = false;

			foreach (ShortBackDataController backDataController in BackDataControllers)
			{
				backDataController.HaveUnsavedChanges = false;
			}
		}
	}
}
