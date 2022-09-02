using Dml;
using Dml.Model.Back;
using Dml.Model.Files;
using Dml.Model.Session;
using Dml.Model.Session.Attributes;
using Dml.Model.Template;
using DocumentMaker.Model.Algorithm;
using DocumentMaker.Model.Controls;
using DocumentMaker.Model.Files;
using DocumentMaker.Model.OfficeFiles;
using DocumentMaker.Model.OfficeFiles.Human;
using DocumentMaker.Model.Session;
using DocumentMaker.Resources;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace DocumentMaker.Model
{
	public class DocumentMakerModel
	{
		private readonly OfficeExporter exporter;
		private readonly ObservableCollection<FullDocumentTemplate> documentTemplates;
		private readonly ObservableRangeCollection<HumanData> humanFullNameList;
		private readonly ObservableRangeCollection<DmxFile> openedFilesList;
		private readonly List<GameObject> gameNameList;

		public DocumentMakerModel()
		{
			exporter = new OfficeExporter();
			documentTemplates = new ObservableCollection<FullDocumentTemplate>
			{
				new FullDocumentTemplate { Name = "Скриптувальник", Type = DocumentTemplateType.Scripter, },
				new FullDocumentTemplate { Name = "Технічний дизайнер", Type = DocumentTemplateType.Cutter, },
				new FullDocumentTemplate { Name = "Художник", Type = DocumentTemplateType.Painter, },
				new FullDocumentTemplate { Name = "Моделлер", Type = DocumentTemplateType.Modeller, },
			};
			humanFullNameList = new ObservableRangeCollection<HumanData>();
			openedFilesList = new ObservableRangeCollection<DmxFile>();
			gameNameList = new List<GameObject>();
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
		public string CorrectSupportWindow_NumberText { get; set; } = "2100";
		[IsNotDmxContent]
		public bool CorrectSupportWindow_TakeSumFromDevelopment { get; set; } = false;

		#endregion

		public IList<DmxFile> OpenedFilesList => openedFilesList;
		public DocumentTemplateType TemplateType { get; set; } = DocumentTemplateType.Empty;
		public string TechnicalTaskDateText { get; set; }
		public string ActDateText { get; set; }
		public string TechnicalTaskNumText { get; set; } = "1";
		public string SelectedHuman { get; set; }
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
		public string ActSum { get; set; }
		public string ActSaldo { get; set; }
		public bool NeedUpdateSum { get; set; }
		public IList<FullDocumentTemplate> DocumentTemplatesList => documentTemplates;
		public IList<HumanData> HumanFullNameList => humanFullNameList;
		public bool HasNoMovedFiles => exporter.HasNoMovedFiles;
		public IList<GameObject> GameNameList => gameNameList;

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

				DocumentGeneralData generalData = new DocumentGeneralData(this, isExportRework, actSum);
				DocumentTableData tableData = new DocumentTableData(backModels, TemplateType, isExportRework);
				if (tableData.Count() <= 0) continue;
				foreach (ResourceInfo resource in DocumentResourceManager.GetItems(isExportRework))
				{
					exporter.Clear();

					if (resource.Type == ResourceType.Docx)
					{
						string nearFullname = exporter.ExportWordTemplate(resource.ProjectName, isExportRework);
						exporter.FillWordGeneralData(generalData);
						exporter.FillWordTableData(tableData);
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

						IList<DmxFile> fileList = openedFilesList.Where(f => f.Name == dmxFile.Name).ToList();
						foreach (DmxFile f in fileList)
						{
							if (f.FullName == dmxFile.FullName)
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

		public void CorrectSaldo(IEnumerable<FullBackDataModel> backDataModels)
		{
			double baseSum = 0;
			if (int.TryParse(ActSum, out int sum))
			{
				baseSum = sum;
			}

			double totalWeight = 0;
			foreach (FullBackDataModel backModel in backDataModels)
			{
				if (uint.TryParse(backModel.SumText, out uint curSum))
				{
					totalWeight += curSum / baseSum;
				}
			}

			int curTotalSum = (int)baseSum;
			foreach (FullBackDataModel backModel in backDataModels)
			{
				double curWeight = 0;
				if (uint.TryParse(backModel.SumText, out uint curSum))
				{
					curWeight = curSum / baseSum;
					curWeight /= totalWeight;
				}
				int newCurSum = (int)(baseSum * curWeight);
				backModel.SumText = newCurSum.ToString();
				curTotalSum -= newCurSum;
			}

			FullBackDataModel firstModel = backDataModels.FirstOrDefault();
			if (firstModel != null && int.TryParse(firstModel.SumText, out int s))
			{
				firstModel.SumText = (s + curTotalSum).ToString();
			}
		}

		public void CorrectDevelopment(int minSum, bool takeSumFromSupport, List<FullBackDataModel> developmentModels, List<FullBackDataModel> supportModels)
		{
			List<int> developmentInput = new List<int>(GetSumsFromModels(developmentModels)),
				supportInput = new List<int>(GetSumsFromModels(supportModels));

			MixingSumAlgorithm.MoreNumber(ref developmentInput, ref supportInput, minSum, takeSumFromSupport, true, false);

			SetSumsToModels(developmentModels, developmentInput);
			SetSumsToModels(supportModels, supportInput);
		}

		public void CorrectSupport(int minSum, bool takeSumFromSupport, List<FullBackDataModel> developmentModels, List<FullBackDataModel> supportModels)
		{
			List<int> developmentInput = new List<int>(GetSumsFromModels(developmentModels)),
				supportInput = new List<int>(GetSumsFromModels(supportModels));

			MixingSumAlgorithm.LessNumber(ref developmentInput, ref supportInput, minSum, takeSumFromSupport, true, false);

			SetSumsToModels(developmentModels, developmentInput);
			SetSumsToModels(supportModels, supportInput);
		}

		private IEnumerable<int> GetSumsFromModels(List<FullBackDataModel> backData)
		{
			foreach (FullBackDataModel model in backData)
			{
				if (int.TryParse(model.SumText, out int sum))
				{
					yield return sum;
				}
				else
				{
					yield return 0;
				}
			}
		}

		private void SetSumsToModels(List<FullBackDataModel> backData, List<int> sums)
		{
			IEnumerator<int> sumsEnum = sums.GetEnumerator();
			IEnumerator<FullBackDataModel> backDataEnum = backData.GetEnumerator();
			while (sumsEnum.MoveNext() && backDataEnum.MoveNext())
			{
				backDataEnum.Current.SumText = sumsEnum.Current.ToString();
			}
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
	}
}
