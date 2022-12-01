using DocumentMaker.Security;
using Mvvm;
using Mvvm.Commands;
using ProjectEditorLib.Model;
using ProjectEditorLib.View;
using ProjectsDb.Context;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ProjectEditorLib.ViewModel
{
	public class ProjectRestoreViewModel : DependencyObject, ICryptedConnectionStringRequired
	{
		ProjectRestoreModel model = new ProjectRestoreModel();

		public ProjectRestoreViewModel()
		{
			InitCommands();
		}

		#region Properties

		public ObservableRangeCollection<TreeViewItem> TreeItems { get; private set; } = new ObservableRangeCollection<TreeViewItem>();

		public ViewModelState State
		{
			get { return (ViewModelState)GetValue(StateProperty); }
			set { SetValue(StateProperty, value); }
		}
		public static readonly DependencyProperty StateProperty = DependencyProperty.Register(nameof(State), typeof(ViewModelState), typeof(ProjectRestoreViewModel));

		public double LoadingBacksProgress
		{
			get { return (double)GetValue(LoadingBacksProgressProperty); }
			set { SetValue(LoadingBacksProgressProperty, value); }
		}
		public static readonly DependencyProperty LoadingBacksProgressProperty = DependencyProperty.Register(nameof(LoadingBacksProgress), typeof(double), typeof(ProjectRestoreViewModel));

		public double LoadingBacksMaximum
		{
			get { return (double)GetValue(LoadingBacksMaximumProperty); }
			set { SetValue(LoadingBacksMaximumProperty, value); }
		}
		public static readonly DependencyProperty LoadingBacksMaximumProperty = DependencyProperty.Register(nameof(LoadingBacksMaximum), typeof(double), typeof(ProjectRestoreViewModel));

		public bool IsLoadingBacks
		{
			get { return (bool)GetValue(IsLoadingBacksProperty); }
			set { SetValue(IsLoadingBacksProperty, value); }
		}
		public static readonly DependencyProperty IsLoadingBacksProperty = DependencyProperty.Register(nameof(IsLoadingBacks), typeof(bool), typeof(ProjectRestoreViewModel));

		#endregion

		#region Commands

		private void InitCommands()
		{
			RestoreTreeViewItemCommand = new Command<TreeViewItem>(OnRestoreTreeViewItemCommandExecute);
			RemoveTreeViewItemCommand = new Command<TreeViewItem>(OnRemoveTreeViewItemCommandExecute);
		}

		public Command<TreeViewItem> RestoreTreeViewItemCommand { get; private set; }
		private async void OnRestoreTreeViewItemCommandExecute(TreeViewItem sender)
		{
			TreeItemHeaderViewModel headerViewModel = (TreeItemHeaderViewModel)((FrameworkElement)sender.Header).DataContext;
			ProjectNode projectNode = headerViewModel.GetModel();
			if(projectNode.Type == ProjectNodeType.Project)
			{
				TreeItems.Remove(sender);
			}
			else
			{
				TreeViewItem parent = sender.Parent as TreeViewItem;
				parent.Items.Remove(sender);

				if(!parent.HasItems)
				{
					TreeItems.Remove(parent);
				}
			}

			if (!await model.TryConnectDB()) return;
			await model.Restore(projectNode.Context);
			await model.DisconnectDB();
		}

		public Command<TreeViewItem> RemoveTreeViewItemCommand { get; private set; }
		private async void OnRemoveTreeViewItemCommandExecute(TreeViewItem sender)
		{
			TreeItemHeaderViewModel headerViewModel = (TreeItemHeaderViewModel)((FrameworkElement)sender.Header).DataContext;
			ProjectNode projectNode = headerViewModel.GetModel();
			TreeViewItem projectTreeItem = null;

			if (!GetUserConfirmForRemovingForever(projectNode.Context)) return;

			if (projectNode.Type == ProjectNodeType.Project)
			{
				TreeItems.Remove(sender);
			}
			else
			{
				projectTreeItem = sender.Parent as TreeViewItem;
			}

			if (!await model.TryConnectDB()) return;
			IEnumerable<IDbObject> removedObjects = await model.RemoveForever(projectNode.Context);
			await model.DisconnectDB();

			if(projectTreeItem != null)
			{
				foreach(TreeViewItem removeTreeViewItem in GetTreeViewItemsByContext(projectTreeItem, removedObjects))
				{
					projectTreeItem.Items.Remove(removeTreeViewItem);
				}
			}
		}

		#endregion

		#region Methods

		private TreeViewItem CreateNodeByType(Back back, TreeViewItem parrent, out ProjectNodeType nodeType)
		{
			nodeType = (ProjectNodeType)Enum.Parse(typeof(ProjectNodeType), back.BackType.Name);
			return CreateTreeViewItem(nodeType, back, parrent);
		}

		private TreeViewItem CreateTreeViewItem(ProjectNodeType nodeType, IDbObject context, TreeViewItem parrent)
		{
			ComparableTreeItemHeader nodeHeader = new ComparableTreeItemHeader();
			TreeItemHeaderViewModel nodeHeaderViewModel = (TreeItemHeaderViewModel)nodeHeader.DataContext;
			nodeHeaderViewModel.ShowFullName = true;
			nodeHeaderViewModel.SetModel(new ProjectNode(nodeType, context), TreeItemContextMenuType.ProjectRestore);
			TreeViewItem treeViewItem = new TreeViewItem { Header = nodeHeader };
			treeViewItem.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription("Header", System.ComponentModel.ListSortDirection.Ascending));
			nodeHeaderViewModel.RestoreCommand = ConvertTreeViewItemCommand(treeViewItem, RestoreTreeViewItemCommand);
			nodeHeaderViewModel.RemoveCommand = ConvertTreeViewItemCommand(treeViewItem, RemoveTreeViewItemCommand);

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

		private Command ConvertTreeViewItemCommand(TreeViewItem sender, Command<TreeViewItem> command)
		{
			return new Command(() => command.Execute(sender));
		}

		private TreeViewItem GetParent(IEnumerable enumerable, TreeViewItem treeViewItem)
		{
			foreach (TreeViewItem treeItem in enumerable)
			{
				if (treeItem.Items.Contains(treeViewItem))
				{
					return treeItem;
				}

				TreeViewItem parrent = GetParent(treeItem.Items, treeViewItem);
				if (parrent != null)
				{
					return parrent;
				}
			}
			return null;
		}

		private IEnumerable<TreeViewItem> GetTreeViewItemsByContext(TreeViewItem root, IEnumerable<IDbObject> dbObjects)
		{
			Dictionary<IDbObject, TreeViewItem> treeItemsWithModel =
					root
					.Items
					.Cast<TreeViewItem>()
					.ToDictionary(key => ((TreeItemHeaderViewModel)((TreeItemHeader)key.Header).DataContext).GetModel().Context);

			foreach (IDbObject dbObject in dbObjects)
			{
				IDbObject key =
					treeItemsWithModel
					.Keys
					.FirstOrDefault(x => x.Id == dbObject.Id && x.GetType() == dbObject.GetType());

				if (key != null)
				{
					yield return treeItemsWithModel[key];
				}
			}
		}

		private bool GetUserConfirmForRemovingForever(IDbObject dbObject)
		{
			return MessageBox.Show(
						"Ви впевнені, що хочете видалити назавжди \"" + dbObject.ToString() + "\" з усіма дочірніми вузлами? (цю дію неможливо відмінити)",
						"Видалення елементу",
						MessageBoxButton.OKCancel,
						MessageBoxImage.Exclamation,
						MessageBoxResult.Cancel)
				== MessageBoxResult.OK;
		}

		public void SetCryptedConnectionString(CryptedConnectionString cryptedConnectionString)
		{
			model.SetConnectionString(cryptedConnectionString);
		}

		public async Task LoadRemovedNodes()
		{
			Task clearing = Task.Run(() =>
			{
				foreach(TreeViewItem projectTreeItem in TreeItems)
				{
					foreach(TreeViewItem backTreeItem in Dispatcher.Invoke(projectTreeItem.Items.Cast<TreeViewItem>))
					{
						while(Dispatcher.Invoke(() => backTreeItem.HasItems))
						{
							Dispatcher.Invoke(() => { backTreeItem.Items.Remove(backTreeItem.Items[0]); });
						}
					}
					Dispatcher.Invoke(projectTreeItem.Items.Clear);
				}
				Dispatcher.Invoke(TreeItems.Clear);
			});

			if (!await model.TryConnectDB()) return;
			List<Project> projects = await model.LoadRemovedNodes();
			await model.DisconnectDB();

			await clearing;

			State = ViewModelState.Loaded;
			LoadingBacksProgress = 0;
			LoadingBacksMaximum = projects.Sum(x => x.Backs.Count) + projects.Count;
			IsLoadingBacks = true;
			foreach (Project project in projects)
			{
				TreeViewItem projectTree = CreateTreeViewItem(ProjectNodeType.Project, project, null);
				AddProjectTreeItem(projectTree);
				++LoadingBacksProgress;
				await Task.Delay(1);

				foreach (Back back in project.Backs)
				{
					projectTree.Items.Add(CreateNodeByType(back, projectTree, out _));
					++LoadingBacksProgress;
					await Task.Delay(1);
				}

				// update sorting
				if(projectTree.Items.Count > 1)
				{
					projectTree.Items.Refresh();
				}
			}
			IsLoadingBacks = false;
		}

		private void AddProjectTreeItem(TreeViewItem treeViewItem)
		{
			IComparable comparable = treeViewItem.Header as IComparable;
			int i = 0;
			bool inserted = false;
			foreach (TreeViewItem treeItem in TreeItems)
			{
				if (comparable.CompareTo(treeItem.Header) < 0)
				{
					TreeItems.Insert(i, treeViewItem);
					inserted = true;
					break;
				}

				++i;
			}

			if (!inserted)
			{
				TreeItems.Add(treeViewItem);
			}
		}

		#endregion
	}
}
