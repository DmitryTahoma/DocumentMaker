using Dml.Controller.Validation;
using Dml.Model.Back;
using Dml.Model.Template;
using Dml.UndoRedo;
using DocumentMaker.Controller.Controls;
using DocumentMaker.Model;
using DocumentMaker.Model.Back;
using DocumentMaker.Model.Controls;
using DocumentMaker.Model.Files;
using DocumentMaker.Model.OfficeFiles.Human;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace DocumentMaker.Controller
{
	public class MainWindowController
	{
		private const string saveFile = "session.xml";
		private const string humansFile = "HumanData.xlsx";
		private const string developmentTypesFile = "DevelopmentTypes.xlsx";
		private const string supportTypesFile = "SupportTypes.xlsx";
		private const string gameNamesFile = "projectnames.xml";

		private readonly StringValidator validator;
		private readonly DocumentMakerModel model;

		private string[] openFilesLater;
		private readonly List<string> notLoadedFiles;

		public MainWindowController(string[] args)
		{
			validator = new StringValidator();
			model = new DocumentMakerModel();
			BackDataControllers = new List<FullBackDataController>();

			openFilesLater = args;
			notLoadedFiles = new List<string>();
		}

		#region Window settings

		public double WindowTop { get => model.WindowTop; set => model.WindowTop = value; }
		public double WindowLeft { get => model.WindowLeft; set => model.WindowLeft = value; }
		public double WindowHeight { get => model.WindowHeight; set => model.WindowHeight = value; }
		public double WindowWidth { get => model.WindowWidth; set => model.WindowWidth = value; }
		public WindowState WindowState { get => model.WindowState; set => model.WindowState = value; }

		public string CorrectDevelopmentWindow_NumberText { get => model.CorrectDevelopmentWindow_NumberText; set => model.CorrectDevelopmentWindow_NumberText = value; }
		public bool CorrectDevelopmentWindow_TakeSumFromSupport { get => model.CorrectDevelopmentWindow_TakeSumFromSupport; set => model.CorrectDevelopmentWindow_TakeSumFromSupport = value; }

		public string CorrectSupportWindow_NumberText { get => model.CorrectSupportWindow_NumberText; set => model.CorrectSupportWindow_NumberText = value; }
		public bool CorrectSupportWindow_TakeSumFromDevelopment { get => model.CorrectSupportWindow_TakeSumFromDevelopment; set => model.CorrectSupportWindow_TakeSumFromDevelopment = value; }

		#endregion

		public IList<DmxFile> OpenedFilesList => model.OpenedFilesList;
		public DocumentTemplateType TemplateType { get => model.TemplateType; set => model.TemplateType = value; }
		public string TechnicalTaskDateText { get => model.TechnicalTaskDateText; set => model.TechnicalTaskDateText = value; }
		public string ActDateText { get => model.ActDateText; set => model.ActDateText = value; }
		public string TechnicalTaskNumText { get => model.TechnicalTaskNumText; set => model.TechnicalTaskNumText = value; }
		public string SelectedHuman { get => model.SelectedHuman; set => model.SelectedHuman = value; }
		public string HumanIdText { get => model.HumanIdText; set => model.HumanIdText = value; }
		public string AddressText { get => model.AddressText; set => model.AddressText = value; }
		public string PaymentAccountText { get => model.PaymentAccountText; set => model.PaymentAccountText = value; }
		public string BankName { get => model.BankName; set => model.BankName = value; }
		public string MfoText { get => model.MfoText; set => model.MfoText = value; }
		public string ContractNumberText { get => model.ContractNumberText; set => model.ContractNumberText = value; }
		public string ContractDateText { get => model.ContractDateText; set => model.ContractDateText = value; }
		public string ContractReworkNumberText { get => model.ContractReworkNumberText; set => model.ContractReworkNumberText = value; }
		public string ContractReworkDateText { get => model.ContractReworkDateText; set => model.ContractReworkDateText = value; }
		public string CityName { get => model.CityName; set => model.CityName = value; }
		public string ActSum { get => model.ActSum; set => model.ActSum = value; }
		public string ActSaldo { get => model.ActSaldo; set => model.ActSaldo = value; }
		public bool NeedUpdateSum { get => model.NeedUpdateSum; set => model.NeedUpdateSum = value; }
		public List<FullBackDataController> BackDataControllers { get; set; }
		public IList<FullDocumentTemplate> DocumentTemplatesList => model.DocumentTemplatesList;
		public IList<HumanData> HumanFullNameList => model.HumanFullNameList;
		public bool HasNoMovedFiles => model.HasNoMovedFiles;
		public IList<WorkObject> CurrentWorkTypesList => DocumentTemplatesList.FirstOrDefault(x => x.Type == TemplateType)?.WorkTypesList;
		public IList<WorkObject> CurrentReworkWorkTypesList => DocumentTemplatesList.FirstOrDefault(x => x.Type == TemplateType)?.ReworkWorkTypesList;
		public IList<GameObject> GameNameList => model.GameNameList;
		public bool IsActionsStackingEnabled => model.IsActionsStackingEnabled;
		public bool CanRedo => model.CanRedo;
		public bool CanUndo => model.CanUndo;

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
			string developmentTypesFullpath = Path.Combine(path, developmentTypesFile);
			string supportTypesFullpath = Path.Combine(path, supportTypesFile);
			string gameNamesFullpath = Path.Combine(path, gameNamesFile);

			model.Load(saveFullpath, out List<FullBackDataModel> backModels);
			foreach (FullBackDataModel model in backModels)
			{
				BackDataControllers.Add(new FullBackDataController(model));
			}

			if (!model.TryLoadHumans(humansFullpath)) notLoadedFiles.Add(humansFullpath);
			if (!model.TryLoadDevelopmentWorkTypes(developmentTypesFullpath)) notLoadedFiles.Add(developmentTypesFullpath);
			if (!model.TryLoadReworkWorkTypes(supportTypesFullpath)) notLoadedFiles.Add(supportTypesFullpath);
			if (!model.TryLoadGameNames(gameNamesFullpath)) notLoadedFiles.Add(gameNamesFullpath);
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
			else if (!validator.IsMoreDateTime(ActDateText, TechnicalTaskDateText))
				errorText = "Дата акту повинна бути більшою за дату технічного завдання.";
			else if (!validator.IsDigit(TechnicalTaskNumText))
				errorText = "Невірно заповнений номер технічного завдання.\nПриклад: 1";
			else if (!uint.TryParse(ActSum, out uint actSum) || actSum == 0)
				errorText = "Сума акту не може бути нульовою!";
			else if (ActSaldo != "0")
				errorText = "Сальдо не нульове!";
			else if (BackDataControllers.FirstOrDefault(x => x.IsOtherType) != null)
				errorText = "Таблиця \"Інше\" повинна бути пустою.";
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

				foreach (FullBackDataController backDataController in BackDataControllers)
				{
					foreach (FullBackDataController innerBackDataController in BackDataControllers)
					{
						if (backDataController != innerBackDataController)
						{
							if (backDataController.EqualsModelWithoutWorkAndRework(innerBackDataController, TemplateType))
							{
								bool validated = false;
								if (backDataController.IsRework == !innerBackDataController.IsRework)
									errorText = "Одна й та ж сама робота не може бути водночас і в розробці, і в підтримці.\nВ розробці пункт №" + (!backDataController.IsRework ? backDataController.Id.ToString() : innerBackDataController.Id.ToString()) + " і в підтримці пункт №" + (backDataController.IsRework ? backDataController.Id.ToString() : innerBackDataController.Id.ToString()) + " мають однакові данні.";
								else if (backDataController.WorkObjectId == innerBackDataController.WorkObjectId)
									errorText = "В " + (!backDataController.IsRework ? "розробці" : "підтримці") + " пункти №" + backDataController.Id.ToString() + " і №" + innerBackDataController.Id.ToString() + " мають однакові данні.";
								else
									validated = true;

								if (!validated)
									return false;
							}
						}
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
			ContractReworkNumberText = humanData.ContractReworkNumberText;
			ContractReworkDateText = humanData.ContractReworkDateText;
			AddressText = humanData.AddressText;
			MfoText = humanData.MfoText;
			CityName = humanData.CityName;
		}

		public void OpenFiles(string[] files, out string skippedFiles)
		{
			model.OpenFiles(files, out List<string> skipped);
			skippedFiles = string.Empty;
			if (skipped.Count > 0)
			{
				foreach (string file in skipped)
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
			bool actionsStackingEnable = IsActionsStackingEnabled;
			DisableActionsStacking();

			TemplateType = file.TemplateType;
			SelectedHuman = file.SelectedHuman;
			ActSum = file.ActSum;
			NeedUpdateSum = file.NeedUpdateSum;
			BackDataControllers.Clear();
			foreach (FullBackDataModel model in file.BackDataModels)
			{
				BackDataControllers.Add(new FullBackDataController(model));
			}

			if (actionsStackingEnable) EnableActionsStacking();
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

		public IEnumerable<int> CorrectSaldo(IEnumerable<FullBackDataController> backDataControllers)
		{
			return model.CorrectSaldo(backDataControllers.Select(x=>x.GetModel()));
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

		public IEnumerable<int> CorrectDevelopment(int minSum, bool takeSumFromSupport)
		{
			return model.CorrectDevelopment(minSum, takeSumFromSupport, GetModels());
		}

		public IEnumerable<int> CorrectSupport(int minSum, bool takeSumFromDevelopment)
		{
			return model.CorrectSupport(minSum, takeSumFromDevelopment, GetModels());
		}

		public void RandomizeWorkTypes(IEnumerable<FullBackDataController> checkedBackData)
		{
			IEnumerable<FullBackDataModel> checkedBackDataModels = checkedBackData.Select(x => x.GetModel());

			model.RandomizeWorkTypes(
				GetModels()
				.Where(x => x.IsOtherType == false && x.IsRework == false)
				.Select(y => new KeyValuePair<bool, FullBackDataModel>(checkedBackDataModels.Contains(y), y)));
		}

		public void RandomizeReworkWorkTypes(IEnumerable<FullBackDataController> checkedBackData)
		{
			IEnumerable<FullBackDataModel> checkedBackDataModels = checkedBackData.Select(x => x.GetModel());

			model.RandomizeWorkTypes(
				GetModels()
				.Where(x => x.IsOtherType == false && x.IsRework == true)
				.Select(y => new KeyValuePair<bool, FullBackDataModel>(checkedBackDataModels.Contains(y), y)));
		}

		public void TrimAllStrings()
		{
			TechnicalTaskDateText = TechnicalTaskDateText?.Trim();
			ActDateText = ActDateText?.Trim();
			TechnicalTaskNumText = TechnicalTaskNumText?.Trim();
			SelectedHuman = SelectedHuman?.Trim();
			HumanIdText = HumanIdText?.Trim();
			AddressText = AddressText?.Trim();
			PaymentAccountText = PaymentAccountText?.Trim();
			BankName = BankName?.Trim();
			MfoText = MfoText?.Trim();
			ContractNumberText = ContractNumberText?.Trim();
			ContractDateText = ContractDateText?.Trim();
			ContractReworkNumberText = ContractReworkNumberText?.Trim();
			ContractReworkDateText = ContractReworkDateText?.Trim();
			CityName = CityName?.Trim();
			ActSum = ActSum?.Trim();

			foreach (FullBackDataController backData in BackDataControllers)
			{
				backData.TrimAllStrings();
			}
		}

		public string GetDcmkFileName() => model.GetDcmkFileName();

		public void ExportDcmk(string path)
		{
			model.ExportDcmk(path, GetModels());
		}

		public List<string> GetNotLoadedFilesList() => notLoadedFiles;

		public void EnableActionsStacking()
		{
			model.EnableActionsStacking();
		}

		public void DisableActionsStacking()
		{
			model.DisableActionsStacking();
		}

		public void Redo()
		{
			model.Redo();
		}

		public void Undo()
		{
			model.Undo();
		}

		public void AddUndoRedoLink(IUndoRedoAction action)
		{
			model.AddUndoRedoLink(action);
		}

		public IUndoRedoActionsStack GetActionsStack()
		{
			return model.GetActionsStack();
		}

		public void RemoveFromActionsStack(IEnumerable<FullBackDataController> controllers)
		{
			model.RemoveFromActionsStack(controllers.Select(x => x.GetModel()));
		}

		public void SubscribeActionPushed(UndoRedoActionPushedHandler action)
		{
			model.SubscribeActionPushed(action);
		}
	}
}
