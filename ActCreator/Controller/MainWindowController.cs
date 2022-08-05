using ActCreator.Model;
using Dml.Controller;
using Dml.Controller.Validation;
using Dml.Model;
using Dml.Model.Template;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace ActCreator.Controller
{
	public class MainWindowController
	{
		private const string saveFile = "session.xml";
		private const string humansFile = "employees.xml";

		private readonly ActCreatorModel model;
		private readonly StringValidator validator;

		public MainWindowController()
		{
			model = new ActCreatorModel();
			validator = new StringValidator();
			BackDataControllers = new List<BackDataController>();
		}

		public DocumentTemplateType TemplateType { get => model.TemplateType; set => model.TemplateType = value; }
		public string SelectedHuman { get => model.SelectedHuman; set => model.SelectedHuman = value; }
		public List<BackDataController> BackDataControllers { get; set; }
		public IList<DocumentTemplate> DocumentTemplatesList => model.DocumentTemplatesList;
		public IList<string> HumanFullNameList => model.HumanFullNameList;

		public void Save()
		{
			string fullpath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), saveFile);

			List<BackDataModel> backDataModels = new List<BackDataModel>();
			foreach (BackDataController controller in BackDataControllers)
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

			model.Load(saveFullpath, out List<BackDataModel> backModels);
			model.LoadHumans(humansFullpath);
			foreach (BackDataModel model in backModels)
			{
				BackDataControllers.Add(new BackDataController(model));
			}
		}

		public bool DmxExists(string path) => model.DmxExists(path);

		public void ExportDmx(string path)
		{
			List<BackDataModel> backDataModels = new List<BackDataModel>();
			foreach(BackDataController controller in BackDataControllers)
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
				foreach (BackDataController backDataController in BackDataControllers)
				{
					if (!backDataController.Validate(ref errorText))
					{
						return false;
					}
				}

				return true;
			}

			return false;
		}
	}
}
