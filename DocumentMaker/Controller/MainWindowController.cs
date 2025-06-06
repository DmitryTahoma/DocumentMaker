﻿using Dml.Controller.Validation;
using Dml.Model.Back;
using Dml.Model.Template;
using Dml.UndoRedo;
using DocumentMaker.Controller.Controls;
using DocumentMaker.Settings;
using DocumentMakerModelLibrary;
using DocumentMakerModelLibrary.Back;
using DocumentMakerModelLibrary.Controls;
using DocumentMakerModelLibrary.Files;
using DocumentMakerModelLibrary.OfficeFiles.Human;
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
		private const string contractFolder = "Contract";
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

			FileInfo file;
			List<string> FileArgs = new List<string>();
			foreach (string name in args)
			{
				file = new FileInfo(name);
				if (file.Extension == Dml.Model.Files.BaseDmxFile.Extension
					|| file.Extension == DcmkFile.Extension)
				{
					FileArgs.Add(name);
					break;
				}
			}

			openFilesLater = FileArgs.ToArray();
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
		public bool CorrectDevelopmentDialog_IsRemoveIdenticalNumbers { get => model.CorrectDevelopmentDialog_IsRemoveIdenticalNumbers; set => model.CorrectDevelopmentDialog_IsRemoveIdenticalNumbers = value; }

		public string CorrectSupportWindow_NumberText { get => model.CorrectSupportWindow_NumberText; set => model.CorrectSupportWindow_NumberText = value; }
		public bool CorrectSupportWindow_TakeSumFromDevelopment { get => model.CorrectSupportWindow_TakeSumFromDevelopment; set => model.CorrectSupportWindow_TakeSumFromDevelopment = value; }
		public bool CorrectSupportDialog_IsCreateNewWorks { get => model.CorrectSupportDialog_IsCreateNewWorks; set => model.CorrectSupportDialog_IsCreateNewWorks = value; }
		public bool CorrectSupportDialog_IsRemoveIdenticalNumbers { get => model.CorrectSupportDialog_IsRemoveIdenticalNumbers; set => model.CorrectSupportDialog_IsRemoveIdenticalNumbers = value; }

		#endregion

		public int LastTypeConnection { get => model.LastTypeConnection; set => model.LastTypeConnection = value; }

		public IList<DmxFile> OpenedFilesList => model.OpenedFilesList;
		public DocumentTemplateType TemplateType { get => model.TemplateType; set => model.TemplateType = value; }
		public DocumentType DocType { get => model.DocType; set => model.DocType = value; }
		public string TechnicalTaskDateText { get => model.TechnicalTaskDateText; set => model.TechnicalTaskDateText = value; }
		public string ActDateText { get => model.ActDateText; set => model.ActDateText = value; }
		public string TechnicalTaskNumText { get => model.TechnicalTaskNumText; set => model.TechnicalTaskNumText = value; }
		public string SelectedHuman { get => model.SelectedHuman; set => model.SelectedHuman = value; }
		public string HumanNameAlt { get => model.HumanNameAlt; set => model.HumanNameAlt = value; }
		public string SelectedContractFile { get => model.SelectedContractFile; set => model.SelectedContractFile = value; }
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
		public string RegionName { get => model.RegionName; set => model.RegionName = value; }
		public string AccountNumberText { get => model.AccountNumberText; set => model.AccountNumberText = value; }
		public string ActSum { get => model.ActSum; set => model.ActSum = value; }
		public string ActSaldo { get => model.ActSaldo; set => model.ActSaldo = value; }
		public bool NeedUpdateSum { get => model.NeedUpdateSum; set => model.NeedUpdateSum = value; }
		public List<FullBackDataController> BackDataControllers { get; set; }
		public IList<FullDocumentTemplate> DocumentTemplatesList => model.DocumentTemplatesList;
		public IList<string> ContractFilesList => model.ContractFilesList;
		public IList<HumanData> HumanFullNameList => model.HumanFullNameList;
		public bool HasNoMovedFiles => model.HasNoMovedFiles;
		public IList<WorkObject> CurrentWorkTypesList => DocumentTemplatesList.FirstOrDefault(x => x.Type == TemplateType)?.WorkTypesList;
		public IList<WorkObject> CurrentReworkWorkTypesList => DocumentTemplatesList.FirstOrDefault(x => x.Type == TemplateType)?.ReworkWorkTypesList;
		public IList<BackDataType> CurrentBackDataTypesList => DocumentTemplatesList.FirstOrDefault(x => x.Type == TemplateType)?.DataTypesList;
		public IList<GameObject> GameNameList => model.GameNameList;
		public bool IsActionsStackingEnabled => model.IsActionsStackingEnabled;
		public bool CanRedo => model.CanRedo;
		public bool CanUndo => model.CanUndo;
		public bool HaveUnsavedChanges { get => model.HaveUnsavedChanges; set => model.HaveUnsavedChanges = value; }

		public void Save()
		{
			string fullpath = Path.Combine(ProgramSettings.DirectoryPath, saveFile);

			model.Save(fullpath, GetModels());
		}

		public void Load()
		{
			string saveFullpath = Path.Combine(ProgramSettings.DirectoryPath, saveFile);
			string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			string pathContractFolder = Path.Combine(path, contractFolder);
			string developmentTypesFullpath = Path.Combine(path, developmentTypesFile);
			string supportTypesFullpath = Path.Combine(path, supportTypesFile);
			string gameNamesFullpath = Path.Combine(path, gameNamesFile);

			model.Load(saveFullpath, out List<FullBackDataModel> backModels);
			model.LoadContract(pathContractFolder);

			foreach (FullBackDataModel model in backModels)
			{
				BackDataControllers.Add(new FullBackDataController(model));
			}

			if (!model.TryLoadDevelopmentWorkTypes(developmentTypesFullpath)) notLoadedFiles.Add(developmentTypesFullpath);
			if (!model.TryLoadReworkWorkTypes(supportTypesFullpath)) notLoadedFiles.Add(supportTypesFullpath);
			if (!model.TryLoadGameNames(gameNamesFullpath)) notLoadedFiles.Add(gameNamesFullpath);
		}

		public void LoadHumans(string fileName)
		{
			string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			string pathContractFolder = Path.Combine(path, contractFolder);
			string humansFullpath = Path.Combine(pathContractFolder, fileName);

			if (!model.TryLoadHumans(humansFullpath)) notLoadedFiles.Add(humansFullpath);
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

				if (human == null)
					errorText = "Людина необрана!";
				else
				{
					SetHuman(human);

					if (!validator.IsFullName(SelectedHuman))
						errorText = "Невірно заповнена строка з повним ім’ям.\nПриклад: Іванов Іван Іванович";
					else if (!validator.IsFree(HumanIdText))
						errorText = "Строка \"ІН\" не може бути пустою.";
					else if (!validator.IsFree(AddressText))
						errorText = "Строка \"Адреса проживання\" не може бути пустою.";
					else if (!validator.IsFree(PaymentAccountText) && (model.DocType == DocumentType.FOP || DocType == DocumentType.FOPF))
						errorText = "Строка \"р/р\" не може бути пустою.";
					else if (!validator.IsFree(BankName) && (model.DocType == DocumentType.FOP || DocType == DocumentType.FOPF))
						errorText = "Строка \"Банк\" не може бути пустою.";
					else if (!validator.IsFree(MfoText) && (model.DocType == DocumentType.FOP || DocType == DocumentType.FOPF))
						errorText = "Строка \"МФО\" не може бути пустою.";
					else if ((model.DocType == DocumentType.FOP || DocType == DocumentType.FOPF || model.DocType == DocumentType.GIG) && (!validator.IsFree(ContractNumberText) || !validator.IsFree(ContractDateText)) && BackDataControllers.FirstOrDefault((x) => !x.IsOtherType && !x.IsRework) != null)
						errorText = "У обраної людини (" + SelectedHuman + ") немає номеру та/або дати договору розробки. Таблиця з розробкою повинна бути пустою.";
					else if ((model.DocType == DocumentType.FOP || DocType == DocumentType.FOPF || model.DocType == DocumentType.GIG) && (!validator.IsFree(ContractReworkNumberText) || !validator.IsFree(ContractReworkDateText)) && BackDataControllers.FirstOrDefault((x) => !x.IsOtherType && x.IsRework) != null)
						errorText = "У обраної людини (" + SelectedHuman + ") немає номеру та/або дати договору підтримки. Таблиця з підтримкою повинна бути пустою.";
					else
						isValidGeneralData = true;
				}
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
			HumanNameAlt = humanData.HumanNameAlt;
			SelectedContractFile = humanData.ContractFile;
			HumanIdText = humanData.HumanIdText;
			DocType = humanData.DocType;
			BankName = humanData.BankName;
			PaymentAccountText = humanData.PaymentAccountText;
			ContractNumberText = humanData.ContractNumberText;
			ContractDateText = humanData.ContractDateText;
			ContractReworkNumberText = humanData.ContractReworkNumberText;
			ContractReworkDateText = humanData.ContractReworkDateText;
			AddressText = humanData.AddressText;
			MfoText = humanData.MfoText;
			CityName = humanData.CityName;
			AccountNumberText = humanData.AccountNumberText;
			RegionName = humanData.RegionName;
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
			SelectedContractFile = file.SelectedContractFile;
			TechnicalTaskDateText = file.TechnicalTaskDateText;
			ActDateText = file.ActDateText;
			TechnicalTaskNumText = file.TechnicalTaskNumText;
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
			return model.CorrectSaldo(backDataControllers.Select(x => x.GetModel()));
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

		public IEnumerable<int> CorrectDevelopment(int minSum, bool takeSumFromSupport, bool isRemoveIdenticalNumbers)
		{
			return model.CorrectDevelopment(minSum, takeSumFromSupport, isRemoveIdenticalNumbers, GetModels());
		}

		public IEnumerable<int> CorrectSupport(int minSum, bool takeSumFromDevelopment, bool isCreateNewWorks, bool isRemoveIdenticalNumbers, out List<KeyValuePair<FullBackDataController, int>> newControllers)
		{
			var res = model.CorrectSupport(minSum, takeSumFromDevelopment, isCreateNewWorks, isRemoveIdenticalNumbers, GetModels(), out List<KeyValuePair<FullBackDataModel, int>> newModels);
			newControllers = new List<KeyValuePair<FullBackDataController, int>>(newModels.Select(x => new KeyValuePair<FullBackDataController, int>(new FullBackDataController(x.Key), x.Value)));
			return res;
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
			HumanNameAlt = HumanNameAlt?.Trim();
			SelectedContractFile = SelectedContractFile?.Trim();
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
			AccountNumberText = AccountNumberText?.Trim();
			RegionName = RegionName?.Trim();
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
			ResetHaveUnsavedChanges();
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

		public void ResetWeights()
		{
			model.ResetWeight(GetModels());
		}

		public bool HaveUnsavedChangesAtAll()
		{
			if (HaveUnsavedChanges)
				return true;

			foreach (FullBackDataController backDataController in BackDataControllers)
			{
				if (backDataController.HaveUnsavedChanges)
					return true;
			}
			return false;
		}

		public void ResetHaveUnsavedChanges()
		{
			HaveUnsavedChanges = false;

			foreach (FullBackDataController backDataController in BackDataControllers)
			{
				backDataController.HaveUnsavedChanges = false;
			}
		}

		public void ClearUndoRedo()
		{
			model.ClearUndoRedo();
		}

		public void ChangeOpenedFilesExtension()
		{
			foreach (DmxFile file in OpenedFilesList)
			{
				file.ChangeExtension();
			}
		}

		public bool ChangeTechnicalTaskDateAtAllFiles(string technicalTaskDateText)
		{
			if (!validator.IsFree(technicalTaskDateText))
			{
				return false;
			}
			else
			{
				model.ChangeTechnicalTaskDateAtAllFiles(technicalTaskDateText);
				return true;
			}
		}

		public bool ChangeActDateAtAllFiles(string actDateText)
		{
			if (!validator.IsFree(actDateText))
			{
				return false;
			}
			else
			{
				model.ChangeActDateAtAllFiles(actDateText);
				return true;
			}
		}

		public bool ChangeContractFileAtAllFiles(string nameFile)
		{
			if (!validator.IsFree(nameFile))
			{
				return false;
			}
			else
			{
				model.ChangeContractFileAtAllFiles(nameFile);
				return true;
			}
		}

		public bool ChangeGameNameAtAllFiles(string sourceGameName, string newGameName, string sourceEpisode, string newEpisode)
		{
			if (!validator.IsFree(sourceGameName) || !validator.IsFree(newGameName))
			{
				return false;
			}
			else
			{
				model.ChangeGameNameAtAllFiles(sourceGameName, newGameName, sourceEpisode, newEpisode);
				return true;
			}
		}
	}
}
