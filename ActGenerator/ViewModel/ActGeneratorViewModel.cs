﻿using ActGenerator.Model;
using ActGenerator.View.Controls;
using ActGenerator.View.Dialogs;
using ActGenerator.ViewModel.Controls;
using ActGenerator.ViewModel.Dialogs;
using Dml;
using Dml.Model.Template;
using DocumentMaker.Security;
using MaterialDesignThemes.Wpf;
using Mvvm;
using Mvvm.Commands;
using ProjectEditorLib.ViewModel;
using ProjectsDb.Context;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ActGenerator.ViewModel
{
	class ActGeneratorViewModel : DependencyObject, ICryptedConnectionStringRequired
	{
		ActGeneratorModel model = new ActGeneratorModel();
		List<Project> dbProjects = null;

		UIElementCollection projectsStackWithNames = null;
		List<Project> projectsList = new List<Project>();

		List<int> savedProjectList = null;
		List<ActGeneratorSession.HumanDataContextSave> savedHumanList = null;

		public ActGeneratorViewModel()
		{
			InitCommands();

			State = ViewModelState.Initialized;
		}

		#region Properties

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
		public static readonly DependencyProperty StateProperty = DependencyProperty.Register(nameof(State), typeof(ViewModelState), typeof(ActGeneratorViewModel), new PropertyMetadata(), CommandHelper.UpdateAllCanExecute);

		#endregion

		#region Commands

		private void InitCommands()
		{
			CloseOpenedDialog = new Command(OnCloseOpenedDialogExecute);
			LoadFromDatabase = new Command(OnLoadFromDatabaseExecute);
			RemoveProjectCommand = new Command(OnRemoveProjectCommandExecute);
			AddHumanCommand = new Command(OnAddHumanCommandExecute);
			RemoveHumanCommand = new Command<IList>(OnRemoveHumanCommandExecute);
			BindProjectsStackWithNames = new Command<UIElementCollection>(OnBindProjectsStackWithNamesExecute);
			GenerateActs = new Command<DependencyObject>(OnGenerateActsExecute);
			BindDialogHostName = new Command<ProjectNamesListControlViewModel>(OnBindDialogHostNameExecute);
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
			if(DialogHost.IsDialogOpen(DialogHostId))
			{
				DialogHost.Close(DialogHostId);
			}
		}

		public Command LoadFromDatabase { get; private set; }
		private async void OnLoadFromDatabaseExecute()
		{
			if (!model.ConnectionStringSetted) return;

			if (State == ViewModelState.Initialized)
			{
				State = ViewModelState.Loading;
				await model.ConnectDbAsync();
				dbProjects = await model.LoadProjectsAsync();
				await model.DisconnectDbAsync();

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
					
				}

				State = ViewModelState.Loaded;
			}
			else if(State == ViewModelState.Loaded)
			{
				State = ViewModelState.Loading;
				await model.ConnectDbAsync();
				dbProjects = await model.LoadProjectsAsync();
				await model.DisconnectDbAsync();

				IEnumerator projectsStackWithNamesEnum = projectsStackWithNames.GetEnumerator();
				List<Project>.Enumerator dbProjectsEnum = dbProjects.GetEnumerator();
				while(projectsStackWithNamesEnum.MoveNext() && dbProjectsEnum.MoveNext())
				{
					ListView listView = projectsStackWithNamesEnum.Current as ListView;
					listView.DataContext = dbProjectsEnum.Current;
					listView.ItemsSource = new List<string> { dbProjectsEnum.Current.Name }.Concat(dbProjectsEnum.Current.AlternativeNames.Select(x => x.Name));
				}
				HumanList.UpdateCollection();
				State = ViewModelState.Loaded;
			}
		}

		public Command AddHumanCommand { get; private set; }
		private void OnAddHumanCommandExecute()
		{
			
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
		private void OnGenerateActsExecute(DependencyObject validateObj)
		{
			if (ValidationHelper.GetFirstInvalid(validateObj, true) is UIElement invalid)
			{
				invalid.Focus();
				return;
			}
		}

		public Command<ProjectNamesListControlViewModel> BindDialogHostName { get; private set; }
		private void OnBindDialogHostNameExecute(ProjectNamesListControlViewModel projectNamesListControlViewModel)
		{
			projectNamesListControlViewModel.DialogHostId = DialogHostId;
		}

		#endregion

		#region Methods

		public void LoadFromSession(ActGeneratorSession actGeneratorSession)
		{
			savedProjectList = actGeneratorSession.ProjectsList;
			savedHumanList = actGeneratorSession.HumanList;
			MinSumText = actGeneratorSession.MinSumText;
			MaxSumText = actGeneratorSession.MaxSumText;
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

		public void SetCryptedConnectionString(CryptedConnectionString cryptedConnectionString)
		{
			model.ConnectionString = cryptedConnectionString;
		}

		#endregion
	}
}
