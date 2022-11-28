using ProjectsDb.Context;
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
using DocumentMaker.Security;

namespace ProjectEditorLib.ViewModel
{
	public class ProjectEditViewModel : DependencyObject, ICryptedConnectionStringRequired
	{
		ProjectEditModel model = new ProjectEditModel();

		UIElementCollection optionsView = null;
		int selectedViewTabIndex = -1;

		Snackbar snackbar = null;

		public ProjectEditViewModel()
		{
			InitCommands();

			State = ViewModelState.Initialized;
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

		public bool HaveUnsavedChanges
		{
			get { return (bool)GetValue(HaveUnsavedChangesProperty); }
			set { SetValue(HaveUnsavedChangesProperty, value); }
		}
		public static readonly DependencyProperty HaveUnsavedChangesProperty = DependencyProperty.Register(nameof(HaveUnsavedChanges), typeof(bool), typeof(ProjectEditViewModel));

		public ViewModelState State
		{
			get { return (ViewModelState)GetValue(StateProperty); }
			set { SetValue(StateProperty, value); }
		}
		public static readonly DependencyProperty StateProperty = DependencyProperty.Register(nameof(State), typeof(ViewModelState), typeof(ProjectEditViewModel));

		#endregion

		#region Commands

		private void InitCommands()
		{
			AddTreeViewItemCommand = new Command<KeyValuePair<TreeViewItem, ProjectNodeType>>(OnAddTreeViewItemCommandExecute);
			RemoveTreeViewItemCommand = new Command<TreeViewItem>(OnRemoveTreeViewItemCommandExecute);
			ChangeNodeOptionsView = new Command<TreeViewItem>(OnChangeNodeOptionsViewExecute);
			BindOptionsView = new Command<UIElementCollection>(OnBindOptionsViewExecute);
			CollapseAllTree = new Command(OnCollapseAllTreeExecute);
			Save = new Command(OnSaveExecute, CanExecuteSave);
			BindSnackbar = new Command<Snackbar>(OnBindSnackbarExecute);
			SendSnackbar = new Command<FrameworkElement>(OnSendSnackbarExecute);
			CancelChanges = new Command(OnCancelChangesExecute);
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
				if (!GetUserConfirmForRemoving(nodeModel)) return;

				await model.ConnectDBAsync();
				removed = await model.RemoveNodeAsync(nodeModel);
				await model.DisconnectDBAsync();
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
			int selectedView = (int)nodeHeaderViewModel.NodeType;
			if (selectedView < 0) selectedView = 0;
			SelectedViewTabIndex = selectedView;
			IDbObject modelContext = nodeHeaderViewModel.GetModel().Context;
			SelectedProjectNodeType = nodeHeaderViewModel.GetModel().Type;
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
			if(IsValidSelectedOptionView())
			{
				ProjectNode nodeModel = GetSaveTreeView();

				await model.ConnectDBAsync();
				await model.SaveNodeChangesAsync(nodeModel);
				await model.DisconnectDBAsync();

				UpdateContextAfterSaving(nodeModel);
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

		public Command CancelChanges { get; private set; }
		private void OnCancelChangesExecute()
		{
			if(!SelectedOptionsViewModel.CancelChanges())
			{
				RemoveTreeViewItemCommand.Execute(SelectedTreeViewItem);
			}
		}

		public ProjectNodeType SelectedProjectNodeType
		{
			get { return (ProjectNodeType)GetValue(SelectedProjectNodeTypeProperty); }
			set { SetValue(SelectedProjectNodeTypeProperty, value); }
		}
		public static readonly DependencyProperty SelectedProjectNodeTypeProperty = DependencyProperty.Register(nameof(SelectedProjectNodeType), typeof(ProjectNodeType), typeof(ProjectEditViewModel));

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
			await model.ConnectDBAsync();
			project = await model.CreateProjectAsync(project);
			await model.DisconnectDBAsync();
			CreationProjectName = string.Empty;
			SelectedEditProject = project;
		}

		public async Task LoadProject()
		{
			if (SelectedOptionsViewModel != null)
			{
				SelectedOptionsViewModel.HaveUnsavedChanges = false;
			}
			Task clearing = Task.Run(() =>
			{
				foreach (TreeViewItem treeItem in TreeItems)
				{
					while (Dispatcher.Invoke(() => treeItem.HasItems))
					{
						Dispatcher.Invoke(() => { treeItem.Items.Remove(treeItem.Items[0]); });
					}
				}
				Dispatcher.Invoke(() => { TreeItems.Clear(); });
			});

			TreeViewItem projectTreeItem = CreateTreeViewItem(ProjectNodeType.Project, SelectedEditProject, null);

			await model.ConnectDBAsync();
			Project project = await model.LoadProjectAsync(SelectedEditProject);
			await model.DisconnectDBAsync();
			projectTreeItem.IsExpanded = true;
			projectTreeItem.IsSelected = true;

			await clearing;
			TreeItems.Add(projectTreeItem);

			State = ViewModelState.Loaded;
			foreach (Back back in project.Backs)
			{
				PushBackToTreeItem(projectTreeItem, back);
				await Task.Delay(1);
			}
		}

		private void PushBackToTreeItem(TreeViewItem parrent, Back context)
		{
			TreeViewItem backNode = CreateNodeByType(context, parrent, out ProjectNodeType nodeType);
			parrent.Items.Add(backNode);

			switch (nodeType)
			{
				case ProjectNodeType.Episode:
				case ProjectNodeType.Back:
				case ProjectNodeType.Craft:
				case ProjectNodeType.Hog:
					foreach(Back back in context.ChildBacks)
					{
						PushBackToTreeItem(backNode, back);
					}
					break;
				case ProjectNodeType.Minigame:
					foreach(CountRegions regions in context.Regions)
					{
						backNode.Items.Add(CreateTreeViewItem(ProjectNodeType.Regions, regions, backNode));
					}
					break;
			}
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
			Back newEpisode = (Back)episodeContext;
			newEpisode.ProjectId = SelectedEditProject.Id;
			SelectedEditProject.Backs.Add(newEpisode);
		}

		private void BindNewBack(IDbObject backContext, TreeItemHeaderViewModel nodeViewModel)
		{
			Back back = (Back)backContext;
			IDbObject parrentDbObj = nodeViewModel.GetParrent().GetModel().Context;
			if (parrentDbObj is Back parrentEpisode)
			{
				back.BaseBackId = parrentEpisode.Id;
				back.ProjectId = parrentEpisode.ProjectId;
				if (parrentEpisode.ChildBacks == null)
				{
					parrentEpisode.ChildBacks = new List<Back>();
				}
				parrentEpisode.ChildBacks.Add(back);
			}
			else if(parrentDbObj is Project parrentProject)
			{
				back.ProjectId = parrentProject.Id;
				parrentProject.Backs.Add(back);
			}
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
					childBack.ProjectId = back.ProjectId;
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
			if (e.Source != (e.Source is TreeViewItem ? sender : (sender as TreeViewItem)?.Header)
				|| sender == SelectedTreeViewItem) return;

			if (e.ChangedButton == MouseButton.Left && e.LeftButton == MouseButtonState.Pressed && !CheckHaveUnsavedChangesAndSave())
			{
				e.Handled = true;
			}
			else if(e.ChangedButton == MouseButton.Left)
			{
				TreeViewItem s = (TreeViewItem)sender;
				s.IsSelected = false;
				s.IsSelected = true;
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
			HaveUnsavedChanges = SelectedViewTabIndex != -1 && SelectedOptionsViewModel.HaveUnsavedChanges;
			return HaveUnsavedChanges && ValidationHelper.IsValid(SelectedOptionsView);
		}

		public bool CheckHaveUnsavedChangesAndSave(bool isClosing = false)
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
					CancelChanges.Execute();
				}
				else if (res == MessageBoxResult.Yes)
				{
					if(isClosing)
					{
						SaveNodeChanges();
					}
					else
					{
						Save.Execute();
					}

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

		private bool IsValidSelectedOptionView()
		{
			DependencyObject invalid = ValidationHelper.GetFirstInvalid(SelectedOptionsView, true);
			if(invalid is UIElement elem)
			{
				elem.Focus();
				return false;
			}
			return true;
		}

		private ProjectNode GetSaveTreeView()
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

			return nodeModel;
		}

		private void UpdateContextAfterSaving(ProjectNode nodeModel)
		{
			if (nodeModel.Type == ProjectNodeType.Project)
			{
				SelectedEditProject = null;
				SelectedEditProject = (Project)nodeModel.Context;
				if (SelectedOptionsViewModel is ProjectViewModel)
				{
					SelectedOptionsViewModel.SetFromContext(nodeModel.Context);
				}
			}
		}

		public void SaveNodeChanges()
		{
			if (IsValidSelectedOptionView())
			{
				ProjectNode nodeModel = GetSaveTreeView();

				if (!model.TryConnectDB()) return;
				model.SaveNodeChanges(nodeModel);
				model.DisconnectDB();

				UpdateContextAfterSaving(nodeModel);
			}
		}

		private bool GetUserConfirmForRemoving(IDbObject dbObject)
		{
			return MessageBox.Show(
						"Ви впевнені, що хочете видалити \"" + dbObject.ToString() + "\" з усіма дочірніми вузлами (якщо вони є)?",
						"Видалення елементу",
						MessageBoxButton.OKCancel,
						MessageBoxImage.Question,
						MessageBoxResult.OK)
				== MessageBoxResult.OK;
		}

		public void SetCryptedConnectionString(CryptedConnectionString cryptedConnectionString)
		{
			model.SetConnectionString(cryptedConnectionString);
		}

		#endregion
	}
}
