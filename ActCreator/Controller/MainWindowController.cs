using ActCreator.Model;
using Dml.Controller;
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

		private readonly DocumentMakerModel model;

		public MainWindowController()
		{
			model = new DocumentMakerModel();
			BackDataControllers = new List<BackDataController>();
		}

		public DocumentTemplateType TemplateType { get => model.TemplateType; set => model.TemplateType = value; }
		public List<BackDataController> BackDataControllers { get; set; }
		public IList<DocumentTemplate> DocumentTemplatesList => model.DocumentTemplatesList;

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
			string fullpath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), saveFile);

			model.Load(fullpath, out List<BackDataModel> backModels);
			foreach (BackDataModel model in backModels)
			{
				BackDataControllers.Add(new BackDataController(model));
			}
		}

		public bool Validate(out string errorText)
		{
			errorText = "";
			bool isValidGeneralData = false;

			if (TemplateType == DocumentTemplateType.Empty)
				errorText = "Оберіть тип шаблону.";
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
