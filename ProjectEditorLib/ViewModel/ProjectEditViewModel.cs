using Db.Context.BackPart;
using Mvvm;
using Mvvm.Commands;
using ProjectEditorLib.Model;
using ProjectEditorLib.View;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ProjectEditorLib.ViewModel
{
	public class ProjectEditViewModel : DependencyObject
	{
		ProjectEditModel model = new ProjectEditModel();
		ViewModelState state = ViewModelState.Initialized;

		UIElementCollection optionsView = null;
		int selectedViewTabIndex = -1;

		DependencyObject dependedObjCreateProject = null;
		DependencyObject dependedObjEditProject = null;

		public ProjectEditViewModel()
		{
			InitCommands();

			TreeItemHeader project = new TreeItemHeader();
			TreeItemHeaderViewModel projectViewModel = (TreeItemHeaderViewModel)project.DataContext;
			projectViewModel.SetModel(new ProjectNode(ProjectNodeType.Project, "Escape"));
			TreeViewItem treeViewItem = new TreeViewItem { Header = project };
			projectViewModel.AddCommand = ConvertAddingCommand(treeViewItem, AddTreeViewItemCommand);
			projectViewModel.RemoveCommand = ConvertRemovingCommand(treeViewItem, RemoveTreeViewItemCommand);
			treeViewItem.Selected += (s, e) => { ChangeNodeOptionsView.Execute((TreeViewItem)s); };

			Binding contextMenuBinding = new Binding(nameof(TreeItemHeaderViewModel.ContextMenuProperty))
			{
				Source = projectViewModel,
				Path = new PropertyPath(nameof(projectViewModel.ContextMenu)),
				Mode = BindingMode.OneWay,
				UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
			};
			treeViewItem.SetBinding(FrameworkElement.ContextMenuProperty, contextMenuBinding);

			TreeItems.Add(treeViewItem);
		}

		#region Properties

		public ObservableRangeCollection<TreeViewItem> TreeItems { get; private set; } = new ObservableRangeCollection<TreeViewItem>();

		public int SelectedViewTabIndex
		{
			get => selectedViewTabIndex;
			set
			{
				selectedViewTabIndex = value < 0 ? -1 : value;
				int i = 0;
				foreach(UIElement elem in optionsView)
				{
					elem.Visibility = i == selectedViewTabIndex ? Visibility.Visible : Visibility.Collapsed;
					++i;
				}
			}
		}

		public bool ProjectSelected
		{
			get { return (bool)GetValue(ProjectSelectedProperty); }
			set { SetValue(ProjectSelectedProperty, value); }
		}
		public static readonly DependencyProperty ProjectSelectedProperty = DependencyProperty.Register(nameof(ProjectSelected), typeof(bool), typeof(ProjectEditViewModel));

		public bool NeedCreateProject
		{
			get { return (bool)GetValue(NeedCreateProjectProperty); }
			set { SetValue(NeedCreateProjectProperty, value); }
		}
		public static readonly DependencyProperty NeedCreateProjectProperty = DependencyProperty.Register(nameof(NeedCreateProject), typeof(bool), typeof(ProjectEditViewModel));

		public ObservableRangeCollection<Project> ProjectList { get; private set; } = new ObservableRangeCollection<Project>();

		public Project SelectedEditProject
		{
			get { return (Project)GetValue(SelectedEditProjectProperty); }
			set { SetValue(SelectedEditProjectProperty, value); }
		}
		public static readonly DependencyProperty SelectedEditProjectProperty = DependencyProperty.Register(nameof(SelectedEditProject), typeof(Project), typeof(ProjectEditViewModel));

		public string CreationProjectName
		{
			get { return (string)GetValue(CreationProjectNameProperty); }
			set { SetValue(CreationProjectNameProperty, value); }
		}
		public static readonly DependencyProperty CreationProjectNameProperty = DependencyProperty.Register(nameof(CreationProjectName), typeof(string), typeof(ProjectEditViewModel));

		#endregion

		#region Commands

		private void InitCommands()
		{
			AddTreeViewItemCommand = new Command<KeyValuePair<TreeViewItem, ProjectNodeType>>(OnAddTreeViewItemCommandExecute);
			RemoveTreeViewItemCommand = new Command<TreeViewItem>(OnRemoveTreeViewItemCommandExecute);
			ChangeNodeOptionsView = new Command<TreeViewItem>(OnChangeNodeOptionsViewExecute);
			BindOptionsView = new Command<UIElementCollection>(OnBindOptionsViewExecute);
			EditProject = new Command(OnEditProjectExecute, CanExecuteEditProject);
			BindDependedObjCreateProject = new Command<DependencyObject>(OnBindDependedObjCreateProjectExecute);
			BindDependedObjEditProject = new Command<DependencyObject>(OnBindDependedObjEditProjectExecute);
			LoadFromDatabase = new Command(OnLoadFromDatabaseExecute);
		}

		public Command<KeyValuePair<TreeViewItem, ProjectNodeType>> AddTreeViewItemCommand { get; private set; }
		private void OnAddTreeViewItemCommandExecute(KeyValuePair<TreeViewItem, ProjectNodeType> addingInfo)
		{
			TreeItemHeader nodeHeader = new TreeItemHeader();
			TreeItemHeaderViewModel nodeHeaderViewModel = (TreeItemHeaderViewModel)nodeHeader.DataContext;
			nodeHeaderViewModel.SetModel(new ProjectNode(addingInfo.Value, addingInfo.Value.ToString()));
			TreeViewItem treeViewItem = new TreeViewItem { Header = nodeHeader };
			nodeHeaderViewModel.AddCommand = ConvertAddingCommand(treeViewItem, AddTreeViewItemCommand);
			nodeHeaderViewModel.RemoveCommand = ConvertRemovingCommand(treeViewItem, RemoveTreeViewItemCommand);
			treeViewItem.Selected += (s, e) => { ChangeNodeOptionsView.Execute((TreeViewItem)s); };

			Binding contextMenuBinding = new Binding(nameof(TreeItemHeaderViewModel.ContextMenuProperty))
			{
				Source = nodeHeaderViewModel,
				Path = new PropertyPath(nameof(nodeHeaderViewModel.ContextMenu)),
				Mode = BindingMode.OneWay,
				UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
			};
			treeViewItem.SetBinding(FrameworkElement.ContextMenuProperty, contextMenuBinding);

			TreeViewItem sender = addingInfo.Key;
			TreeItemHeader senderHeader = (TreeItemHeader)sender.Header;
			TreeItemHeaderViewModel senderHeaderViewModel = (TreeItemHeaderViewModel)senderHeader.DataContext;
			senderHeaderViewModel.AddChild(nodeHeaderViewModel);
			sender.Items.Add(treeViewItem);
			sender.IsExpanded = true;
			treeViewItem.IsSelected = true;
		}

		public Command<TreeViewItem> RemoveTreeViewItemCommand { get; private set; }
		private void OnRemoveTreeViewItemCommandExecute(TreeViewItem sender)
		{
			TreeItemHeader senderHeader = (TreeItemHeader)sender.Header;
			TreeItemHeaderViewModel senderHeaderViewModel = (TreeItemHeaderViewModel)senderHeader.DataContext;
			senderHeaderViewModel.Remove();
			TreeViewItem parrent = GetParent(TreeItems, sender);
			parrent.Items.Remove(sender);
		}

		private Command<ProjectNodeType> ConvertAddingCommand(TreeViewItem sender, Command<KeyValuePair<TreeViewItem, ProjectNodeType>> command)
		{
			return new Command<ProjectNodeType>((projectNodeType) => command.Execute(new KeyValuePair<TreeViewItem, ProjectNodeType>(sender, projectNodeType)));
		}

		private Command ConvertRemovingCommand(TreeViewItem sender, Command<TreeViewItem> command)
		{
			return new Command(() => command.Execute(sender));
		}

		public Command<TreeViewItem> ChangeNodeOptionsView { get; private set; }
		private void OnChangeNodeOptionsViewExecute(TreeViewItem selectedNode)
		{
			if (!selectedNode.IsSelected) return;

			TreeItemHeader nodeHeader = (TreeItemHeader)selectedNode.Header;
			TreeItemHeaderViewModel nodeHeaderViewModel = (TreeItemHeaderViewModel)nodeHeader.DataContext;
			int selectedView = (int)nodeHeaderViewModel.NodeType - 1;
			if (selectedView < 0) selectedView = 0;
			SelectedViewTabIndex = selectedView;
		}

		public Command<UIElementCollection> BindOptionsView { get; private set; }
		private void OnBindOptionsViewExecute(UIElementCollection collection)
		{
			optionsView = collection;
			SelectedViewTabIndex = -1;
		}

		public Command EditProject { get; private set; }
		private async void OnEditProjectExecute()
		{
			DependencyObject invalid = ValidationHelper.GetFirstInvalid(NeedCreateProject ? dependedObjCreateProject : dependedObjEditProject, true);
			if (invalid != null)
			{
				(invalid as UIElement)?.Focus();
			}
			else 
			{
				if (NeedCreateProject)
					await CreateProject();
				else
					EditSelectedProject();
				ProjectSelected = true;
			}
		}

		public Command<DependencyObject> BindDependedObjCreateProject { get; private set; }
		private void OnBindDependedObjCreateProjectExecute(DependencyObject dependencyObject)
		{
			dependedObjCreateProject = dependencyObject;
		}

		public Command<DependencyObject> BindDependedObjEditProject { get; private set; }
		private void OnBindDependedObjEditProjectExecute(DependencyObject dependencyObject)
		{
			dependedObjEditProject = dependencyObject;
		}

		public Command LoadFromDatabase { get; private set; }
		public async void OnLoadFromDatabaseExecute()
		{
			if(state == ViewModelState.Initialized)
			{
				state = ViewModelState.Loading;
				await model.ConnectDB();
				ProjectList.AddRange(await model.LoadProjects());
				await model.DisconnectDB();
				state = ViewModelState.Loaded;
			}
		}

		#endregion

		#region Methods

		private TreeViewItem GetParent(IEnumerable enumerable, TreeViewItem treeViewItem)
		{
			foreach(TreeViewItem treeItem in enumerable)
			{
				if(treeItem.Items.Contains(treeViewItem))
				{
					return treeItem;
				}

				TreeViewItem parrent = GetParent(treeItem.Items, treeViewItem);
				if(parrent != null)
				{
					return parrent;
				}
			}
			return null;
		}

		private bool CanExecuteEditProject()
		{
			DependencyObject currentDependedObj = NeedCreateProject ? dependedObjCreateProject : dependedObjEditProject;
			return currentDependedObj != null && ValidationHelper.IsValid(currentDependedObj);
		}

		private async Task CreateProject()
		{
			Project project = new Project { Name = CreationProjectName };
			await model.ConnectDB();
			project = await model.CreateProject(project);
			await model.DisconnectDB();
			CreationProjectName = string.Empty;
			ProjectList.Add(project);
			SelectedEditProject = project;
			NeedCreateProject = false;
		}

		private void EditSelectedProject()
		{

		}

		#endregion
	}
}
