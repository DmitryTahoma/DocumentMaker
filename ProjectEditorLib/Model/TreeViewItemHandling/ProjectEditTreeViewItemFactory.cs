using Mvvm.Commands;
using ProjectEditorLib.View;
using ProjectEditorLib.ViewModel;
using System.Collections.Generic;
using System.Windows.Controls;

namespace ProjectEditorLib.Model.TreeViewItemHandling
{
	class ProjectEditTreeViewItemFactory : TreeViewItemFactory
	{
		private Command<KeyValuePair<TreeViewItem, ProjectNodeType>> addingCommand = null;
		private Command<TreeViewItem> removingCommand = null;

		public ProjectEditTreeViewItemFactory(Command<KeyValuePair<TreeViewItem, ProjectNodeType>> addingCommand, Command<TreeViewItem> removingCommand) : base()
		{
			this.addingCommand = addingCommand;
			this.removingCommand = removingCommand;
		}

		protected override void CreationInitTreeViewItem(TreeViewItem treeViewItem, TreeItemHeader treeItemHeader, TreeItemHeaderViewModel treeItemHeaderViewModel)
		{
			base.CreationInitTreeViewItem(treeViewItem, treeItemHeader, treeItemHeaderViewModel);

			treeItemHeaderViewModel.AddCommand = new Command<ProjectNodeType>(
				(projectNodeType) => addingCommand.Execute(new KeyValuePair<TreeViewItem, ProjectNodeType>(treeViewItem, projectNodeType)));
			treeItemHeaderViewModel.RemoveCommand = ConvertTreeViewItemCommand(treeViewItem, removingCommand);

			treeItemHeaderViewModel.InitProjectEditionContextMenu();
		}
	}
}
