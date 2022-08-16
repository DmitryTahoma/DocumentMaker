using Dml.Controller;
using Dml.Controller.Validation;
using Dml.Model;
using Dml.Model.Files;
using Dml.Model.Template;
using DocumentMaker.Controller.Controls;
using DocumentMaker.Model;
using DocumentMaker.Model.Controls;
using DocumentMaker.Model.Files;
using DocumentMaker.Model.OfficeFiles.Human;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;

namespace DocumentMaker.Controller
{
	public class MainWindowController
	{
		private const string saveFile = "session.xml";
		private const string humansFile = "HumanData.xlsx";

		private readonly StringValidator validator;
		private readonly DocumentMakerModel model;

		private string[] openFilesLater;

		public MainWindowController(string[] args)
		{
			validator = new StringValidator();
			model = new DocumentMakerModel();
			BackDataControllers = new List<FullBackDataController>();

			openFilesLater = args;
		}

		#region Window settings

		public double WindowTop { get => model.WindowTop; set => model.WindowTop = value; }
		public double WindowLeft { get => model.WindowLeft; set => model.WindowLeft = value; }
		public double WindowHeight { get => model.WindowHeight; set => model.WindowHeight = value; }
		public double WindowWidth { get => model.WindowWidth; set => model.WindowWidth = value; }
		public WindowState WindowState { get => model.WindowState; set => model.WindowState = value; }

		public double WindowInformation_WindowTop { get => model.WindowInformation_WindowTop; set => model.WindowInformation_WindowTop  = value; }
		public double WindowInformation_WindowLeft { get => model.WindowInformation_WindowLeft; set => model.WindowInformation_WindowLeft  = value; }
		public double WindowInformation_WindowHeight { get => model.WindowInformation_WindowHeight; set => model.WindowInformation_WindowHeight  = value; }
		public double WindowInformation_WindowWidth { get => model.WindowInformation_WindowWidth; set => model.WindowInformation_WindowWidth  = value; }
		public WindowState WindowInformation_WindowState { get => model.WindowInformation_WindowState; set => model.WindowInformation_WindowState = value; }

		public double CorrectDevelopmentWindow_WindowTop { get => model.CorrectDevelopmentWindow_WindowTop; set => model.CorrectDevelopmentWindow_WindowTop = value; }
		public double CorrectDevelopmentWindow_WindowLeft { get => model.CorrectDevelopmentWindow_WindowLeft; set => model.CorrectDevelopmentWindow_WindowLeft  = value; }
		public string CorrectDevelopmentWindow_NumberText { get => model.CorrectDevelopmentWindow_NumberText; set => model.CorrectDevelopmentWindow_NumberText  = value; }
		public bool CorrectDevelopmentWindow_TakeSumFromSupport { get => model.CorrectDevelopmentWindow_TakeSumFromSupport; set => model.CorrectDevelopmentWindow_TakeSumFromSupport  = value; }

		public double CorrectSupportWindow_WindowTop { get => model.CorrectSupportWindow_WindowTop; set => model.CorrectSupportWindow_WindowTop = value; }
		public double CorrectSupportWindow_WindowLeft { get => model.CorrectSupportWindow_WindowLeft; set => model.CorrectSupportWindow_WindowLeft = value; }
		public string CorrectSupportWindow_NumberText { get => model.CorrectSupportWindow_NumberText; set => model.CorrectSupportWindow_NumberText = value; }
		public bool CorrectSupportWindow_TakeSumFromDevelopment { get => model.CorrectSupportWindow_TakeSumFromDevelopment; set => model.CorrectSupportWindow_TakeSumFromDevelopment = value; }

		#endregion

		public IList<DmxFile> OpenedFilesList => model.OpenedFilesList;
		public DocumentTemplateType TemplateType { get => model.TemplateType; set => model.TemplateType = value; }
		public string TechnicalTaskDateText { get => model.TechnicalTaskDateText; set => model.TechnicalTaskDateText = value; }
		public string ActDateText { get => model.ActDateText; set => model.ActDateText = value; }
		public string AdditionNumText { get => model.AdditionNumText; set => model.AdditionNumText = value; }
		public string SelectedHuman { get => model.SelectedHuman; set => model.SelectedHuman = value; }
		public string HumanIdText { get => model.HumanIdText; set => model.HumanIdText = value; }
		public string AddressText { get => model.AddressText; set => model.AddressText = value; }
		public string PaymentAccountText { get => model.PaymentAccountText; set => model.PaymentAccountText = value; }
		public string BankName { get => model.BankName; set => model.BankName = value; }
		public string MfoText { get => model.MfoText; set => model.MfoText = value; }
		public string ContractNumberText { get => model.ContractNumberText; set => model.ContractNumberText = value; }
		public string ContractDateText { get => model.ContractDateText; set => model.ContractDateText = value; }
		public string ActSum { get => model.ActSum; set => model.ActSum = value; }
		public string ActSaldo { get => model.ActSaldo; set => model.ActSaldo = value; }
		public List<FullBackDataController> BackDataControllers { get; set; }
		public IList<DocumentTemplate> DocumentTemplatesList => model.DocumentTemplatesList;
		public IList<HumanData> HumanFullNameList => model.HumanFullNameList;
		public bool HasNoMovedFiles => model.HasNoMovedFiles;

		public void Save()
		{
			string fullpath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), saveFile);

			model.Save(fullpath, GetModels());
		}

		public void Load()
		{
			string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			string saveFullpath = Path.Combine(path, saveFile);
			string humansFullpath = Path.Combine(path, humansFile);

			model.Load(saveFullpath, out List<FullBackDataModel> backModels);
			foreach (FullBackDataModel model in backModels)
			{
				BackDataControllers.Add(new FullBackDataController(model));
			}
			model.LoadHumans(humansFullpath);
		}

		public void Export(string path)
		{
			model.Export(path, GetModels());
		}

		public bool Validate(out string errorText)
		{
			errorText = "";
			bool isValidGeneralData = false;

			if (TemplateType == DocumentTemplateType.Empty)
				errorText = "Оберіть тип шаблону.";
			else if (!validator.IsDate(TechnicalTaskDateText))
				errorText = "Невірно заповнена дата тех.завдання.\nПриклад: 20.07.2021";
			else if (!validator.IsDate(ActDateText))
				errorText = "Невірно заповнена дата акту.\nПриклад: 20.07.2021";
			else if (!validator.IsDigit(AdditionNumText))
				errorText = "Невірно заповнений номер додатку.\nПриклад: 1";
			else
			{
				HumanData human = GetSelectedHuman();
				SetHuman(human);

				if (human == null)
					errorText = "Людина необрана!";
				else if (!validator.IsFullName(SelectedHuman))
					errorText = "Невірно заповнена строка з повним ім’ям.\nПриклад: Іванов Іван Іванович";
				else if (!validator.IsFree(HumanIdText))
					errorText = "Строка \"ІН\" не може бути пустою.";
				else if (!validator.IsFree(AddressText))
					errorText = "Строка \"Адреса проживання\" не може бути пустою.";
				else if (!validator.IsFree(PaymentAccountText))
					errorText = "Строка \"р/р\" не може бути пустою.";
				else if (!validator.IsFree(BankName))
					errorText = "Строка \"Банк\" не може бути пустою.";
				else if (!validator.IsFree(MfoText))
					errorText = "Строка \"МФО\" не може бути пустою.";
				else if (!validator.IsFree(ContractNumberText))
					errorText = "Строка \"Номер договору\" не може бути пустою.";
				else if (!validator.IsFree(ContractDateText))
					errorText = "Строка \"Дата складання договору\" не може бути пустою.";
				else
					isValidGeneralData = true;
			}

			if (isValidGeneralData)
			{
				foreach (FullBackDataController backDataController in BackDataControllers)
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

		public string GetInfoNoMovedFiles()
		{
			IEnumerable<KeyValuePair<string, string>> files = model.GetInfoNoMovedFiles();
			string res = "";

			foreach (KeyValuePair<string, string> file in files)
			{
				res += file.Value + "\n";
			}

			return res;
		}

		public void ReplaceCreatedFiles()
		{
			model.ReplaceCreatedFiles();
		}

		public void RemoveTemplates()
		{
			model.RemoveTemplates();
		}

		public void SetHuman(HumanData humanData)
		{
			SelectedHuman = humanData.Name;
			HumanIdText = humanData.HumanIdText;
			BankName = humanData.BankName;
			PaymentAccountText = humanData.PaymentAccountText;
			ContractNumberText = humanData.ContractNumberText;
			ContractDateText = humanData.ContractDateText;
			AddressText = humanData.AddressText;
			MfoText = humanData.MfoText;
		}

		public void OpenFiles(string[] files, out string skippedFiles)
		{
			model.OpenFiles(files, out List<string> skipped);
			skippedFiles = string.Empty;
			if(skipped.Count > 0)
			{
				foreach(string file in skipped)
				{
					skippedFiles += file + '\n';
				}
				skippedFiles = skippedFiles.Substring(0, skippedFiles.Length - 1);
			}
		}

		public void LoadFiles()
		{
			model.LoadFiles();
		}

		public void SetDataFromFile(DmxFile file)
		{
			TemplateType = file.TemplateType;
			SelectedHuman = file.SelectedHuman;
			BackDataControllers.Clear();
			foreach(FullBackDataModel model in file.BackDataModels)
			{
				BackDataControllers.Add(new FullBackDataController(model));
			}
		}

		public void CloseFile(DmxFile file)
		{
			model.CloseFile(file);
		}

		public void SetSelectedFile(DmxFile file)
		{
			model.SetSelectedFile(file);
		}

		public DmxFile GetSelectedFile()
		{
			return model.GetSelectedFile();
		}

		public string[] GetOpenLaterFiles()
		{
			string[] res = openFilesLater;
			openFilesLater = null;
			return res;
		}

		public HumanData GetSelectedHuman()
		{
			return model.HumanFullNameList.FirstOrDefault(x => x.Name == SelectedHuman);
		}

		public void CorrectSaldo()
		{
			model.CorrectSaldo(GetModels());
		}

		private List<FullBackDataModel> GetModels()
		{
			List<FullBackDataModel> backDataModels = new List<FullBackDataModel>();
			foreach (FullBackDataController controller in BackDataControllers)
			{
				backDataModels.Add(controller.GetModel());
			}
			return backDataModels;
		}

		public void CorrectDevelopment(int minSum, bool takeSumFromSupport)
		{
			List<FullBackDataModel> developmentModels = new List<FullBackDataModel>(), supportModels = new List<FullBackDataModel>();
			foreach(FullBackDataController controller in BackDataControllers)
			{
				if(controller.IsRework)
				{
					supportModels.Add(controller.GetModel());
				}
				else
				{
					developmentModels.Add(controller.GetModel());
				}
			}

			model.CorrectDevelopment(minSum, takeSumFromSupport, developmentModels, supportModels);
		}

		public void CorrectSupport(int minSum, bool takeSumFromDevelopment)
		{
			List<FullBackDataModel> developmentModels = new List<FullBackDataModel>(), supportModels = new List<FullBackDataModel>();
			foreach (FullBackDataController controller in BackDataControllers)
			{
				if (controller.IsRework)
				{
					supportModels.Add(controller.GetModel());
				}
				else
				{
					developmentModels.Add(controller.GetModel());
				}
			}

			model.CorrectSupport(minSum, takeSumFromDevelopment, developmentModels, supportModels);
		}
	}
}
