using Mvvm;
using Mvvm.Commands;
using ProjectEditorLib.Model;
using ProjectEditorLib.View;
using ProjectsDb.Context;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ProjectEditorLib.ViewModel
{
	public class ProjectRestoreViewModel
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
		}

		public Command LoadFromDatabase { get; private set; }
		private async void OnLoadFromDatabaseExecute()
		{
			await model.ConnectDB();
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
			nodeHeaderViewModel.SetModel(new ProjectNode(nodeType, context));
			TreeViewItem treeViewItem = new TreeViewItem { Header = nodeHeader };
			//nodeHeaderViewModel.AddCommand = ConvertAddingCommand(treeViewItem, AddTreeViewItemCommand);
			//nodeHeaderViewModel.RemoveCommand = ConvertRemovingCommand(treeViewItem, RemoveTreeViewItemCommand);

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

		#endregion
	}
}
