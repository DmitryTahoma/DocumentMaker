﻿using ActGenerator.Model.Controls;
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
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ActGenerator.Model
{
	class ActGeneratorModel : ProjectsDbConnector
	{
		delegate Task GeneratingPart(GenerationDialogViewModel dialogContext, double totalProgressPart);

		List<GeneratingPart> generatingParts = new List<GeneratingPart>();

		// contain Project or AlternativeProjectName with reference to Project
		List<IDbObject> projects = null;
		IEnumerable<HumanListItemControlModel> humen = null;
		List<DcmkFile> dcmkFiles = null;
		int minSum = default;
		int maxSum = default;

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

		public async Task StartGeneration(GenerationDialogViewModel dialogContext)
		{
			while(!dialogContext.Dispatcher.Invoke(() => dialogContext.IsClosing) && !dialogContext.Dispatcher.Invoke(() => dialogContext.GenerationStarted))
			{
				await Task.Delay(1);
			}

			if(dialogContext.Dispatcher.Invoke(() => dialogContext.GenerationStarted))
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

				dialogContext.Dispatcher.Invoke(() => dialogContext.LabelText = "Згенеровано!");
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
				if (dialogContext.Dispatcher.Invoke(() => dialogContext.IsClosing)) break;
				dialogContext.Dispatcher.Invoke(() => dialogContext.LabelText = "Завантаження проєкту \"" + dbObject.ToString() + '"');

				Project project = dbObject as Project ?? ((AlternativeProjectName)dbObject).Project;

				Project prevLoadedProject = GetPrevProject(dbObject);
				if (prevLoadedProject != null)
				{
					project.Backs = prevLoadedProject.Backs;
					dialogContext.Dispatcher.Invoke(() => dialogContext.ProgressValue += progressPart);
					await Task.Delay(1);
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
				if (dialogContext.Dispatcher.Invoke(() => dialogContext.IsClosing)) return;

				project.Backs.Add(back);
				back.BackType = backTypes.First(y => y.Id == back.BackTypeId);
				back.Regions = db.CountRegions.Where(y => y.BackId == back.Id).ToList();

				dialogContext.Dispatcher.Invoke(() => dialogContext.ProgressValue += progressPart);
				await Task.Delay(1);
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

			dialogContext.Dispatcher.Invoke(() => dialogContext.ProgressValue = startProgress + totalProgressPart);
			await Task.Delay(1);
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

				dialogContext.Dispatcher.Invoke(() => dialogContext.LabelText = "Генерація робіт проєкту \"" + dbObject.ToString() + '"');
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
							dialogContext.Dispatcher.Invoke(() => dialogContext.ProgressValue += skippedProgress);
							await Task.Delay(1);
							break;
						}
					}
				}
				else
				{
					foreach (Back back in project.Backs)
					{
						if (dialogContext.Dispatcher.Invoke(() => dialogContext.IsClosing)) break;

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

								dialogContext.Dispatcher.Invoke(() => dialogContext.ProgressValue += progressPart);
								await Task.Delay(1);
							}
						}
					}
				}
				generatedWorkLists.Add(generatedWorkList);
			}
		}

		private async Task RemovingUsedWorks(GenerationDialogViewModel dialogContext, double totalProgressPart)
		{
			dialogContext.Dispatcher.Invoke(() => dialogContext.LabelText = "Видалення використаних робіт");
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
						if (dialogContext.Dispatcher.Invoke(() => dialogContext.IsClosing)) return;

						if (back.IsRework)
						{
							foreach (GeneratedWorkList workList in generatedWorkLists)
							{
								if (dialogContext.Dispatcher.Invoke(() => dialogContext.IsClosing)) return;

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

								dialogContext.Dispatcher.Invoke(() => dialogContext.ProgressValue += progressPart);
								await Task.Delay(1);
							}
						}
					}
				}
			}

			dialogContext.Dispatcher.Invoke(() => dialogContext.ProgressValue = startProgress + totalProgressPart);
			await Task.Delay(1);
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
			dialogContext.Dispatcher.Invoke(() => dialogContext.LabelText = "Розгортання регіонів");
			double progressPart = totalProgressPart / 2 / generatedWorkLists.SelectMany(x => x.GeneratedWorks.SelectMany(y => y.Value)).Count();

			foreach (GeneratedWorkList generatedWorkList in generatedWorkLists)
			{
				List<GeneratedWork> newGeneratedWorks = new List<GeneratedWork>();

				foreach (GeneratedWork generatedWork in generatedWorkList.GeneratedWorks.SelectMany(x => x.Value))
				{
					if (dialogContext.Dispatcher.Invoke(() => dialogContext.IsClosing)) return;

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

					dialogContext.Dispatcher.Invoke(() => dialogContext.ProgressValue += progressPart);
					await Task.Delay(1);
				}

				double localProgressPart = totalProgressPart / 2 / generatedWorkLists.Count / newGeneratedWorks.Count;

				generatedWorkList.ClearWorks();
				foreach(GeneratedWork work in newGeneratedWorks)
				{
					if (dialogContext.Dispatcher.Invoke(() => dialogContext.IsClosing)) return;

					generatedWorkList.AddGeneratedWork(work);

					dialogContext.Dispatcher.Invoke(() => dialogContext.ProgressValue += localProgressPart);
					await Task.Delay(1);
				}
			}
		}

		private async Task GeneratingActs(GenerationDialogViewModel dialogContext, double totalProgressPart)
		{
			dialogContext.Dispatcher.Invoke(() => dialogContext.LabelText = "Генерація актів");
			await Task.Delay(1);
			Random random = new Random();

			acts = new Dictionary<HumanListItemControlModel, List<GeneratedWorkList>>();
			foreach (HumanListItemControlModel human in humen)
			{
				KeyValuePair<HumanListItemControlModel, List<GeneratedWorkList>> act =
					new KeyValuePair<HumanListItemControlModel, List<GeneratedWorkList>>(human, new List<GeneratedWorkList>());

				int sum = (int)human.Sum;
				int minCountWorks = (int)Math.Ceiling((double)sum / maxSum);
				int maxCountWorks = (int)Math.Floor((double)sum / minSum);

				int countWorks = minCountWorks;
				if (minCountWorks != maxCountWorks)
				{
					countWorks = random.Next(minCountWorks, maxCountWorks + 1);
				}

				foreach(FullDocumentTemplate documentTemplate in human.SelectedTemplates)
				{
					Dictionary<GeneratedWorkList, List<GeneratedWork>> enableWorks = generatedWorkLists.ToDictionary(key => key, value => value.GeneratedWorks[documentTemplate]);
					int countEnableWorks = enableWorks.SelectMany(x => x.Value).Count();

					while (countEnableWorks > 0 && act.Value.SelectMany(x => x.GeneratedWorks.SelectMany(y => y.Value)).Count() < countWorks)
					{
						int randomWorkIndex = random.Next(0, countEnableWorks);

						foreach (KeyValuePair<GeneratedWorkList, List<GeneratedWork>> workList in enableWorks)
						{
							if(randomWorkIndex < workList.Value.Count)
							{
								GeneratedWork randomWork = workList.Value[randomWorkIndex];
								workList.Value.Remove(randomWork);
								--countEnableWorks;

								GeneratedWorkList generatedWorkList = act.Value.FirstOrDefault(x => x.Project == workList.Key.Project);
								if(generatedWorkList == null)
								{
									generatedWorkList = new GeneratedWorkList { Project = workList.Key.Project };
									act.Value.Add(generatedWorkList);
								}
								generatedWorkList.AddGeneratedWork(randomWork);
								break;
							}
							randomWorkIndex -= workList.Value.Count;
						}
					}
				}

				List<int> sums = new List<int>();

				int remainder = sum;
				for (int i = 0; i < countWorks; ++i)
				{
					int randomSum = random.Next(minSum + 1, maxSum);
					remainder -= randomSum;
					sums.Add(randomSum);
				}

				while(remainder != 0)
				{
					int direction = remainder > 0 ? 1 : -1;
					for (int i = 0; i < countWorks && remainder != 0; ++i)
					{
						int nextSum = sums[i] + direction;
						if (remainder > 0 ? nextSum < maxSum : nextSum > minSum)
						{
							sums[i] += direction;
							remainder -= direction;
						}
					}
				}

				MixingSumAlgorithm.RemoveIdenticalNumbers(ref sums, minSum, maxSum);
			}
		}
	}
}
