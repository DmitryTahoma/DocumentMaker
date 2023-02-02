using ActGenerator.Model.Controls;
using ActGenerator.ViewModel.Dialogs;
using DocumentMakerModelLibrary;
using DocumentMakerModelLibrary.Algorithm;
using DocumentMakerModelLibrary.Back;
using DocumentMakerModelLibrary.Controls;
using DocumentMakerModelLibrary.Files;
using DocumentMakerModelLibrary.OfficeFiles;
using DocumentMakerModelLibrary.OfficeFiles.Human;
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
		ActGeneratorCache cache = new ActGeneratorCache();
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
		List<HumanData> humanDatas = null;

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
			generatingParts.Add(GeneratingActs);
			generatingParts.Add(SavingActs);
		}

		public void SetProjects(List<IDbObject> projects)
		{
			this.projects = projects;
		}

		public void SetHumen(IEnumerable<HumanListItemControlModel> humen)
		{
			this.humen = humen;
			documentTemplates = humen.SelectMany(x => x.SelectedTemplates).Distinct().ToList();
		}

		public void SetDocumentList(List<DcmkFile> dcmkFiles)
		{
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
			this.isCollapseRegionsWorks = isCollapseRegionsWorks;
		}

		public void SetIgnoringActDate(DateTime ignoringActDate)
		{
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

		public bool ContainsHumanDatas()
		{
			return humanDatas != null;
		}

		public void SetHumanDatas(List<HumanData> humanDatas)
		{
			this.humanDatas = humanDatas;
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
			}
			catch (Exception e)
			{
				dialogContext.Dispatcher.Invoke(() => System.Windows.MessageBox.Show(e.ToString(), "Генерація | Помилка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error));
			}
		}

		private async Task LoadingProjects(GenerationDialogViewModel dialogContext, double totalProgressPart)
		{
			// list with projects loaded earlier
			List<IDbObject> loadedProjects = new List<IDbObject>();
			// list with not loaded projects
			List<IDbObject> emptyProjects = new List<IDbObject>();

			// determination of the loaded project or not
			foreach (IDbObject dbObject in projects)
			{
				if (cache.TryGetProject(dbObject, out IDbObject project) || cache.TryDuplicateProject(dbObject, out project))
				{
					loadedProjects.Add(project);
				}
				else
				{
					emptyProjects.Add(dbObject);
				}
			}

			if (IsBreakGeneration(dialogContext)) return;

			if (emptyProjects.Count > 0)
			{
				await ConnectDbAsync();

				if (cache.BackTypes == null)
					cache.BackTypes = db.BackTypes.ToList();

				double progressPart = totalProgressPart / emptyProjects.Count;
				foreach (IDbObject dbObject in emptyProjects)
				{
					await SetProcessDescription(dialogContext, "Завантаження проєкту \"" + dbObject.ToString() + '"');

					Project project = cache.UnpackProject(dbObject);

					Project prevLoadedProject = cache.GetProject(cachedProj => cachedProj.Id == project.Id);
					if (prevLoadedProject != null)
					{
						project.Backs = prevLoadedProject.Backs;
						await AddPogress(dialogContext, progressPart);
					}
					else
					{
						await LoadProject(project, dialogContext, progressPart);
					}

					if (IsBreakGeneration(dialogContext))
					{
						DisconnectDb();
						return;
					}

					cache.SetProject(dbObject);
				}
			}
			else
			{
				await AddPogress(dialogContext, totalProgressPart);
			}

			projects = loadedProjects.Union(emptyProjects).ToList();

			await DisconnectDbAsync();
		}

		private async Task LoadProject(Project project, GenerationDialogViewModel dialogContext, double totalProgressPart)
		{
			double startProgress = dialogContext.Dispatcher.Invoke(() => dialogContext.ProgressValue);
			double progressPart = totalProgressPart / db.Backs.Count(x => x.ProjectId == project.Id);

			List<int> removedBacksIndexes = new List<int>();

			project.Backs = new List<Back>();
			foreach (Back back in db.Backs.Where(x => x.ProjectId == project.Id).ToList())
			{
				if (IsBreakGeneration(dialogContext)) return;

				if (back.DeletionDate != null || (back.BaseBackId.HasValue && removedBacksIndexes.Contains(back.BaseBackId.Value)))
				{
					removedBacksIndexes.Add(back.Id);
				}
				else
				{
					project.Backs.Add(back);
					loadedBacks.Add(back);
					back.BackType = cache.BackTypes.First(y => y.Id == back.BackTypeId);
					back.Regions = db.CountRegions.Where(y => y.DeletionDate == null && y.BackId == back.Id).ToList();
				}

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
			int projectId = cache.GetProjectId(currentDbObject);

			foreach (IDbObject loadedDbObject in projects)
			{
				if (loadedDbObject == currentDbObject)
				{
					return null;
				}
				Project loadedProject = cache.UnpackProject(loadedDbObject);

				if (projectId == loadedProject.Id)
				{
					return loadedProject;
				}
			}
			return null;
		}

		private async Task GeneratingAllWorks(GenerationDialogViewModel dialogContext, double totalProgressPart)
		{
			generatedWorkLists = new List<GeneratedWorkList>();

			foreach(IDbObject dbObject in projects)
			{
				if (cache.TryGetGeneratedWorkList(dbObject, out GeneratedWorkList generatedWorkList) || cache.TryDuplicateGeneratedWorkList(dbObject, out generatedWorkList))
				{
					generatedWorkLists.Add(generatedWorkList);
				}
				else
				{
					generatedWorkList = new GeneratedWorkList { Project = dbObject };
					generatedWorkLists.Add(generatedWorkList);
					cache.AddGeneratedWorkList(generatedWorkList);
				}
			}

			double progressPart = totalProgressPart / generatedWorkLists.Count / documentTemplates.Count;

			foreach (GeneratedWorkList generatedWorkList in generatedWorkLists)
			{
				Project project = cache.UnpackProject(generatedWorkList.Project);
				await SetProcessDescription(dialogContext, "Генерація робіт проєкту \"" + generatedWorkList.Project.ToString() + '"');

				bool workListUpdated = false;
				foreach (FullDocumentTemplate documentTemplate in documentTemplates)
				{
					if (generatedWorkList.GeneratedWorks.ContainsKey(documentTemplate))
					{
						await AddPogress(dialogContext, progressPart);
						continue;
					}
					workListUpdated = true;

					double progressPart2 = progressPart / documentTemplate.ReworkWorkTypesList.Count / project.Backs.Count;

					foreach (WorkObject workObject in documentTemplate.ReworkWorkTypesList)
					{
						foreach (Back back in project.Backs)
						{
							if (IsBreakGeneration(dialogContext))
							{
								generatedWorkList.GeneratedWorks.Remove(documentTemplate);
								return;
							}

							GeneratedWork generatedWork = new GeneratedWork
							{
								WorkObject = workObject,
								Back = back,
								DocumentTemplate = documentTemplate,
							};

							CountRegions countRegions = back.Regions.FirstOrDefault();
							if (countRegions != null)
							{
								generatedWork.Regions = new List<int>();
								for (int i = 1; i <= countRegions.Count; ++i)
								{
									generatedWork.Regions.Add(i);
								}
							}

							generatedWorkList.AddGeneratedWork(generatedWork);

							await AddPogress(dialogContext, progressPart2);
						}
					}
				}

				if(workListUpdated)
				{
					cache.UpdateGeneratedWorkList(generatedWorkList);
				}
			}
		}

		private async Task RemovingUsedWorks(GenerationDialogViewModel dialogContext, double totalProgressPart)
		{
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
												|| (generatedWork.Back.BackType.Name == ProjectNodeType.Craft.ToString()
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

							if(randomWork.HaveRegions && !randomWork.BackUsed)
							{
								GeneratedWork tempWork = randomWork.Clone();
								int randNum = random.Next(0, randomWork.Regions.Count * 3);
								GeneratedWork clonedRandomWork = randomWork.Clone();

								if (randNum < randomWork.Regions.Count)
								{
									if (isCollapseRegionsWorks)
									{
										clonedRandomWork.Regions.Clear();
									}
									else
									{
										int randRegion = randomWork.Regions[randNum];
										clonedRandomWork.Regions.RemoveAt(randNum);
									}
									tempWork.BackUsed = true;
								}
								else
								{
									clonedRandomWork.BackUsed = true;
									tempWork.Regions.Clear();
								}

								workList.Value.Remove(randomWork);
								workList.Value.Add(clonedRandomWork);
								randomWork = tempWork;
							}
							else
							{
								workList.Value.Remove(randomWork);
								--countEnableWorks;
							}

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
				string path = Path.Combine(savingPath, CreateActFileName(act.Key.HumanData));
				documentMakerModel.ExportDcmk(path, backs);
				generatedActNames.Add(path);

				await AddPogress(dialogContext, progressPart);
			}
		}

		private string GetEpisodeNumber(Back back)
		{
			while(back != null)
			{
				if (cache.BackTypes.First(x => x.Id == back.BackTypeId).Name == ProjectNodeType.Episode.ToString())
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

			bool isRegions = generatedWork.HaveRegions;
			if (projectNodeType == ProjectNodeType.Back)
			{
				return isRegions ? Dml.Model.Back.BackType.Regions : Dml.Model.Back.BackType.Back;
			}
			else
			{
				return isRegions ? Dml.Model.Back.BackType.HogRegions : Dml.Model.Back.BackType.Hog;
			}
		}

		private string CreateActFileName(HumanData humanData)
		{
			string res = string.Empty;

			string[] name = humanData.Name.Split(' ');
			List<HumanData> repeatSurnameHumen = humanDatas.Where(x => x.Name != humanData.Name && x.Name.StartsWith(name[0])).ToList();

			res += name[0] + ' ';

			if(repeatSurnameHumen.Count > 0)
			{
				List<string[]> splittedNames = repeatSurnameHumen.Select(x => x.Name.Split(' ')).ToList();

				for (int i = 1; i <= 2; ++i)
				{
					bool addedName = false;
					foreach (string[] splittedName in splittedNames)
					{
						if (splittedName[i][0] == name[i][0])
						{
							res += name[i] + ' ';
							addedName = true;
							break;
						}
					}
					if (!addedName)
					{
						res += name[i][0] + ". ";
						break;
					}
				}
			}

			res += actDate.ToString(dateFormat) + ' ' + DateTime.Now.ToString("dd.MM.yy_HH-mm-ss.ffffff") + DcmkFile.Extension;
			return res;
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

		public List<string> GetGeneratedActNames()
		{
			return generatedActNames;
		}

		public bool IsReadyToGeneration(HumanListItemControlModel human)
		{
			foreach(IDbObject project in projects)
			{
				foreach(FullDocumentTemplate documentTemplate in human.SelectedTemplates)
				{
					if (!cache.ContainsGeneratedWorkList(project, documentTemplate))
					{
						return false;
					}
				}
			}
			return true;
		}
	}
}
