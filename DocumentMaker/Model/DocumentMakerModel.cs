﻿using Dml;
using Dml.Model;
using Dml.Model.Files;
using Dml.Model.Session;
using Dml.Model.Template;
using DocumentMaker.Model.OfficeFiles;
using DocumentMaker.Model.OfficeFiles.Human;
using DocumentMaker.Resources;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace DocumentMaker.Model
{
	public class DocumentMakerModel
	{
		private readonly OfficeExporter exporter;
		private readonly ObservableCollection<DocumentTemplate> documentTemplates;
		private ObservableRangeCollection<HumanData> humanFullNameList;
		private ObservableRangeCollection<DmxFile> openedFilesList;

		public DocumentMakerModel()
		{
			exporter = new OfficeExporter();
			documentTemplates = new ObservableCollection<DocumentTemplate>
			{
				new DocumentTemplate { Name = "Скриптувальник", Type = DocumentTemplateType.Scripter, },
				new DocumentTemplate { Name = "Різник", Type = DocumentTemplateType.Cutter, },
				new DocumentTemplate { Name = "Художник", Type = DocumentTemplateType.Painter, },
				new DocumentTemplate { Name = "Моделлер", Type = DocumentTemplateType.Modeller, },
			};
			humanFullNameList = new ObservableRangeCollection<HumanData>();
			openedFilesList = new ObservableRangeCollection<DmxFile>();
		}

		public IList<DmxFile> OpenedFilesList => openedFilesList;
		public DocumentTemplateType TemplateType { get; set; }
		public string TechnicalTaskDateText { get; set; }
		public string ActDateText { get; set; }
		public string AdditionNumText { get; set; }
		public string SelectedHuman { get; set; }
		public string HumanIdText { get; set; }
		public string AddressText { get; set; }
		public string PaymentAccountText { get; set; }
		public string BankName { get; set; }
		public string MfoText { get; set; }
		public string ContractNumberText { get; set; }
		public string ContractDateText { get; set; }
		public IList<DocumentTemplate> DocumentTemplatesList => documentTemplates;
		public IList<HumanData> HumanFullNameList => humanFullNameList;
		public bool HasNoMovedFiles => exporter.HasNoMovedFiles;

		public void Save(string path, IEnumerable<BackDataModel> backModels)
		{
			XmlSaver saver = new XmlSaver();
			saver.AppendAllProperties(this);

			foreach (BackDataModel backDataModel in backModels)
			{
				saver.CreateBackNode();
				saver.AppendAllBackProperties(backDataModel);
				saver.PushBackNode();
			}

			saver.Save(path);
		}

		public void Load(string path, out List<BackDataModel> backModels)
		{
			backModels = new List<BackDataModel>();

			XmlLoader loader = new XmlLoader();
			if (loader.TryLoad(path))
			{
				loader.SetLoadedProperties(this);
				loader.SetLoadedBacksProperties(backModels);
			}
		}

		public void LoadHumans(string path)
		{
			XlsxLoader loader = new XlsxLoader();
			loader.LoadHumans(path, humanFullNameList);
		}

		public void Export(string path, IEnumerable<BackDataModel> backModels)
		{
			DocumentGeneralData generalData = new DocumentGeneralData(this);
			DocumentTableData tableData = new DocumentTableData(backModels, TemplateType);

			foreach (ResourceInfo resource in DocumentResourceManager.Items)
			{
				exporter.Clear();

				if (resource.Type == ResourceType.Docx)
				{
					exporter.ExportWordTemplate(resource.ProjectName);
					exporter.FillWordGeneralData(generalData);
					exporter.FillWordTableData(tableData);
					exporter.SaveWordContent(resource.ProjectName);
				}
				else if (resource.Type == ResourceType.Xlsx)
				{
					exporter.ExportExcelTemplate(resource.ProjectName);
					exporter.FillExcelTableData(resource.ProjectName, tableData);
				}

				exporter.SaveTemplate(generalData, path, resource.ProjectName, resource.TemplateName);
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

					if (File.Exists(file) && file.EndsWith(DmxFile.Extension))
					{
						bool isAdd = true;
						DmxFile dmxFile = new DmxFile(file);

						IList<DmxFile> fileList = openedFilesList.Where(f => f.Name == dmxFile.Name).ToList();
						foreach(DmxFile f in fileList)
						{
							if(f.FullName == dmxFile.FullName)
							{
								isAdd = false;
								break;
							}
						}

						if(isAdd)
						{
							adding.Add(dmxFile);
							dmxFile.ShowFullName = fileList.Count > 0;
							
							foreach(DmxFile f in fileList)
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
			foreach(DmxFile file in openedFilesList)
			{
				if(!file.Loaded)
				{
					file.Load();
				}
			}
		}
	}
}
