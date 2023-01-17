﻿using ActGenerator.Model.Controls;
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
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ActGenerator.Model
{
	class ActGeneratorModel : ProjectsDbConnector
	{
		delegate Task GeneratingPart(GenerationDialogViewModel dialogContext, double totalProgressPart);

		List<GeneratingPart> generatingParts = new List<GeneratingPart>();
		Random random = new Random();

		// contain Project or AlternativeProjectName with reference to Project
		List<IDbObject> projects = null;
		IEnumerable<HumanListItemControlModel> humen = null;
		List<DcmkFile> dcmkFiles = null;
		int minSum = default;
		int maxSum = default;
		string savingPath = null;

		List<FullDocumentTemplate> documentTemplates = null;

		List<GeneratedWorkList> generatedWorkLists = null;
		Dictionary<HumanListItemControlModel, List<GeneratedWorkList>> acts = null;

		public ActGeneratorModel()
		{
			generatingParts.Add(LoadingProjects);
			generatingParts.Add(GeneratingAllWorks);
			generatingParts.Add(RemovingUsedWorks);
			generatingParts.Add(GeneratingAllRegionsWork);
			generatingParts.Add(GeneratingActs);
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

		public async Task StartGeneration(GenerationDialogViewModel dialogContext)
		{
			{
				double progressPart = dialogContext.Dispatcher.Invoke(() => dialogContext.ProgressMaximum) / generatingParts.Count;

				foreach (GeneratingPart generatingPart in generatingParts)
				{
					await generatingPart(dialogContext, progressPart);

					if(dialogContext.IsClosing)
					{
						return;
					}
				}

				await SetProcessDescription(dialogContext, "Згенеровано!");
			}
		}

		public bool IsPosibleSum(int sum)
		{
			return sum >= minSum && Math.Ceiling((double)sum / maxSum) <= sum / minSum;
		}

		private async Task LoadingProjects(GenerationDialogViewModel dialogContext, double totalProgressPart)
		{
			await ConnectDbAsync();

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
			double progressPart = totalProgressPart / db.Backs.Count(x => x.ProjectId == project.Id);

			List<BackType> backTypes = db.BackTypes.ToList();

			project.Backs = new List<Back>();
			foreach (Back back in db.Backs.Where(x => x.ProjectId == project.Id).ToList())
			{
				if (IsBreakGeneration(dialogContext)) return;

				project.Backs.Add(back);
				back.BackType = backTypes.First(y => y.Id == back.BackTypeId);
				back.Regions = db.CountRegions.Where(y => y.BackId == back.Id).ToList();

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
			await SetProcessDescription(dialogContext, "Видалення використаних робіт");
			double startProgress = dialogContext.Dispatcher.Invoke(() => dialogContext.ProgressValue);
			double progressPart = totalProgressPart
				/ dcmkFiles.SelectMany(x => x.BackDataModels).Count()
				/ generatedWorkLists.Count;

			foreach(DcmkFile dcmkFile in dcmkFiles)
			{
				if (documentTemplates.FirstOrDefault(x => x.Type == dcmkFile.TemplateType) != null)
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
						GeneratedWork regionsWork = generatedWork.Clone();
						backWork.Regions.Clear();
						regionsWork.BackUsed = true;
						newGeneratedWorks.Add(backWork);
						newGeneratedWorks.Add(regionsWork);
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
				int countWorks = GetRandomCountWorks(sum);

				if (!TryGetActWithRandomWorks(dialogContext, countWorks, human.SelectedTemplates, act))
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

		private int GetRandomCountWorks(int sum)
		{
			int minCountWorks = (int)Math.Ceiling((double)sum / maxSum);
			int maxCountWorks = (int)Math.Floor((double)sum / minSum);

			int countWorks = minCountWorks;
			if (minCountWorks != maxCountWorks)
			{
				countWorks = random.Next(minCountWorks, maxCountWorks + 1);
			}
			return countWorks;
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

			while (remainder != 0)
			{
				int direction = remainder > 0 ? 1 : -1;
				for (int i = 0; i < countWorks && remainder != 0; ++i)
				{
					int nextSum = sums[i] + direction;
					if (remainder > 0 ? nextSum <= maxSum : nextSum >= minSum)
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
	}
}
