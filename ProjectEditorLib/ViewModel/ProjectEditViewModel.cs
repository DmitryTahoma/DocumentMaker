using Db.Context;
using Db.Context.BackPart;
using DocumentMakerThemes.ViewModel;
using MaterialDesignThemes.Wpf;
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
using System.Windows.Input;

namespace ProjectEditorLib.ViewModel
{
	public class ProjectEditViewModel : DependencyObject
	{
		ProjectEditModel model = new ProjectEditModel();

		UIElementCollection optionsView = null;
		int selectedViewTabIndex = -1;

		DependencyObject dependedObjCreateProject = null;
		DependencyObject dependedObjEditProject = null;

		Snackbar snackbar = null;

		public ProjectEditViewModel()
		{
			InitCommands();
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
					if(i == selectedViewTabIndex)
					{
						elem.Visibility = Visibility.Visible;
						SelectedOptionsView = elem;
						SelectedOptionsViewModel = (IDbObjectViewModel)((FrameworkElement)elem).DataContext;
					}
					else
					{
						elem.Visibility = Visibility.Collapsed;
					}
					++i;
				}
			}
		}

		public UIElement SelectedOptionsView { get; private set; }

		public IDbObjectViewModel SelectedOptionsViewModel { get; private set; }

		public bool ProjectSelected
		{
			get { return (bool)GetValue(ProjectSelectedProperty); }
			set { SetValue(ProjectSelectedProperty, value); }
		}
		public static readonly DependencyProperty ProjectSelectedProperty = DependencyProperty.Register(nameof(ProjectSelected), typeof(bool), typeof(ProjectEditViewModel));

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

		public TreeViewItem SelectedTreeViewItem { get; private set; } = null;

		#endregion

		#region Commands

		private void InitCommands()
		{
			AddTreeViewItemCommand = new Command<KeyValuePair<TreeViewItem, ProjectNodeType>>(OnAddTreeViewItemCommandExecute);
			RemoveTreeViewItemCommand = new Command<TreeViewItem>(OnRemoveTreeViewItemCommandExecute);
			ChangeNodeOptionsView = new Command<TreeViewItem>(OnChangeNodeOptionsViewExecute);
			BindOptionsView = new Command<UIElementCollection>(OnBindOptionsViewExecute);
			BindDependedObjCreateProject = new Command<DependencyObject>(OnBindDependedObjCreateProjectExecute);
			BindDependedObjEditProject = new Command<DependencyObject>(OnBindDependedObjEditProjectExecute);
			CollapseAllTree = new Command(OnCollapseAllTreeExecute);
			Save = new Command(OnSaveExecute, CanExecuteSave);
			BindSnackbar = new Command<Snackbar>(OnBindSnackbarExecute);
			SendSnackbar = new Command<FrameworkElement>(OnSendSnackbarExecute);
		}

		public Command<KeyValuePair<TreeViewItem, ProjectNodeType>> AddTreeViewItemCommand { get; private set; }
		private void OnAddTreeViewItemCommandExecute(KeyValuePair<TreeViewItem, ProjectNodeType> addingInfo)
		{
			TreeViewItem parrent = addingInfo.Key;
			TreeViewItem treeViewItem = CreateTreeViewItem(addingInfo.Value, null, parrent);

			parrent.Items.Add(treeViewItem);
			parrent.IsExpanded = true;
			treeViewItem.IsSelected = true;
		}

		public Command<TreeViewItem> RemoveTreeViewItemCommand { get; private set; }
		private async void OnRemoveTreeViewItemCommandExecute(TreeViewItem sender)
		{
			TreeItemHeader senderHeader = (TreeItemHeader)sender.Header;
			TreeItemHeaderViewModel senderHeaderViewModel = (TreeItemHeaderViewModel)senderHeader.DataContext;
			IDbObject nodeModel = senderHeaderViewModel.GetModel()?.Context;
			bool removed = nodeModel == null;

			if(!removed)
			{
				await model.ConnectDB();
				removed = await model.RemoveNode(nodeModel);
				await model.DisconnectDB();
			}

			if(removed)
			{
				senderHeaderViewModel.Remove();
				TreeViewItem parrent = GetParent(TreeItems, sender);
				parrent.Items.Remove(sender);
			}
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
			SelectedTreeViewItem = selectedNode;

			TreeItemHeader nodeHeader = (TreeItemHeader)selectedNode.Header;
			TreeItemHeaderViewModel nodeHeaderViewModel = (TreeItemHeaderViewModel)nodeHeader.DataContext;
			int selectedView = (int)nodeHeaderViewModel.NodeType - 1;
			if (selectedView < 0) selectedView = 0;
			SelectedViewTabIndex = selectedView;
			IDbObject modelContext = nodeHeaderViewModel.GetModel().Context;
			SelectedOptionsViewModel.SetFromContext(modelContext);
			SelectedOptionsViewModel.HaveUnsavedChanges = modelContext == null;
		}

		public Command<UIElementCollection> BindOptionsView { get; private set; }
		private void OnBindOptionsViewExecute(UIElementCollection collection)
		{
			if(optionsView == null)
			{
				optionsView = collection;
				SelectedViewTabIndex = -1;
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

		public Command CollapseAllTree { get; private set; }
		private void OnCollapseAllTreeExecute()
		{
			if(CheckHaveUnsavedChangesAndSave())
			{
				CollapseTreeItems(TreeItems);
			}
		}

		public Command Save { get; private set; }
		private async void OnSaveExecute()
		{
			DependencyObject invalid = ValidationHelper.GetFirstInvalid(SelectedOptionsView, true);
			if (invalid != null)
			{
				(invalid as UIElement)?.Focus();
			}
			else
			{
				TreeItemHeaderViewModel nodeViewModel = (TreeItemHeaderViewModel)((TreeItemHeader)SelectedTreeViewItem.Header).DataContext;
				ProjectNode nodeModel = nodeViewModel.GetModel();
				bool isNewNode = nodeModel.Context == null;
				nodeModel.Context = SelectedOptionsViewModel.UpdateContext(nodeModel.Context);
				SelectedOptionsViewModel.HaveUnsavedChanges = false;
				if (isNewNode)
				{
					switch (nodeModel.Type)
					{
						case ProjectNodeType.Episode: BindNewEpisode(nodeModel.Context); break;
						case ProjectNodeType.Back:
						case ProjectNodeType.Craft: BindNewBack(nodeModel.Context, nodeViewModel); break;
						case ProjectNodeType.Minigame:
						case ProjectNodeType.Dialog:
						case ProjectNodeType.Hog: BindNewChildBack(nodeModel.Context, nodeViewModel); break;
						case ProjectNodeType.Regions: BindNewRegions(nodeModel.Context, nodeViewModel); break;
					}
				}

				nodeViewModel.UpdateText();

				await model.ConnectDB();
				await model.SaveNodeChanges(nodeModel);
				await model.DisconnectDB();

				if(nodeModel.Type == ProjectNodeType.Project)
				{
					SelectedEditProject = null;
					SelectedEditProject = (Project)nodeModel.Context;
					SelectedOptionsViewModel.SetFromContext(nodeModel.Context);
				}
			}
		}

		public Command<Snackbar> BindSnackbar { get; private set; }
		private void OnBindSnackbarExecute(Snackbar snackbar)
		{
			this.snackbar = snackbar;
		}

		public Command<FrameworkElement> SendSnackbar { get; private set; }
		private void OnSendSnackbarExecute(FrameworkElement target)
		{
			if(target.DataContext is ISnackbarRequired targetObj)
			{
				targetObj.SetSnackbar(snackbar);
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

		public async Task CreateProject()
		{
			Project project = new Project { Name = CreationProjectName };
			await model.ConnectDB();
			project = await model.CreateProject(project);
			await model.DisconnectDB();
			CreationProjectName = string.Empty;
			SelectedEditProject = project;
		}

		public async Task LoadProject()
		{
			TreeItems.Clear();
			TreeViewItem projectTreeItem = CreateTreeViewItem(ProjectNodeType.Project, SelectedEditProject, null);
			TreeItems.Add(projectTreeItem);

			await model.ConnectDB();
			Project project = await model.LoadProject(SelectedEditProject);
			await model.DisconnectDB();
			foreach (Episode episode in project.Episodes)
			{
				TreeViewItem episodeTreeItem = CreateTreeViewItem(ProjectNodeType.Episode, episode, projectTreeItem);
				projectTreeItem.Items.Add(episodeTreeItem);

				foreach(Back back in episode.Backs)
				{
					TreeViewItem backNode = CreateNodeByType(back, episodeTreeItem, out ProjectNodeType nodeType);
					if (nodeType == ProjectNodeType.Minigame)
					{
						foreach (CountRegions regions in back.Regions)
						{
							backNode.Items.Add(CreateTreeViewItem(ProjectNodeType.Regions, regions, backNode));
						}
					}
					if (backNode != null)
					{
						episodeTreeItem.Items.Add(backNode);
						await SetChildsBacks(backNode, back);
					}
				}
			}
			projectTreeItem.IsExpanded = true;
			projectTreeItem.IsSelected = true;
		}

		private async Task SetChildsBacks(TreeViewItem parrentNode, Back back)
		{
			foreach(Back childBack in back.ChildBacks)
			{
				TreeViewItem childBackNode = CreateNodeByType(childBack, parrentNode, out ProjectNodeType nodeType);
				if (nodeType == ProjectNodeType.Minigame)
				{
					foreach(CountRegions regions in childBack.Regions)
					{
						childBackNode.Items.Add(CreateTreeViewItem(ProjectNodeType.Regions, regions, childBackNode));
					}
				}

				if (childBackNode != null)
				{
					parrentNode.Items.Add(childBackNode);
					await SetChildsBacks(childBackNode, childBack);
				}
			}
		}

		private TreeViewItem CreateNodeByType(Back back, TreeViewItem parrent, out ProjectNodeType nodeType)
		{
			nodeType = (ProjectNodeType)Enum.Parse(typeof(ProjectNodeType), back.BackType.Name);
			return CreateTreeViewItem(nodeType, back, parrent);
		}

		private TreeViewItem CreateTreeViewItem(ProjectNodeType nodeType, IDbObject context, TreeViewItem parrent)
		{
			TreeItemHeader nodeHeader = new TreeItemHeader();
			TreeItemHeaderViewModel nodeHeaderViewModel = (TreeItemHeaderViewModel)nodeHeader.DataContext;
			nodeHeaderViewModel.SetModel(new ProjectNode(nodeType, context));
			TreeViewItem treeViewItem = new TreeViewItem { Header = nodeHeader };
			nodeHeaderViewModel.AddCommand = ConvertAddingCommand(treeViewItem, AddTreeViewItemCommand);
			nodeHeaderViewModel.RemoveCommand = ConvertRemovingCommand(treeViewItem, RemoveTreeViewItemCommand);
			treeViewItem.Selected += (s, e) => { ChangeNodeOptionsView.Execute((TreeViewItem)s); };
			treeViewItem.PreviewMouseDown += BeforeTreeViewItemSelected;
			treeViewItem.PreviewMouseUp += BeforeTreeViewItemShowContextMenu;

			Binding contextMenuBinding = new Binding(nameof(TreeItemHeaderViewModel.ContextMenuProperty))
			{
				Source = nodeHeaderViewModel,
				Path = new PropertyPath(nameof(nodeHeaderViewModel.ContextMenu)),
				Mode = BindingMode.OneWay,
				UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
			};
			treeViewItem.SetBinding(FrameworkElement.ContextMenuProperty, contextMenuBinding);

			if (parrent != null)
			{
				TreeItemHeader parrentHeader = (TreeItemHeader)parrent.Header;
				TreeItemHeaderViewModel parrentHeaderViewModel = (TreeItemHeaderViewModel)parrentHeader.DataContext;
				parrentHeaderViewModel.AddChild(nodeHeaderViewModel);
			}

			return treeViewItem;
		}

		private void CollapseTreeItems(IEnumerable treeItems)
		{
			foreach(TreeViewItem item in treeItems)
			{
				item.IsExpanded = false;
				CollapseTreeItems(item.Items);
			}
		}

		private void BindNewEpisode(IDbObject episodeContext)
		{
			Episode newEpisode = (Episode)episodeContext;
			newEpisode.ProjectId = SelectedEditProject.Id;
			SelectedEditProject.Episodes.Add(newEpisode);
		}

		private void BindNewBack(IDbObject backContext, TreeItemHeaderViewModel nodeViewModel)
		{
			Back back = (Back)backContext;
			back.EpisodeId = nodeViewModel.GetParrent().GetModel().Context.Id;
			Episode parrentEpisode = (Episode)nodeViewModel.GetParrent().GetModel().Context;
			if (parrentEpisode.Backs == null)
			{
				parrentEpisode.Backs = new List<Back>();
			}
			parrentEpisode.Backs.Add(back);
		}

		private void BindNewChildBack(IDbObject childBackContext, TreeItemHeaderViewModel nodeViewModel)
		{
			Back childBack = (Back)childBackContext;
			TreeItemHeaderViewModel parrentBackNodeViewModel = nodeViewModel.GetParrent();
			childBack.BaseBackId = parrentBackNodeViewModel.GetModel().Context.Id;
			while (parrentBackNodeViewModel != null)
			{
				if (parrentBackNodeViewModel.GetModel().Context is Back back)
				{
					childBack.EpisodeId = back.EpisodeId;
					if (back.ChildBacks == null)
					{
						back.ChildBacks = new List<Back>();
					}
					back.ChildBacks.Add(childBack);
					break;
				}
				parrentBackNodeViewModel = parrentBackNodeViewModel.GetParrent();
			}
		}

		private void BindNewRegions(IDbObject regionsContext, TreeItemHeaderViewModel nodeViewModel)
		{
			CountRegions regions= (CountRegions)regionsContext;
			regions.BackId = nodeViewModel.GetParrent().GetModel().Context.Id;
			Back parrentBackContext = (Back)nodeViewModel.GetParrent().GetModel().Context;
			if (parrentBackContext.Regions == null)
			{
				parrentBackContext.Regions = new List<CountRegions>();
			}
			parrentBackContext.Regions.Add(regions);
		}

		private void BeforeTreeViewItemSelected(object sender, MouseButtonEventArgs e) // TreeViewItem.PreviewMouseDown
		{
			if(e.ChangedButton == MouseButton.Left && e.LeftButton == MouseButtonState.Pressed && !CheckHaveUnsavedChangesAndSave())
			{
				e.Handled = true;
			}
		}

		private void BeforeTreeViewItemShowContextMenu(object sender, MouseButtonEventArgs e) // TreeViewItem.PreviewMouseUp
		{
			if (e.ChangedButton == MouseButton.Right && e.RightButton == MouseButtonState.Released && sender is TreeViewItem treeViewItem)
			{
				bool canAdd = SelectedViewTabIndex == -1 || !SelectedOptionsViewModel.HaveUnsavedChanges;
				foreach (UIElement elem in treeViewItem.ContextMenu.Items)
				{
					if (elem is MenuItem menuItem && menuItem.DataContext is NodeContextMenuItemType menuType && menuType == NodeContextMenuItemType.Adding)
					{
						menuItem.IsEnabled = canAdd;
					}
				}
			}
		}

		private bool CanExecuteSave()
		{
			return SelectedViewTabIndex != -1 && SelectedOptionsViewModel.HaveUnsavedChanges && ValidationHelper.IsValid(SelectedOptionsView);
		}

		private bool CheckHaveUnsavedChangesAndSave()
		{
			if(SelectedViewTabIndex != -1 && SelectedOptionsViewModel.HaveUnsavedChanges)
			{
				MessageBoxResult res = MessageBox.Show(
					"Обраний елемент має незбережені зміни. Зберегти зміни?",
					"Зміна елементу",
					MessageBoxButton.YesNoCancel,
					MessageBoxImage.Question,
					MessageBoxResult.Yes);

				if (res == MessageBoxResult.No)
				{
					TreeItemHeaderViewModel nodeViewModel = (TreeItemHeaderViewModel)((TreeItemHeader)SelectedTreeViewItem.Header).DataContext;
					ProjectNode nodeModel = nodeViewModel.GetModel();
					if (nodeModel.Context == null)
					{
						RemoveTreeViewItemCommand.Execute(SelectedTreeViewItem);
					}
				}
				else if (res == MessageBoxResult.Yes)
				{
					Save.Execute();

					if (SelectedOptionsViewModel.HaveUnsavedChanges)
					{
						return false;
					}
				}
				else
				{
					return false;
				}
			}
			return true;
		}

		#endregion
	}
}
