using Dml;
using Dml.Model.Back;
using Dml.Model.Files;
using Dml.Model.Session;
using Dml.Model.Session.Attributes;
using Dml.Model.Template;
using Dml.UndoRedo;
using DocumentMakerModelLibrary.Algorithm;
using DocumentMakerModelLibrary.Back;
using DocumentMakerModelLibrary.Controls;
using DocumentMakerModelLibrary.Files;
using DocumentMakerModelLibrary.OfficeFiles;
using DocumentMakerModelLibrary.OfficeFiles.Human;
using DocumentMakerModelLibrary.Session;
using DocumentMaker.Resources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace DocumentMakerModelLibrary
{
	public class DocumentMakerModel
	{
		private readonly OfficeExporter exporter;
		private readonly ExcelExporter excelExporter;
		private readonly ObservableCollection<FullDocumentTemplate> documentTemplates;
		private readonly ObservableCollection<string> contractFilesList;
		private readonly ObservableRangeCollection<HumanData> humanFullNameList;
		private readonly ObservableRangeCollection<DmxFile> openedFilesList;
		private readonly List<GameObject> gameNameList;

		private readonly ActionsStack actionsStack;
		private readonly PropertySetActionProvider<DocumentMakerModel, string> actSumActionProvider = new PropertySetActionProvider<DocumentMakerModel, string>(x => x.ActSum);

		private string actSum;

		public DocumentMakerModel()
		{
			exporter = new OfficeExporter();
			excelExporter = new ExcelExporter();
			documentTemplates = new ObservableCollection<FullDocumentTemplate>
			{
				new FullDocumentTemplate("Скриптувальник", DocumentTemplateType.Scripter),
				new FullDocumentTemplate("Технічний дизайнер", DocumentTemplateType.Cutter),
				new FullDocumentTemplate("Художник", DocumentTemplateType.Painter),
				new FullDocumentTemplate("Моделлер", DocumentTemplateType.Modeller),
				new FullDocumentTemplate("Тестувальник", DocumentTemplateType.Tester),
				new FullDocumentTemplate("Програміст", DocumentTemplateType.Programmer),
				new FullDocumentTemplate("Звукорежисер", DocumentTemplateType.Soundman),
				new FullDocumentTemplate("Аніматор", DocumentTemplateType.Animator),
				new FullDocumentTemplate("Перекладач", DocumentTemplateType.Translator),
				new FullDocumentTemplate("Підтримка", DocumentTemplateType.Support),
			};
			contractFilesList = new ObservableCollection<string>();
			humanFullNameList = new ObservableRangeCollection<HumanData>();
			openedFilesList = new ObservableRangeCollection<DmxFile>();
			gameNameList = new List<GameObject>();
			actionsStack = new ActionsStack();
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

		[IsNotDmxContent]
		public string CorrectDevelopmentWindow_NumberText { get; set; } = "2500";
		[IsNotDmxContent]
		public bool CorrectDevelopmentWindow_TakeSumFromSupport { get; set; } = false;
		[IsNotDmxContent]
		public bool CorrectDevelopmentDialog_IsRemoveIdenticalNumbers { get; set; } = false;

		[IsNotDmxContent]
		public string CorrectSupportWindow_NumberText { get; set; } = "2100";
		[IsNotDmxContent]
		public bool CorrectSupportWindow_TakeSumFromDevelopment { get; set; } = false;
		[IsNotDmxContent]
		public bool CorrectSupportDialog_IsCreateNewWorks { get; set; } = false;
		[IsNotDmxContent]
		public bool CorrectSupportDialog_IsRemoveIdenticalNumbers { get; set; } = false;

		#endregion

		[IsNotDmxContent]
		public int LastTypeConnection { get; set; } = 0;

		public IList<DmxFile> OpenedFilesList => openedFilesList;
		public DocumentTemplateType TemplateType { get; set; } = DocumentTemplateType.Empty;
		public DocumentType DocType { get; set; } = DocumentType.Empty;
		public string TechnicalTaskDateText { get; set; }
		public string ActDateText { get; set; }
		public string TechnicalTaskNumText { get; set; } = "1";
		public string SelectedHuman { get; set; }
		public string HumanNameAlt { get; set; }
		public string SelectedContractFile { get; set; }
		public string HumanIdText { get; set; }
		public string AddressText { get; set; }
		public string PaymentAccountText { get; set; }
		public string BankName { get; set; }
		public string MfoText { get; set; }
		public string ContractNumberText { get; set; }
		public string ContractDateText { get; set; }
		public string ContractReworkNumberText { get; set; }
		public string ContractReworkDateText { get; set; }
		public string CityName { get; set; }
		public string RegionName { get; set; }
		public string AccountNumberText { get; set; }
		public string ActSum
		{
			get => actSum;
			set
			{
				if (actionsStack.ActionsStackingEnabled) actionsStack.Push(actSumActionProvider.CreateAction(this, value));
				actSum = value;
			}
		}
		public string ActSaldo { get; set; }
		public bool NeedUpdateSum { get; set; }
		public IList<FullDocumentTemplate> DocumentTemplatesList => documentTemplates;
		public IList<string> ContractFilesList => contractFilesList;
		public IList<HumanData> HumanFullNameList => humanFullNameList;
		public bool HasNoMovedFiles => exporter.HasNoMovedFiles;
		public IList<GameObject> GameNameList => gameNameList;
		public bool IsActionsStackingEnabled => actionsStack.ActionsStackingEnabled;
		public bool CanRedo => actionsStack.CanRedo;
		public bool CanUndo => actionsStack.CanUndo;

		[IsNotSavingContent]
		public bool HaveUnsavedChanges { get; set; }

		public void Save(string path, IEnumerable<FullBackDataModel> backModels)
		{
			DmxSaver saver = new DmxSaver();
			saver.AppendAllProperties(this);

			foreach (FullBackDataModel backDataModel in backModels)
			{
				saver.CreateBackNode();
				saver.AppendAllBackProperties(backDataModel);
				saver.PushBackNode();
			}
			foreach (DmxFile file in openedFilesList)
			{
				saver.CreateDmxFileNode();
				saver.AppendAllDmxFileProperties(file);
				saver.PushDmxFileNode();
			}

			saver.Save(path);
		}

		public void Load(string path, out List<FullBackDataModel> backModels)
		{
			backModels = new List<FullBackDataModel>();

			DmxLoader loader = new DmxLoader();
			if (loader.TryLoad(path))
			{
				loader.SetLoadedProperties(this);
				loader.SetLoadedListProperties(backModels);
				loader.SetLoadedDmxFiles(openedFilesList);
			}
		}

		public void LoadContract(string path)
		{
			if (Directory.Exists(path))
			{
				string fileMask = "*.xlsx";
				foreach (string fileName in Directory.GetFiles(path, fileMask))
				{
					contractFilesList.Add(fileName.Substring(fileName.LastIndexOf('\\') + 1));
				}
			}
		}

		public bool TryLoadHumans(string path)
		{
			try
			{
				XlsxLoader loader = new XlsxLoader();
				loader.LoadHumans(path, humanFullNameList);
				return true;
			}
			catch
			{
				return false;
			}
		}

		public bool TryLoadDevelopmentWorkTypes(string path)
		{
			try
			{
				foreach (FullDocumentTemplate template in documentTemplates)
				{
					template.LoadWorkTypesList(path);
				}
				return true;
			}
			catch
			{
				return false;
			}
		}

		public bool TryLoadReworkWorkTypes(string path)
		{
			try
			{
				foreach (FullDocumentTemplate template in documentTemplates)
				{
					template.LoadReworkWorkTypesList(path);
				}
				return true;
			}
			catch
			{
				return false;
			}
		}

		public bool TryLoadGameNames(string path)
		{
			XmlLoader loader = new XmlLoader();
			if (loader.TryLoad(path))
			{
				loader.SetLoadedGameNames(gameNameList);
				return true;
			}
			return false;
		}

		public void Export(string path, IEnumerable<FullBackDataModel> backModels)
		{
			for (int i = 0; i < 2; ++i)
			{
				bool isExportRework = i == 1;
				uint actSum = 0;

				foreach (FullBackDataModel backModel in backModels)
				{
					if (backModel.IsRework == isExportRework && uint.TryParse(backModel.SumText, out uint sum))
					{
						actSum += sum;
					}
				}

				DocumentGeneralData generalData = new DocumentGeneralData(this, isExportRework, actSum, backModels.Count());
				DocumentTableData tableData = new DocumentTableData(backModels, TemplateType, isExportRework);
				if (tableData.Count() <= 0) continue;
				int j = -1;
				foreach (ResourceInfo resource in DocumentResourceManager.GetItems(isExportRework))
				{
					++j;

					if (j == 2)
						continue;

					if (j == 0 && (TemplateType == DocumentTemplateType.Support || TemplateType == DocumentTemplateType.Translator))
						continue;

					exporter.Clear();

					if (resource.Type == ResourceType.Docx)
					{
						string nearFullname = exporter.ExportWordTemplate(resource.ProjectName, isExportRework, 0);
						exporter.FillWordGeneralData(generalData);
						exporter.FillWordTableData(tableData, 0);
						exporter.SaveWordContent(nearFullname);
						exporter.SaveTemplate(generalData, path, nearFullname, resource.TemplateName);
					}
					else if (resource.Type == ResourceType.Xlsx)
					{
						exporter.ExportExcelTemplate(resource.ProjectName);
						exporter.FillExcelTableData(resource.ProjectName, tableData);
						exporter.SaveTemplate(generalData, path, "", resource.TemplateName);
					}
				}
			}

			if (DocType == DocumentType.FOP || DocType == DocumentType.FOPF)
			{ 
				uint actSum = 0;
				foreach (FullBackDataModel backModel in backModels)
				{
					if (uint.TryParse(backModel.SumText, out uint sum))
					{
						actSum += sum;
					}
				}

				DocumentGeneralData generalData = new DocumentGeneralData(this, false, actSum, backModels.Count());
				DocumentTableData tableData = new DocumentTableData(backModels, this);

				if (tableData.Count() > 0)
				{
					int j = -1;
					foreach (ResourceInfo resource in DocumentResourceManager.GetItems(false))
					{
						++j;

						if (j != 2)
							continue;

						exporter.Clear();

						if (resource.Type == ResourceType.Docx)
						{
							string nearFullname = exporter.ExportWordTemplate(resource.ProjectName, false, 2);
							exporter.FillWordGeneralData(generalData);
							exporter.FillWordTableData(tableData, 2);
							exporter.SaveWordContent(nearFullname);
							exporter.SaveTemplate(generalData, path, nearFullname, resource.TemplateName);
						}
					}
				}
			}
		}

		public IEnumerable<KeyValuePair<string, string>> GetInfoNoMovedFiles()
		{
			return exporter.GetNoMovedFiles();
		}

		public void ReplaceCreatedFiles()
		{
			exporter.ReplaceCreatedFiles();
		}

		public void RemoveTemplates()
		{
			exporter.RemoveTemplates();
		}

		public void OpenFiles(string[] files, out List<string> skippedFiles)
		{
			skippedFiles = new List<string>();
			if (files != null && files.Length > 0)
			{
				List<DmxFile> adding = new List<DmxFile>();
				foreach (string file in files)
				{
					if (openedFilesList.Where(f => f.FullName == file).Count() > 0) continue;

					if (File.Exists(file) && (file.EndsWith(BaseDmxFile.Extension) || file.EndsWith(DcmkFile.Extension)))
					{
						bool isAdd = true;
						DmxFile dmxFile = file.EndsWith(DcmkFile.Extension) ? new DcmkFile(file) : new DmxFile(file);

						IList<DmxFile> fileList = openedFilesList.Where(f => Path.ChangeExtension(f.Name, null) == Path.ChangeExtension(dmxFile.Name, null)).ToList();
						foreach (DmxFile f in fileList)
						{
							if (Path.ChangeExtension(f.FullName, null) == Path.ChangeExtension(dmxFile.FullName, null))
							{
								isAdd = false;
								break;
							}
						}

						if (isAdd)
						{
							adding.Add(dmxFile);
							dmxFile.ShowFullName = fileList.Count > 0;

							foreach (DmxFile f in fileList)
							{
								f.ShowFullName = true;
							}
						}
					}
					else
					{
						skippedFiles.Add(file);
					}
				}
				openedFilesList.AddRange(adding);
			}
		}

		public void LoadFiles()
		{
			foreach (DmxFile file in openedFilesList)
			{
				if (!file.Loaded)
				{
					file.Load();
				}
			}
		}

		public void CloseFile(DmxFile file)
		{
			openedFilesList.Remove(file);

			IEnumerable<DmxFile> fileList = openedFilesList.Where(x => x.Name == file.Name);
			bool showFullName = fileList.Count() != 1;
			foreach (DmxFile dmxFile in fileList)
			{
				dmxFile.ShowFullName = showFullName;
			}
		}

		public void SetSelectedFile(DmxFile file)
		{
			foreach (DmxFile dmxFile in openedFilesList)
			{
				dmxFile.Selected = dmxFile == file;
			}
		}

		public DmxFile GetSelectedFile()
		{
			if (openedFilesList.Count > 0)
			{
				foreach (DmxFile dmxFile in openedFilesList)
				{
					if (dmxFile.Selected)
					{
						return dmxFile;
					}
				}
			}
			return null;
		}

		public IEnumerable<int> CorrectSaldo(IEnumerable<FullBackDataModel> backDataModels)
		{
			int saldo = int.TryParse(ActSaldo, out int tempSaldo) ? tempSaldo : 0;
			List<StructContainer<int>> openList = new List<StructContainer<int>>(backDataModels.Select(x => (StructContainer<int>)(int.TryParse(x.SumText, out int s) ? s : 0)));
			List<StructContainer<int>> closeList = new List<StructContainer<int>>(openList);

			while (saldo != 0 && closeList.Count > 0)
			{
				int partOfSaldo = saldo / closeList.Count;

				foreach (StructContainer<int> sumContainer in closeList)
				{
					sumContainer.Obj += partOfSaldo;
					saldo -= partOfSaldo;
				}
				StructContainer<int> lastContainer = closeList.LastOrDefault();
				if (lastContainer != null)
				{
					lastContainer.Obj += saldo;
					saldo -= saldo;
				}

				List<StructContainer<int>> deletionList = new List<StructContainer<int>>();
				foreach (StructContainer<int> sumContainer in closeList)
				{
					if ((int)sumContainer <= 0)
					{
						saldo += (int)sumContainer;
						sumContainer.Obj = 0;
						deletionList.Add(sumContainer);
					}
				}
				foreach (StructContainer<int> sumContainer in deletionList)
				{
					closeList.Remove(sumContainer);
				}
			}

			return openList.Select(x => (int)x);
		}

		public IEnumerable<int> CorrectDevelopment(int minSum, bool takeSumFromSupport, bool isRemoveIdenticalNumbers, List<FullBackDataModel> models)
		{
			List<int> developmentInput = new List<int>();
			List<int> supportInput = new List<int>();

			foreach (FullBackDataModel model in models)
				(model.IsRework ? supportInput : developmentInput).Add(int.TryParse(model.SumText, out int s) ? s : 0);

			MixingSumAlgorithm.MoreNumber(ref developmentInput, ref supportInput, minSum, takeSumFromSupport, isRemoveIdenticalNumbers, false);

			List<int> result = new List<int>();
			IEnumerator<int> developmentInputEnum = developmentInput.GetEnumerator();
			IEnumerator<int> supportInputEnum = supportInput.GetEnumerator();
			developmentInputEnum.MoveNext();
			supportInputEnum.MoveNext();
			foreach (FullBackDataModel model in models)
			{
				IEnumerator<int> enumerator = model.IsRework ? supportInputEnum : developmentInputEnum;
				result.Add(enumerator.Current);
				enumerator.MoveNext();
			}
			return result;
		}

		public IEnumerable<int> CorrectSupport(int minSum, bool takeSumFromSupport, bool isCreateNewWorks, bool isRemoveIdenticalNumbers, List<FullBackDataModel> models, out List<KeyValuePair<FullBackDataModel, int>> newModels)
		{
			List<int> developmentInput = new List<int>();
			List<int> supportInput = new List<int>();

			foreach (FullBackDataModel model in models)
				(model.IsRework ? supportInput : developmentInput).Add(int.TryParse(model.SumText, out int s) ? s : 0);

			List<FullBackDataModel> enableWorks = new List<FullBackDataModel>();
			if (isCreateNewWorks)
			{
				enableWorks.AddRange(GetEnableModelsWithWork(models));
			}
			int supportCount = supportInput.Count;
			MixingSumAlgorithm.LessNumber(ref developmentInput, ref supportInput, minSum, takeSumFromSupport, isRemoveIdenticalNumbers, isCreateNewWorks, enableWorks.Count);
			List<int> newSupport = new List<int>();
			while (supportInput.Count > supportCount)
			{
				newSupport.Insert(0, supportInput[supportInput.Count - 1]);
				supportInput.RemoveAt(supportInput.Count - 1);
			}

			List<int> result = new List<int>();
			IEnumerator<int> developmentInputEnum = developmentInput.GetEnumerator();
			IEnumerator<int> supportInputEnum = supportInput.GetEnumerator();
			developmentInputEnum.MoveNext();
			supportInputEnum.MoveNext();
			foreach (FullBackDataModel model in models)
			{
				IEnumerator<int> enumerator = model.IsRework ? supportInputEnum : developmentInputEnum;
				result.Add(enumerator.Current);
				enumerator.MoveNext();
			}

			newModels = new List<KeyValuePair<FullBackDataModel, int>>();
			if (newSupport.Count > 0)
			{
				Random random = new Random();

				List<int>.Enumerator newSupportEnum = newSupport.GetEnumerator();
				while (newSupportEnum.MoveNext())
				{
					int index = random.Next(enableWorks.Count);

					FullBackDataModel model = enableWorks[index];
					enableWorks.RemoveAt(index);
					newModels.Add(new KeyValuePair<FullBackDataModel, int>(model, newSupportEnum.Current));
				}
			}

			return result;
		}

		private IEnumerable<FullBackDataModel> GetEnableModelsWithWork(List<FullBackDataModel> models)
		{
			foreach (FullBackDataModel model in models)
			{
				IEnumerable<WorkObject> enableWorks = GetEnableWorks(model, models);
				foreach (WorkObject workObject in enableWorks)
				{
					yield return new FullBackDataModel(model, workObject);
				}
			}
		}

		private IEnumerable<WorkObject> GetEnableWorks(FullBackDataModel current, List<FullBackDataModel> models)
		{
			List<WorkObject> res = new List<WorkObject>(current.WorkTypesList);

			foreach (FullBackDataModel model in models)
			{
				if (model == current || current.EqualsWithoutWork(model, TemplateType))
				{
					res.RemoveAll(x => x.Id == model.WorkObjectId);
				}
			}

			return res;
		}

		public void RandomizeWorkTypes(IEnumerable<KeyValuePair<bool, FullBackDataModel>> backDatas)
		{
			BackDataRandomizer.ByWorkTypeName(backDatas, TemplateType);
		}

		public string GetDcmkFileName()
		{
			return SelectedHuman + DcmkFile.Extension;
		}

		public void ExportDcmk(string path, IEnumerable<FullBackDataModel> backModels)
		{
			XmlSaver saver = new XmlSaver();
			saver.AppendAllProperties(this, true);

			foreach (FullBackDataModel backDataModel in backModels)
			{
				saver.CreateBackNode();
				saver.AppendAllBackProperties(backDataModel);
				saver.PushBackNode();
			}

			saver.Save(path);
		}

		public void EnableActionsStacking()
		{
			actionsStack.ActionsStackingEnabled = true;
		}

		public void DisableActionsStacking()
		{
			actionsStack.ActionsStackingEnabled = false;
		}

		public void Redo()
		{
			actionsStack.Redo();
		}

		public void Undo()
		{
			actionsStack.Undo();
		}

		public void AddUndoRedoLink(IUndoRedoAction action)
		{
			actionsStack.AddLinkToLast(action);
		}

		public IUndoRedoActionsStack GetActionsStack()
		{
			return actionsStack;
		}

		public void RemoveFromActionsStack(IEnumerable<FullBackDataModel> models)
		{
			foreach (FullBackDataModel model in models)
			{
				actionsStack.RemoveActionsWithTarget(model);
			}
		}

		public void SubscribeActionPushed(UndoRedoActionPushedHandler action)
		{
			actionsStack.SubscribePushed(action);
		}

		public void ResetWeight(IEnumerable<FullBackDataModel> models)
		{
			double totalTime = 0;
			List<double> times = new List<double>();

			foreach (FullBackDataModel model in models)
			{
				int time = 0;
				if (int.TryParse(model.SpentTimeText, out int spentTime))
				{
					time = spentTime;
				}
				totalTime += time;
				times.Add(time);
			}

			IEnumerator<double> timesEnum = times.GetEnumerator();
			IEnumerator<FullBackDataModel> backDataModelsEnum = models.GetEnumerator();

			while (timesEnum.MoveNext() && backDataModelsEnum.MoveNext())
			{
				backDataModelsEnum.Current.Weight = timesEnum.Current / totalTime;
			}
		}

		public void ClearUndoRedo()
		{
			actionsStack.Clear();
		}

		public void ChangeTechnicalTaskDateAtAllFiles(string technicalTaskDateText)
		{
			foreach (DmxFile file in openedFilesList)
			{
				file.TechnicalTaskDateText = technicalTaskDateText;
			}
		}

		public void ChangeActDateAtAllFiles(string actDateText)
		{
			foreach (DmxFile file in openedFilesList)
			{
				file.ActDateText = actDateText;
			}
		}

		public void ChangeContractFileAtAllFiles(string nameFile)
		{
			foreach (DmxFile file in openedFilesList)
			{
				file.SelectedContractFile = nameFile;
			}
		}

		public void ChangeGameNameAtAllFiles(string sourceGameName, string newGameName, string sourceEpisode, string newEpisode)
		{
			foreach (DmxFile file in openedFilesList)
			{
				foreach (FullBackDataModel fullBackDataModel in file.BackDataModels)
				{
					if (fullBackDataModel.Type != BackType.Other)
					{
						if (fullBackDataModel.GameName == sourceGameName && (sourceEpisode == null || fullBackDataModel.EpisodeNumberText == sourceEpisode))
						{
							fullBackDataModel.GameName = newGameName;
							if (sourceEpisode != null)
								fullBackDataModel.EpisodeNumberText = newEpisode;
						}
					}
				}
			}
		}
	}
}
