using ActGenerator.Model;
using ActGenerator.View.Dialogs;
using ActGenerator.ViewModel.Dialogs;
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
using System.Windows;

namespace ActGenerator.ViewModel
{
	class ActGeneratorViewModel : DependencyObject
	{
		ActGeneratorModel model = new ActGeneratorModel();
		List<Project> dbProjects = null;
		List<Human> dbHumans = null;

		ListSelector listSelector;
		ListSelectorViewModel listSelectorViewModel;

		List<int> savedProjectList = null;
		List<ActGeneratorSession.HumanDataContextSave> savedHumanList = null;

		public ActGeneratorViewModel()
		{
			InitCommands();

			listSelector = new ListSelector();
			listSelectorViewModel = (ListSelectorViewModel)listSelector.DataContext;
		}

		#region Properties

		public string DialogHostId { get; } = "ActGeneratorDialogHost";

		public ObservableRangeCollection<Project> ProjectsList { get; private set; } = new ObservableRangeCollection<Project>();

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

		#endregion

		#region Commands

		private void InitCommands()
		{
			AddProjectCommand = new Command(OnAddProjectCommandExecute);
			CloseOpenedDialog = new Command(OnCloseOpenedDialogExecute);
			LoadFromDatabase = new Command(OnLoadFromDatabaseExecute);
			RemoveProjectCommand = new Command<IList>(OnRemoveProjectCommandExecute);
			AddHumanCommand = new Command(OnAddHumanCommandExecute);
			RemoveHumanCommand = new Command<IList>(OnRemoveHumanCommandExecute);
		}

		public Command AddProjectCommand { get; private set; }
		private async void OnAddProjectCommandExecute()
		{
			listSelectorViewModel.ItemsDisplayMemberPath = nameof(Project.Name);
			List<Project> projects = new List<Project>(dbProjects.Where(x => !ProjectsList.Contains(x)));
			projects.RemoveAll(x => ProjectsList.Contains(x));
			listSelectorViewModel.SetItems(projects);
			await DialogHost.Show(listSelector, DialogHostId);
			if(listSelectorViewModel.IsAddingPressed && listSelectorViewModel.SelectedItems != null)
			{
				ProjectsList.AddRange(listSelectorViewModel.SelectedItems.Cast<Project>());
			}
		}

		public Command<IList> RemoveProjectCommand { get; private set; }
		private void OnRemoveProjectCommandExecute(IList selectedItems)
		{
			ProjectsList.RemoveRange(selectedItems.Cast<Project>());
		}

		public Command CloseOpenedDialog { get; private set; }
		private void OnCloseOpenedDialogExecute()
		{
			if(DialogHost.IsDialogOpen(DialogHostId))
			{
				DialogHost.Close(DialogHostId);
			}
		}

		public Command LoadFromDatabase { get; private set; }
		private async void OnLoadFromDatabaseExecute()
		{
			if (dbProjects == null)
			{
				await model.ConnectDB();
				dbProjects = await model.LoadProjects();
				dbHumans = await model.LoadHumen();
				await model.DisconnectDB();

				if(savedProjectList != null)
				{
					ProjectsList.AddRange(dbProjects.Where(x => savedProjectList.Contains(x.Id)));
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
			}
		}

		public Command AddHumanCommand { get; private set; }
		private async void OnAddHumanCommandExecute()
		{
			listSelectorViewModel.ItemsDisplayMemberPath = nameof(Human.FullName);
			List<Human> humen = new List<Human>(dbHumans.Where(x => !HumanList.Select(y => y.Context).Contains(x)));
			humen.RemoveAll(x => HumanList.Select(y => y.Context).Contains(x));
			listSelectorViewModel.SetItems(humen);
			await DialogHost.Show(listSelector, DialogHostId);
			if(listSelectorViewModel.IsAddingPressed && listSelectorViewModel.SelectedItems != null)
			{
				HumanList.AddRange(listSelectorViewModel.SelectedItems.Cast<Human>().Select(x => new HumanDataContext(x)));
			}
		}

		public Command<IList> RemoveHumanCommand { get; private set; }
		private void OnRemoveHumanCommandExecute(IList selectedItems)
		{
			HumanList.RemoveRange(selectedItems.Cast<HumanDataContext>());
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

		#endregion
	}
}
