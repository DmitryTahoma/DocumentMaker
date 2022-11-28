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
	public class ProjectRestoreViewModel : ICryptedConnectionStringRequired
	{
		ProjectRestoreModel model = new ProjectRestoreModel();

		public ProjectRestoreViewModel()
		{
			InitCommands();
		}

		#region Properties

		public ObservableRangeCollection<TreeViewItem> TreeItems { get; private set; } = new ObservableRangeCollection<TreeViewItem>();

		#endregion

		#region Commands

		private void InitCommands()
		{
			LoadFromDatabase = new Command(OnLoadFromDatabaseExecute);
			RestoreTreeViewItemCommand = new Command<TreeViewItem>(OnRestoreTreeViewItemCommandExecute);
			RemoveTreeViewItemCommand = new Command<TreeViewItem>(OnRemoveTreeViewItemCommandExecute);
		}

		public Command LoadFromDatabase { get; private set; }
		private async void OnLoadFromDatabaseExecute()
		{
			if (!await model.TryConnectDB()) return;
			List<Project> projects = await model.LoadRemovedNodes();
			await model.DisconnectDB();

			TreeItems.Clear();
			foreach(Project project in projects)
			{
				TreeViewItem projectTree = CreateTreeViewItem(ProjectNodeType.Project, project, null);
				TreeItems.Add(projectTree);

				foreach(Back back in project.Backs)
				{
					projectTree.Items.Add(CreateNodeByType(back, projectTree, out _));
				}
			}
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
			TreeItemHeader nodeHeader = new TreeItemHeader();
			TreeItemHeaderViewModel nodeHeaderViewModel = (TreeItemHeaderViewModel)nodeHeader.DataContext;
			nodeHeaderViewModel.ShowFullName = true;
			nodeHeaderViewModel.SetModel(new ProjectNode(nodeType, context), TreeItemContextMenuType.ProjectRestore);
			TreeViewItem treeViewItem = new TreeViewItem { Header = nodeHeader };
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

		#endregion
	}
}
