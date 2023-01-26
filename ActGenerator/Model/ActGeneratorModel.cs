using ActGenerator.Model.Controls;
using ActGenerator.ViewModel.Dialogs;
using DocumentMakerModelLibrary;
using DocumentMakerModelLibrary.Algorithm;
using DocumentMakerModelLibrary.Back;
using DocumentMakerModelLibrary.Controls;
using DocumentMakerModelLibrary.Files;
using DocumentMakerModelLibrary.OfficeFiles;
using ProjectEditorLib.Model;
using ProjectsDb;
using ProjectsDb.Context;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ActGenerator.Model
{
	class ActGeneratorModel : ProjectsDbConnector
	{
		delegate Task GeneratingPart(GenerationDialogViewModel dialogContext, double totalProgressPart);

		const string dateFormat = "dd.MM.yyyy";

		List<GeneratingPart> generatingParts = new List<GeneratingPart>();
		Random random = new Random();

		// contain Project or AlternativeProjectName with reference to Project
		List<IDbObject> projects = null;
		IEnumerable<HumanListItemControlModel> humen = null;
		DateTime ignoringActDate = new DateTime(1, 1, 1);
		DateTime minActDateIgnoring = DateTime.Now;
		List<DcmkFile> dcmkFiles = null;
		int minSum = default;
		int maxSum = default;
		string savingPath = null;
		bool isCollapseRegionsWorks = default;
		DateTime technicalTaskDate = DateTime.MinValue;
		DateTime actDate = DateTime.MinValue;

		bool needGenarateWorks = true;

		List<BackType> loadedBackTypes = new List<BackType>();
		List<Back> loadedBacks = new List<Back>();

		List<FullDocumentTemplate> documentTemplates = null;

		List<GeneratedWorkList> generatedWorkLists = null;
		Dictionary<HumanListItemControlModel, List<GeneratedWorkList>> acts = null;
		List<string> generatedActNames = null;

		public ActGeneratorModel()
		{
			generatingParts.Add(LoadingProjects);
			generatingParts.Add(GeneratingAllWorks);
			generatingParts.Add(RemovingUsedWorks);
			generatingParts.Add(GeneratingAllRegionsWork);
			generatingParts.Add(GeneratingActs);
			generatingParts.Add(SavingActs);
		}

		public bool WorksGenerated => !needGenarateWorks;

		public void SetProjects(List<IDbObject> projects)
		{
			if(!needGenarateWorks)
			{
				if(this.projects == null || this.projects.Count != projects.Count)
				{
					needGenarateWorks = true;
				}
				else
				{
					IEnumerator<IDbObject> thisProjectsEnum = this.projects.GetEnumerator();
					IEnumerator<IDbObject> projectsEnum = projects.GetEnumerator();

					while(thisProjectsEnum.MoveNext() && projectsEnum.MoveNext())
					{
						if (thisProjectsEnum.Current.Id != projectsEnum.Current.Id || thisProjectsEnum.Current.GetType() != projectsEnum.Current.GetType())
						{
							needGenarateWorks = true;
							break;
						}
					}
				}
			}

			this.projects = projects;
		}
		
		public void SetHumen(IEnumerable<HumanListItemControlModel> humen)
		{
			this.humen = humen;
			List<FullDocumentTemplate> newDocumentTemplates = humen.SelectMany(x => x.SelectedTemplates).Distinct().ToList();
			if(!needGenarateWorks)
			{
				if(documentTemplates == null || documentTemplates.Count != newDocumentTemplates.Count)
				{
					needGenarateWorks = true;
				}
				else
				{
					IEnumerator<FullDocumentTemplate> newTemplatesEnum = newDocumentTemplates.GetEnumerator();
					IEnumerator<FullDocumentTemplate> templatesEnum = documentTemplates.GetEnumerator();

					while(newTemplatesEnum.MoveNext() && templatesEnum.MoveNext())
					{
						if(newTemplatesEnum.Current.Type != templatesEnum.Current.Type)
						{
							needGenarateWorks = true;
							break;
						}
					}
				}
			}
			documentTemplates = newDocumentTemplates;
		}

		public void SetDocumentList(List<DcmkFile> dcmkFiles)
		{
			if (!needGenarateWorks)
			{
				if(this.dcmkFiles == null || this.dcmkFiles.Count != dcmkFiles.Count)
				{
					needGenarateWorks = true;
				}
				else
				{
					IEnumerator<DcmkFile> thisDcmkFilesEnum = this.dcmkFiles.GetEnumerator();
					IEnumerator<DcmkFile> dcmkFilesEnum = dcmkFiles.GetEnumerator();

					while(thisDcmkFilesEnum.MoveNext() && dcmkFilesEnum.MoveNext())
					{
						if(thisDcmkFilesEnum.Current.FullName != dcmkFilesEnum.Current.FullName)
						{
							needGenarateWorks = true;
							break;
						}
					}
				}
			}

			this.dcmkFiles = dcmkFiles;
		}

		public void SetSumLimits(int minSum, int maxSum)
		{
			this.minSum = minSum;
			this.maxSum = maxSum;
		}

		public void SetSavingPath(string path)
		{
			savingPath = path;
		}

		public void SetIsCollapseRegionsWorks(bool isCollapseRegionsWorks)
		{
			if(!needGenarateWorks && this.isCollapseRegionsWorks != isCollapseRegionsWorks)
			{
				needGenarateWorks = true;
			}
			this.isCollapseRegionsWorks = isCollapseRegionsWorks;
		}

		public void SetIgnoringActDate(DateTime ignoringActDate)
		{
			if(!needGenarateWorks && this.ignoringActDate != ignoringActDate)
			{
				needGenarateWorks = true;
			}
			this.ignoringActDate = ignoringActDate;
			minActDateIgnoring = DateTime.Now
				.AddDays(-(this.ignoringActDate.Day - 1))
				.AddMonths(-(this.ignoringActDate.Month - 1))
				.AddYears(-(this.ignoringActDate.Year - 1));
		}

		public void SetDates(DateTime technicalTaskDate, DateTime actDate)
		{
			this.technicalTaskDate = technicalTaskDate;
			this.actDate = actDate;
		}

		public async Task StartGeneration(GenerationDialogViewModel dialogContext)
		{
			try
			{
				double progressPart = dialogContext.Dispatcher.Invoke(() => dialogContext.ProgressMaximum) / generatingParts.Count;

				foreach (GeneratingPart generatingPart in generatingParts)
				{
					await generatingPart(dialogContext, progressPart);

					if (dialogContext.IsClosing)
					{
						await DisconnectDbAsync();
						return;
					}
				}

				dialogContext.Dispatcher.Invoke(() => dialogContext.ProgressValue = dialogContext.ProgressMaximum);
				await SetProcessDescription(dialogContext, "Згенеровано!");
				needGenarateWorks = false;
			}
			catch (Exception e)
			{
				dialogContext.Dispatcher.Invoke(() => System.Windows.MessageBox.Show(e.ToString(), "Генерація | Помилка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error));
			}
		}

		private async Task LoadingProjects(GenerationDialogViewModel dialogContext, double totalProgressPart)
		{
			if (await IsSkipGenerating(dialogContext, totalProgressPart)) return;

			await ConnectDbAsync();

			loadedBackTypes.Clear();
			loadedBackTypes.AddRange(db.BackTypes.ToList());
			loadedBacks.Clear();

			double progressPart = totalProgressPart / projects.Count;
			foreach (IDbObject dbObject in projects)
			{
				if (IsBreakGeneration(dialogContext)) break;
				await SetProcessDescription(dialogContext, "Завантаження проєкту \"" + dbObject.ToString() + '"');

				Project project = dbObject as Project ?? ((AlternativeProjectName)dbObject).Project;

				Project prevLoadedProject = GetPrevProject(dbObject);
				if (prevLoadedProject != null)
				{
					project.Backs = prevLoadedProject.Backs;
					await AddPogress(dialogContext, progressPart);
				}
				else
				{
					await LoadProject(project, dialogContext, progressPart);
				}
			}

			await DisconnectDbAsync();
		}

		private async Task LoadProject(Project project, GenerationDialogViewModel dialogContext, double totalProgressPart)
		{
			double startProgress = dialogContext.Dispatcher.Invoke(() => dialogContext.ProgressValue);
			double progressPart = totalProgressPart / db.Backs.Count(x => x.DeletionDate == null && x.ProjectId == project.Id);

			project.Backs = new List<Back>();
			foreach (Back back in db.Backs.Where(x => x.DeletionDate == null && x.ProjectId == project.Id).ToList())
			{
				if (IsBreakGeneration(dialogContext)) return;

				project.Backs.Add(back);
				loadedBacks.Add(back);
				back.BackType = loadedBackTypes.First(y => y.Id == back.BackTypeId);
				back.Regions = db.CountRegions.Where(y => y.DeletionDate == null && y.BackId == back.Id).ToList();

				await AddPogress(dialogContext, progressPart);
			}

			List<Back> episodes = project.Backs.Where(x => x.BackType.Name == ProjectNodeType.Episode.ToString()).ToList();
			episodes.ForEach(x => project.Backs.Remove(x));
			foreach (Back back in project.Backs)
			{
				back.BaseBack = episodes.FirstOrDefault(x => x.Id == back.BaseBackId);

				foreach(CountRegions countRegions in back.Regions)
				{
					countRegions.Back = back;
				}
			}

			await SetProgress(dialogContext, startProgress + totalProgressPart);
		}

		private Project GetPrevProject(IDbObject currentDbObject)
		{
			int? projectId = currentDbObject is Project project ? project.Id : ((AlternativeProjectName)currentDbObject).ProjectId;

			foreach (IDbObject loadedDbObject in projects)
			{
				if (loadedDbObject == currentDbObject)
				{
					return null;
				}
				Project loadedProject = loadedDbObject as Project ?? ((AlternativeProjectName)loadedDbObject).Project;

				if (projectId == loadedProject.Id)
				{
					return loadedProject;
				}
			}
			return null;
		}

		private async Task GeneratingAllWorks(GenerationDialogViewModel dialogContext, double totalProgressPart)
		{
			if (await IsSkipGenerating(dialogContext, totalProgressPart)) return;

			generatedWorkLists = new List<GeneratedWorkList>();
			double progressPart = totalProgressPart
				/ projects.SelectMany(x => x is Project project ? project.Backs : ((AlternativeProjectName)x).Project.Backs).Count()
				/ documentTemplates.SelectMany(y => y.ReworkWorkTypesList).Count();

			foreach (IDbObject dbObject in projects)
			{
				Project project = dbObject as Project;
				if (project == null) project = ((AlternativeProjectName)dbObject).Project;

				await SetProcessDescription(dialogContext, "Генерація робіт проєкту \"" + dbObject.ToString() + '"');
				GeneratedWorkList generatedWorkList = new GeneratedWorkList
				{
					Project = dbObject,
				};

				Project prevProject = GetPrevProject(dbObject);
				if (prevProject != null)
				{
					foreach(GeneratedWorkList prevWorkList in generatedWorkLists)
					{
						if(prevProject.Id == (prevWorkList.Project is AlternativeProjectName alternativeProjectName ? alternativeProjectName.ProjectId : prevWorkList.Project.Id))
						{
							generatedWorkList.CopyWorks(prevWorkList.GeneratedWorks);
							double skippedProgress = progressPart * generatedWorkList.GeneratedWorks.Count;
							await AddPogress(dialogContext, skippedProgress);
							break;
						}
					}
				}
				else
				{
					foreach (Back back in project.Backs)
					{
						if (IsBreakGeneration(dialogContext)) break;

						foreach (FullDocumentTemplate documentTemplate in documentTemplates)
						{
							foreach (WorkObject workObject in documentTemplate.ReworkWorkTypesList)
							{
								GeneratedWork generatedWork = new GeneratedWork
								{
									WorkObject = workObject,
									Back = back,
									DocumentTemplate = documentTemplate,
								};

								CountRegions countRegions = back.Regions.FirstOrDefault();
								if(countRegions != null)
								{
									generatedWork.Regions = new List<int>();
									for (int i = 1; i <= countRegions.Count; ++i)
									{
										generatedWork.Regions.Add(i);
									}
								}

								generatedWorkList.AddGeneratedWork(generatedWork);

								await AddPogress(dialogContext, progressPart);
							}
						}
					}
				}
				generatedWorkLists.Add(generatedWorkList);
			}
		}

		private async Task RemovingUsedWorks(GenerationDialogViewModel dialogContext, double totalProgressPart)
		{
			if (await IsSkipGenerating(dialogContext, totalProgressPart)) return;

			await SetProcessDescription(dialogContext, "Видалення використаних робіт");
			double startProgress = dialogContext.Dispatcher.Invoke(() => dialogContext.ProgressValue);
			double progressPart = totalProgressPart
				/ dcmkFiles.SelectMany(x => x.BackDataModels).Count()
				/ generatedWorkLists.Count;

			foreach(DcmkFile dcmkFile in dcmkFiles)
			{
				if (DateTime.TryParse(dcmkFile.ActDateText, out DateTime actDate)
					&& actDate > minActDateIgnoring
					&& documentTemplates.FirstOrDefault(x => x.Type == dcmkFile.TemplateType) != null)
				{
					foreach (FullBackDataModel back in dcmkFile.BackDataModels)
					{
						if (IsBreakGeneration(dialogContext)) return;

						if (back.IsRework)
						{
							foreach (GeneratedWorkList workList in generatedWorkLists)
							{
								if (IsBreakGeneration(dialogContext)) return;

								if (back.GameName == workList.Project.ToString())
								{
									foreach(GeneratedWork generatedWork in workList.GeneratedWorks.SelectMany(x => x.Value))
									{
										if (back.WorkObjectId == generatedWork.WorkObject.Id
											&& IsEqualTypes(back.Type, generatedWork.Back.BackType)
											&& (generatedWork.Back.BaseBack == null || generatedWork.Back.BaseBack.Number.ToString() == back.EpisodeNumberText)
											&& ((generatedWork.Back.BackType.Name == ProjectNodeType.Back.ToString()
													&& back.BackNumberText == generatedWork.Back.Number.ToString()
													&& back.BackName == generatedWork.Back.Name)
												|| (back.BackName == generatedWork.Back.Number.ToString() + ". " + generatedWork.Back.Name)))
										{
											GeneratedWork changedGeneratedWork = generatedWork.Clone();

											string regs = Regex.Replace(BackTaskStrings.GetRegionString(back.Type, back.BackCountRegionsText), @"\s+", "");
											if (string.IsNullOrEmpty(regs))
											{
												changedGeneratedWork.BackUsed = true;
											}
											else
											{
												int[] iregs = regs.Split(',').Select(x => int.Parse(x)).ToArray();
												foreach(int reg in iregs)
												{
													changedGeneratedWork.Regions.Remove(reg);
												}
											}

											if(changedGeneratedWork.ContainWork())
											{
												workList.InsertGeneratedWork(0, changedGeneratedWork);
											}
											workList.RemoveGeneratedWork(generatedWork);
											break;
										}
									}
								}

								await AddPogress(dialogContext, progressPart);
							}
						}
					}
				}
			}

			await SetProgress(dialogContext, startProgress + totalProgressPart);
		}

		private bool IsEqualTypes(Dml.Model.Back.BackType backType, BackType dbBackType)
		{
			return Enum.TryParse(dbBackType.Name, out ProjectNodeType projectNodeType)
				&& (((backType == Dml.Model.Back.BackType.Back || backType == Dml.Model.Back.BackType.Regions) && projectNodeType == ProjectNodeType.Back)
					|| (backType == Dml.Model.Back.BackType.Dialog && projectNodeType == ProjectNodeType.Dialog)
					|| (backType == Dml.Model.Back.BackType.Mg && projectNodeType == ProjectNodeType.Minigame)
					|| ((backType == Dml.Model.Back.BackType.Hog || backType == Dml.Model.Back.BackType.HogRegions) && projectNodeType == ProjectNodeType.Hog)
					|| (backType == Dml.Model.Back.BackType.Craft && projectNodeType == ProjectNodeType.Craft));
		}

		private async Task GeneratingAllRegionsWork(GenerationDialogViewModel dialogContext, double totalProgressPart)
		{
			if (await IsSkipGenerating(dialogContext, totalProgressPart)) return;

			await SetProcessDescription(dialogContext, "Розгортання регіонів");
			double progressPart = totalProgressPart / 2 / generatedWorkLists.SelectMany(x => x.GeneratedWorks.SelectMany(y => y.Value)).Count();

			foreach (GeneratedWorkList generatedWorkList in generatedWorkLists)
			{
				List<GeneratedWork> newGeneratedWorks = new List<GeneratedWork>();

				foreach (GeneratedWork generatedWork in generatedWorkList.GeneratedWorks.SelectMany(x => x.Value))
				{
					if (IsBreakGeneration(dialogContext)) return;

					if (!generatedWork.BackUsed && generatedWork.Regions != null && generatedWork.Regions.Count > 0)
					{
						GeneratedWork backWork = generatedWork.Clone();
						backWork.Regions.Clear();
						newGeneratedWorks.Add(backWork);

						if(isCollapseRegionsWorks)
						{
							GeneratedWork regionsWork = generatedWork.Clone();
							regionsWork.BackUsed = true;
							newGeneratedWorks.Add(regionsWork);
						}
						else
						{
							foreach(int regNum in generatedWork.Regions)
							{
								GeneratedWork regionWork = generatedWork.Clone();
								regionWork.BackUsed = true;
								regionWork.Regions.Clear();
								regionWork.Regions.Add(regNum);
								newGeneratedWorks.Add(regionWork);
							}
						}
					}
					else
					{
						newGeneratedWorks.Add(generatedWork);
					}

					await AddPogress(dialogContext, progressPart);
				}

				double localProgressPart = totalProgressPart / 2 / generatedWorkLists.Count / newGeneratedWorks.Count;

				generatedWorkList.ClearWorks();
				foreach(GeneratedWork work in newGeneratedWorks)
				{
					if (IsBreakGeneration(dialogContext)) return;

					generatedWorkList.AddGeneratedWork(work);

					await AddPogress(dialogContext, localProgressPart);
				}
			}
		}

		private async Task GeneratingActs(GenerationDialogViewModel dialogContext, double totalProgressPart)
		{
			const int parts = 3;
			double progressPart = totalProgressPart / humen.Count() / parts;

			acts = new Dictionary<HumanListItemControlModel, List<GeneratedWorkList>>();
			foreach (HumanListItemControlModel human in humen)
			{
				await SetProcessDescription(dialogContext, "Створення акту \"" + human.HumanData.Name + "\"");

				KeyValuePair<HumanListItemControlModel, List<GeneratedWorkList>> act =
					new KeyValuePair<HumanListItemControlModel, List<GeneratedWorkList>>(human, new List<GeneratedWorkList>());

				int sum = (int)human.Sum;
				int countEnableWorks = GetCountEnabledWorks(human.SelectedTemplates);
				int countWorks = GetRandomCountWorks(sum, countEnableWorks);

				if (countWorks > countEnableWorks || !TryGetActWithRandomWorks(dialogContext, countWorks, human.SelectedTemplates, act))
				{
					if (IsBreakGeneration(dialogContext)) return;

					dialogContext.Dispatcher.Invoke(() => System.Windows.MessageBox.Show("Не достатньо робіт для акту \"" + human.HumanData.Name + "\". Акт буде пропущений.", "Генерація", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning));
					ReturnBackUsedWorks(act);
					await AddPogress(dialogContext, progressPart * parts);
					continue;
				}

				await AddPogress(dialogContext, progressPart);

				List<int> sums = GetRandomSums(sum, countWorks);

				if (IsBreakGeneration(dialogContext)) return;

				await AddPogress(dialogContext, progressPart);

				MixingSumAlgorithm.RemoveIdenticalNumbers(ref sums, minSum, maxSum);
				if (IsBreakGeneration(dialogContext)) return;

				List<int>.Enumerator sumsEnum = sums.GetEnumerator();
				sumsEnum.MoveNext();
				foreach (GeneratedWork generatedWork in act.Value.SelectMany(x => x.GeneratedWorks.SelectMany(y => y.Value)))
				{
					if (IsBreakGeneration(dialogContext)) return;

					generatedWork.Sum = sumsEnum.Current;
					sumsEnum.MoveNext();
				}

				acts.Add(act.Key, act.Value);

				await AddPogress(dialogContext, progressPart);
			}
		}

		private int GetRandomCountWorks(int sum, int countEnableWorks)
		{
			int minCountWorks = GetMinCountWorks(sum);
			int maxCountWorks = (int)Math.Floor((double)sum / minSum);
			if (maxCountWorks > countEnableWorks) maxCountWorks = countEnableWorks;

			int countWorks = minCountWorks;
			if (minCountWorks < maxCountWorks)
			{
				countWorks = random.Next(minCountWorks, maxCountWorks + 1);
			}
			return countWorks;
		}

		public int GetMinCountWorks(int sum)
		{
			return (int)Math.Ceiling((double)sum / maxSum);
		}

		private bool TryGetActWithRandomWorks(GenerationDialogViewModel dialogContext, int countWorks, IEnumerable<FullDocumentTemplate> enableTemplates, KeyValuePair<HumanListItemControlModel, List<GeneratedWorkList>> act)
		{
			int countAddedWorks = 0;

			foreach (FullDocumentTemplate documentTemplate in enableTemplates)
			{
				if (IsBreakGeneration(dialogContext)) return false;
				
				Dictionary<GeneratedWorkList, List<GeneratedWork>> enableWorks = generatedWorkLists.ToDictionary(key => key, value => value.GeneratedWorks[documentTemplate]);
				int countEnableWorks = enableWorks.SelectMany(x => x.Value).Count();

				while (countEnableWorks > 0 && countAddedWorks < countWorks)
				{
					if (IsBreakGeneration(dialogContext)) return false;

					int randomWorkIndex = random.Next(0, countEnableWorks);

					foreach (KeyValuePair<GeneratedWorkList, List<GeneratedWork>> workList in enableWorks)
					{
						if (IsBreakGeneration(dialogContext)) return false;

						if (randomWorkIndex < workList.Value.Count)
						{
							GeneratedWork randomWork = workList.Value[randomWorkIndex];
							workList.Value.Remove(randomWork);
							--countEnableWorks;

							GeneratedWorkList generatedWorkList = act.Value.FirstOrDefault(x => x.Project == workList.Key.Project);
							if (generatedWorkList == null)
							{
								generatedWorkList = new GeneratedWorkList { Project = workList.Key.Project };
								act.Value.Add(generatedWorkList);
							}
							generatedWorkList.AddGeneratedWork(randomWork);
							++countAddedWorks;
							break;
						}
						randomWorkIndex -= workList.Value.Count;
					}
				}
			}

			return countAddedWorks == countWorks;
		}

		private List<int> GetRandomSums(int totalSum, int countWorks)
		{
			List<int> sums = new List<int>();

			int remainder = totalSum;
			for (int i = 0; i < countWorks; ++i)
			{
				int randomSum = random.Next(minSum + 1, maxSum);
				remainder -= randomSum;
				sums.Add(randomSum);
			}

			int minSumLimit = minSum, maxSumLimit = maxSum;
			int targetSum = totalSum / countWorks;
			if (targetSum < minSum || targetSum > maxSum)
			{
				minSumLimit = 100;
				maxSumLimit = int.MaxValue;
			}

			while (remainder != 0)
			{
				int direction = remainder > 0 ? 1 : -1;
				for (int i = 0; i < countWorks && remainder != 0; ++i)
				{
					int nextSum = sums[i] + direction;
					if (remainder > 0 ? nextSum <= maxSumLimit : nextSum >= minSumLimit)
					{
						sums[i] += direction;
						remainder -= direction;
					}
				}
			}

			return sums;
		}

		private void ReturnBackUsedWorks(KeyValuePair<HumanListItemControlModel, List<GeneratedWorkList>> act)
		{
			foreach (GeneratedWorkList actGeneratedWorkList in act.Value)
			{
				foreach (GeneratedWorkList generatedWorkList in generatedWorkLists)
				{
					if (actGeneratedWorkList.Project == generatedWorkList.Project)
					{
						foreach (List<GeneratedWork> values in actGeneratedWorkList.GeneratedWorks.Values)
						{
							values.ForEach(x => generatedWorkList.AddGeneratedWork(x));
						}
						break;
					}
				}
			}
		}

		public int GetCountEnabledWorks(List<FullDocumentTemplate> documentTemplates)
		{
			int res = 0;
			documentTemplates
				.ForEach(documentTemplate => res += generatedWorkLists.SelectMany(x => x.GeneratedWorks[documentTemplate]).Count());
			return res;
		}

		private async Task SavingActs(GenerationDialogViewModel dialogContext, double totalProgressPart)
		{
			double progressPart = totalProgressPart / acts.Count;

			generatedActNames = new List<string>();
			foreach (KeyValuePair<HumanListItemControlModel, List<GeneratedWorkList>> act in acts)
			{
				await SetProcessDescription(dialogContext, "Збереження акту \"" + act.Key.HumanData.Name + '"');

				if (IsBreakGeneration(dialogContext)) return;

				DocumentMakerModel documentMakerModel = new DocumentMakerModel();
				documentMakerModel.SelectedHuman = act.Key.HumanData.Name;
				documentMakerModel.ActSum = ((int)act.Key.Sum).ToString();
				documentMakerModel.TemplateType = act.Value.First().GeneratedWorks.Keys.First().Type;
				documentMakerModel.TechnicalTaskDateText = technicalTaskDate.ToString(dateFormat);
				documentMakerModel.ActDateText = actDate.ToString(dateFormat);

				List<FullBackDataModel> backs = new List<FullBackDataModel>();
				foreach (GeneratedWorkList generatedWorkList in act.Value)
				{
					if (IsBreakGeneration(dialogContext)) return;

					foreach (GeneratedWork generatedWork in generatedWorkList.GeneratedWorks.Values.SelectMany(x => x))
					{
						if (IsBreakGeneration(dialogContext)) return;

						FullBackDataModel m = new FullBackDataModel
						{
							EpisodeNumberText = GetEpisodeNumber(generatedWork.Back),
							BackNumberText = generatedWork.Back.Number.ToString(),
							BackName = generatedWork.Back.Name,
							GameName = generatedWorkList.Project.ToString(),
							SpentTimeText = generatedWork.Sum.ToString(),
							SumText = generatedWork.Sum.ToString(),
							WorkObjectId = generatedWork.WorkObject.Id,
							Type = GetDmlBackType(generatedWork),
							IsRework = true,
						};

						if (m.Type == Dml.Model.Back.BackType.Regions || m.Type == Dml.Model.Back.BackType.HogRegions)
						{
							m.BackCountRegionsText = string.Empty;
							foreach (int regNum in generatedWork.Regions)
							{
								m.BackCountRegionsText += regNum.ToString() + ",";
							}
						}

						if (m.Type == Dml.Model.Back.BackType.Mg
							|| m.Type == Dml.Model.Back.BackType.Dialog
							|| m.Type == Dml.Model.Back.BackType.Hog || m.Type == Dml.Model.Back.BackType.HogRegions)
						{
							m.BackName = m.BackNumberText + ". " + m.BackName;
							m.BackNumberText = GetBaseBackNumber(generatedWork.Back);
						}

						backs.Add(m);
					}
				}
				string path = Path.Combine(savingPath, documentMakerModel.SelectedHuman + DcmkFile.Extension);
				documentMakerModel.ExportDcmk(path, backs);
				generatedActNames.Add(path);

				await AddPogress(dialogContext, progressPart);
			}
		}

		private string GetEpisodeNumber(Back back)
		{
			while(back != null)
			{
				if (loadedBackTypes.First(x => x.Id == back.BackTypeId).Name == ProjectNodeType.Episode.ToString())
					return back.Number.ToString();
				else if (back.BaseBackId == null)
					break;

				back = loadedBacks.FirstOrDefault(x => x.Id == back.BaseBackId);
			}

			return null;
		}

		private string GetBaseBackNumber(Back back)
		{
			return back.BaseBackId == null ? null : loadedBacks.FirstOrDefault(x => x.Id == back.BaseBackId)?.Number.ToString();
		}

		private Dml.Model.Back.BackType GetDmlBackType(GeneratedWork generatedWork)
		{
			ProjectNodeType projectNodeType = (ProjectNodeType)Enum.Parse(typeof(ProjectNodeType), generatedWork.Back.BackType.Name);
			switch(projectNodeType)
			{
				case ProjectNodeType.Craft: return Dml.Model.Back.BackType.Craft;
				case ProjectNodeType.Dialog: return Dml.Model.Back.BackType.Dialog;
				case ProjectNodeType.Minigame: return Dml.Model.Back.BackType.Mg;
			}

			bool isRegions = generatedWork.Regions != null && generatedWork.Regions.Count > 0;
			if (projectNodeType == ProjectNodeType.Back)
			{
				return isRegions ? Dml.Model.Back.BackType.Regions : Dml.Model.Back.BackType.Back;
			}
			else
			{
				return isRegions ? Dml.Model.Back.BackType.HogRegions : Dml.Model.Back.BackType.Hog;
			}
		}

		private void InvokeInDialogDispatcher(GenerationDialogViewModel dialogContext, Action action)
		{
			dialogContext.Dispatcher.Invoke(action);
		}

		private async Task AddPogress(GenerationDialogViewModel dialogContext, double progress)
		{
			InvokeInDialogDispatcher(dialogContext, () => dialogContext.ProgressValue += progress);
			await Task.Delay(1);
		}

		private async Task SetProgress(GenerationDialogViewModel dialogContext, double progress)
		{
			InvokeInDialogDispatcher(dialogContext, () => dialogContext.ProgressValue = progress);
			await Task.Delay(1);
		}

		private async Task SetProcessDescription(GenerationDialogViewModel dialogContext, string text)
		{
			InvokeInDialogDispatcher(dialogContext, () => dialogContext.LabelText = text);
			await Task.Delay(1);
		}

		private bool IsBreakGeneration(GenerationDialogViewModel dialogContext)
		{
			return dialogContext.Dispatcher.Invoke(() => dialogContext.IsClosing);
		}

		private async Task<bool> IsSkipGenerating(GenerationDialogViewModel dialogContext, double totalProgressPart)
		{
			if (!needGenarateWorks)
			{
				await AddPogress(dialogContext, totalProgressPart);
			}
			return !needGenarateWorks;
		}

		public List<string> GetGeneratedActNames()
		{
			return generatedActNames;
		}
	}
}
