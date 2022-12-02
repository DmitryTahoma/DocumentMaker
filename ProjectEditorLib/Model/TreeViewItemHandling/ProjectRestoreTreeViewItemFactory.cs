using Mvvm.Commands;
using ProjectEditorLib.View;
using ProjectEditorLib.ViewModel;
using System.Windows.Controls;

namespace ProjectEditorLib.Model.TreeViewItemHandling
{
	class ProjectRestoreTreeViewItemFactory : TreeViewItemFactory
	{
		Command<TreeViewItem> restoringCommand = null;
		Command<TreeViewItem> removingCommand = null;

		public ProjectRestoreTreeViewItemFactory(Command<TreeViewItem> restoringCommand, Command<TreeViewItem> removingCommand)
		{
			this.restoringCommand = restoringCommand;
			this.removingCommand = removingCommand;
		}

		protected override void CreationInitTreeItemHeaderViewModel(TreeItemHeaderViewModel treeItemHeaderViewModel)
		{
			base.CreationInitTreeItemHeaderViewModel(treeItemHeaderViewModel);

			treeItemHeaderViewModel.ShowFullName = true;
		}

		protected override void CreationInitTreeViewItem(TreeViewItem treeViewItem, TreeItemHeader treeItemHeader, TreeItemHeaderViewModel treeItemHeaderViewModel)
		{
			base.CreationInitTreeViewItem(treeViewItem, treeItemHeader, treeItemHeaderViewModel);

			treeItemHeaderViewModel.RestoreCommand = ConvertTreeViewItemCommand(treeViewItem, restoringCommand);
			treeItemHeaderViewModel.RemoveCommand = ConvertTreeViewItemCommand(treeViewItem, removingCommand);

			treeItemHeaderViewModel.InitProjectRestoreContextMenu();
		}
	}
}
