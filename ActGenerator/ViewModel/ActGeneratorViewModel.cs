using ActGenerator.Model;
using ActGenerator.View.Dialogs;
using ActGenerator.ViewModel.Dialogs;
using Db.Context.ActPart;
using Db.Context.BackPart;
using Db.Context.HumanPart;
using Dml.Model.Template;
using MaterialDesignThemes.Wpf;
using Mvvm;
using Mvvm.Commands;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ActGenerator.ViewModel
{
	class ActGeneratorViewModel : DependencyObject
	{
		ActGeneratorModel model = new ActGeneratorModel();
		List<Project> dbProjects = null;
		List<Human> dbHumans = null;

		ListSelector listSelector;
		ListSelectorViewModel listSelectorViewModel;

		UIElementCollection projectsStackWithNames = null;
		List<Project> projectsList = new List<Project>();

		List<int> savedProjectList = null;
		List<ActGeneratorSession.HumanDataContextSave> savedHumanList = null;

		public ActGeneratorViewModel()
		{
			InitCommands();

			listSelector = new ListSelector();
			listSelectorViewModel = (ListSelectorViewModel)listSelector.DataContext;

			State = ViewModelState.Initialized;
		}

		#region Properties

		public bool IsOpenActGeneratorDialogHost { get; private set; } = false;

		public string DialogHostId { get; } = "ActGeneratorDialogHost";

		public IEnumerable<Project> ProjectsList => projectsList;

		public ObservableRangeCollection<HumanDataContext> HumanList { get; private set; } = new ObservableRangeCollection<HumanDataContext>();

		public ObservableRangeCollection<DocumentTemplate> DocumentTemplates { get; private set; } = new ObservableRangeCollection<DocumentTemplate> 
		{
			new DocumentTemplate("Скриптувальник", DocumentTemplateType.Scripter),
			new DocumentTemplate("Технічний дизайнер", DocumentTemplateType.Cutter),
			new DocumentTemplate("Художник", DocumentTemplateType.Painter),
			new DocumentTemplate("Моделлер", DocumentTemplateType.Modeller),
		};

		public ObservableRangeCollection<DateTimeItem> DateTimeItems { get; private set; } = new ObservableRangeCollection<DateTimeItem>()
		{
			new DateTimeItem{ Text = "тиждень", DateTime = new DateTime(1, 1, 8) },
			new DateTimeItem{ Text = "місяць", DateTime = new DateTime(1, 2, 1) },
			new DateTimeItem{ Text = "3 місяці", DateTime = new DateTime(1, 4, 1) },
			new DateTimeItem{ Text = "пів року", DateTime = new DateTime(1, 7, 1) },
			new DateTimeItem{ Text = "рік", DateTime = new DateTime(2, 1, 1) },
		};

		public string MinSumText
		{
			get { return (string)GetValue(MinSumTextProperty); }
			set { SetValue(MinSumTextProperty, value); }
		}
		public static readonly DependencyProperty MinSumTextProperty = DependencyProperty.Register(nameof(MinSumText), typeof(string), typeof(ActGeneratorViewModel));

		public string MaxSumText
		{
			get { return (string)GetValue(MaxSumTextProperty); }
			set { SetValue(MaxSumTextProperty, value); }
		}
		public static readonly DependencyProperty MaxSumTextProperty = DependencyProperty.Register(nameof(MaxSumText), typeof(string), typeof(ActGeneratorViewModel));

		public bool IsUniqueNumbers
		{
			get { return (bool)GetValue(IsUniqueNumbersProperty); }
			set { SetValue(IsUniqueNumbersProperty, value); }
		}
		public static readonly DependencyProperty IsUniqueNumbersProperty = DependencyProperty.Register(nameof(IsUniqueNumbers), typeof(bool), typeof(ActGeneratorViewModel));

		public bool CanUseOldWorks
		{
			get { return (bool)GetValue(CanUseOldWorksProperty); }
			set { SetValue(CanUseOldWorksProperty, value); }
		}
		public static readonly DependencyProperty CanUseOldWorksProperty = DependencyProperty.Register(nameof(CanUseOldWorks), typeof(bool), typeof(ActGeneratorViewModel));

		public DateTimeItem SelectedDateTimeItem
		{
			get { return (DateTimeItem)GetValue(SelectedDateTimeItemProperty); }
			set { SetValue(SelectedDateTimeItemProperty, value); }
		}
		public static readonly DependencyProperty SelectedDateTimeItemProperty = DependencyProperty.Register(nameof(SelectedDateTimeItem), typeof(DateTimeItem), typeof(ActGeneratorViewModel));

		public ViewModelState State
		{
			get { return (ViewModelState)GetValue(StateProperty); }
			set { SetValue(StateProperty, value); }
		}
		public static readonly DependencyProperty StateProperty = DependencyProperty.Register(nameof(State), typeof(ViewModelState), typeof(ActGeneratorViewModel));

		#endregion

		#region Commands

		private void InitCommands()
		{
			AddProjectCommand = new Command(OnAddProjectCommandExecute);
			CloseOpenedDialog = new Command(OnCloseOpenedDialogExecute);
			LoadFromDatabase = new Command(OnLoadFromDatabaseExecute);
			RemoveProjectCommand = new Command(OnRemoveProjectCommandExecute);
			AddHumanCommand = new Command(OnAddHumanCommandExecute);
			RemoveHumanCommand = new Command<IList>(OnRemoveHumanCommandExecute);
			BindProjectsStackWithNames = new Command<UIElementCollection>(OnBindProjectsStackWithNamesExecute);
			GenerateActs = new Command<DependencyObject>(OnGenerateActsExecute);
		}

		public Command AddProjectCommand { get; private set; }
		private async void OnAddProjectCommandExecute()
		{
			listSelectorViewModel.ItemsDisplayMemberPath = nameof(Project.Name);
			List<Project> projects = new List<Project>(dbProjects.Where(x => !ProjectsList.Contains(x)));
			projects.RemoveAll(x => ProjectsList.Contains(x));
			listSelectorViewModel.SetItems(projects);
			IsOpenActGeneratorDialogHost = true;
			await DialogHost.Show(listSelector, DialogHostId);
			if (IsOpenActGeneratorDialogHost)
			{
				IsOpenActGeneratorDialogHost = false;
				if (listSelectorViewModel.IsAddingPressed && listSelectorViewModel.SelectedItems != null)
				{
					listSelectorViewModel.SelectedItems
						.Cast<Project>()
						.ToList()
						.ForEach(AddProjectToStack);
				}
			}
		}

		public Command RemoveProjectCommand { get; private set; }
		private void OnRemoveProjectCommandExecute()
		{
			projectsStackWithNames
				.Cast<ListView>()
				.Where(x => x.SelectedIndex != -1)
				.ToList()
				.ForEach((x) => 
				{
					projectsStackWithNames.Remove(x);
					projectsList.Remove((Project)x.DataContext);
				});
		}

		public Command CloseOpenedDialog { get; private set; }
		private void OnCloseOpenedDialogExecute()
		{
			if(IsOpenActGeneratorDialogHost)
			{
				IsOpenActGeneratorDialogHost = false;
				DialogHost.Close(DialogHostId);
			}
		}

		public Command LoadFromDatabase { get; private set; }
		private async void OnLoadFromDatabaseExecute()
		{
			if (State == ViewModelState.Initialized)
			{
				State = ViewModelState.Loading;
				await model.ConnectDB();
				dbProjects = await model.LoadProjects();
				dbHumans = await model.LoadHumen();
				await model.DisconnectDB();

				if (savedProjectList != null)
				{
					dbProjects
						.Where(x => savedProjectList
							.Contains(x.Id))
						.ToList()
						.ForEach(AddProjectToStack);
				}
				if (savedHumanList != null)
				{
					HumanList.AddRange(savedHumanList
						.Select(x =>
							new HumanDataContext(dbHumans
								.FirstOrDefault(y => y.Id == x.ContextId))
							{
								SumText = x.SumText,
								Template = DocumentTemplates
									.FirstOrDefault(y => y.Type == x.TemplateType)
							}));
				}

				State = ViewModelState.Loaded;
			}
			else if(State == ViewModelState.Loaded)
			{
				State = ViewModelState.Loading;
				await model.ConnectDB();
				dbProjects = await model.LoadProjects();
				await model.SyncCollection(dbHumans);
				await model.DisconnectDB();

				IEnumerator projectsStackWithNamesEnum = projectsStackWithNames.GetEnumerator();
				List<Project>.Enumerator dbProjectsEnum = dbProjects.GetEnumerator();
				while(projectsStackWithNamesEnum.MoveNext() && dbProjectsEnum.MoveNext())
				{
					ListView listView = projectsStackWithNamesEnum.Current as ListView;
					listView.DataContext = dbProjectsEnum.Current;
					listView.ItemsSource = new List<string> { dbProjectsEnum.Current.Name }.Concat(dbProjectsEnum.Current.AlternativeNames.Select(x => x.Name));
				}
				HumanList.ToList().ForEach(x => x.Context.Set(dbHumans.FirstOrDefault(y => y.Id == x.Context.Id)));
				HumanList.UpdateCollection();
				State = ViewModelState.Loaded;
			}
		}

		public Command AddHumanCommand { get; private set; }
		private async void OnAddHumanCommandExecute()
		{
			listSelectorViewModel.ItemsDisplayMemberPath = nameof(Human.FullName);
			List<Human> humen = new List<Human>(dbHumans.Where(x => !HumanList.Select(y => y.Context).Contains(x)));
			humen.RemoveAll(x => HumanList.Select(y => y.Context).Contains(x));
			listSelectorViewModel.SetItems(humen);
			IsOpenActGeneratorDialogHost = true;
			await DialogHost.Show(listSelector, DialogHostId);
			if (IsOpenActGeneratorDialogHost)
			{
				IsOpenActGeneratorDialogHost = false;
				if (listSelectorViewModel.IsAddingPressed && listSelectorViewModel.SelectedItems != null)
				{
					HumanList.AddRange(listSelectorViewModel.SelectedItems.Cast<Human>().Select(x => new HumanDataContext(x)));
				}
			}
		}

		public Command<IList> RemoveHumanCommand { get; private set; }
		private void OnRemoveHumanCommandExecute(IList selectedItems)
		{
			HumanList.RemoveRange(selectedItems.Cast<HumanDataContext>());
		}

		public Command<UIElementCollection> BindProjectsStackWithNames { get; private set; }
		private void OnBindProjectsStackWithNamesExecute(UIElementCollection collection)
		{
			if(projectsStackWithNames == null)
			{
				projectsStackWithNames = collection;
			}
		}

		public Command<DependencyObject> GenerateActs { get; private set; }
		private async void OnGenerateActsExecute(DependencyObject validateObj)
		{
			if (ValidationHelper.GetFirstInvalid(validateObj, true) is UIElement invalid)
			{
				invalid.Focus();
				return;
			}

			List <KeyValuePair<Project, bool[]>> selectedProjectNames = GetSelectedProjectNames();

			await model.ConnectDB();
			List<FullWork> works = await model.GenerateEnableWorks(selectedProjectNames);
			await model.RemoveUsedWorks(works, CanUseOldWorks, SelectedDateTimeItem.DateTime);
			await model.DisconnectDB();

			int minSum = int.Parse(MinSumText);
			int maxSum = int.Parse(MaxSumText);
			var acts = model.GenerateActs(works, HumanList, minSum, maxSum,
				lessWorksCallback: (curHuman) => 
				{
					return MessageBox.Show("Недостатньо робіт для генерації акту [" + curHuman.FullName + "]. Продовжити? (Поточний акт буде пропущений)", "", MessageBoxButton.YesNo)
							== MessageBoxResult.Yes; 
				},
				unrealSumCallback: (curHuman) => 
				{
					return MessageBox.Show("Неможливо розкидати роботи для акту [" + curHuman.FullName + "]. Продовжити? (Поточний акт буде пропущений)", "", MessageBoxButton.YesNo)
									== MessageBoxResult.Yes;
				}
			);

			if(acts != null)
			{
				model.ExportActs(@"D:\", acts, projectsList);
				MessageBox.Show("Сгенеровано актів: " + acts.Count.ToString());
			}
		}

		#endregion

		#region Methods

		public void LoadFromSession(ActGeneratorSession actGeneratorSession)
		{
			savedProjectList = actGeneratorSession.ProjectsList;
			savedHumanList = actGeneratorSession.HumanList;
			MinSumText = actGeneratorSession.MinSumText;
			MaxSumText = actGeneratorSession.MaxSumText;
			IsUniqueNumbers = actGeneratorSession.IsUniqueNumbers;
			CanUseOldWorks = actGeneratorSession.CanUseOldWorks;
			SelectedDateTimeItem = DateTimeItems?.FirstOrDefault(x => x.DateTime == actGeneratorSession.SelectedDateTimeItem?.DateTime)
				?? DateTimeItems?.FirstOrDefault();
		}

		private void AddProjectToStack(Project project)
		{
			ListView listView = new ListView { Style = Application.Current.FindResource("MaterialDesignFilterChipPrimaryOutlineListBox") as Style };
			listView.DataContext = project;
			listView.ItemsSource = new List<string> { project.Name }.Concat(project.AlternativeNames.Select(x => x.Name));
			listView.SelectedIndex = 0;
			projectsStackWithNames.Add(listView);
			projectsList.Add(project);
		}

		private List<KeyValuePair<Project, bool[]>> GetSelectedProjectNames()
		{
			List<KeyValuePair<Project, bool[]>> selectedProjectNames = new List<KeyValuePair<Project, bool[]>>();
			foreach (ListView listView in projectsStackWithNames)
			{
				if (listView.SelectedIndex >= 0)
				{
					Project proj = (Project)listView.DataContext;
					KeyValuePair<Project, bool[]> keyProj = new KeyValuePair<Project, bool[]>(proj, new bool[proj.AlternativeNames.Count + 1]);
					foreach (object selectedItem in listView.SelectedItems)
					{
						int index = -1;
						int i = 0;
						foreach (object item in listView.ItemsSource)
						{
							if (item == selectedItem)
							{
								index = i;
								break;
							}
							++i;
						}
						keyProj.Value[index] = true;
					}
					selectedProjectNames.Add(keyProj);
				}
			}
			return selectedProjectNames;
		}

		#endregion
	}
}
