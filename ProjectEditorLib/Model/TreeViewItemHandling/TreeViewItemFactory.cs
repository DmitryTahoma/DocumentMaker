using Mvvm.Commands;
using ProjectEditorLib.View;
using ProjectEditorLib.ViewModel;
using ProjectsDb.Context;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace ProjectEditorLib.Model.TreeViewItemHandling
{
	abstract class TreeViewItemFactory : ITreeViewItemFactory
	{
		public RoutedEventHandler TreeViewItemSelectedHandler { get; set; } = null;
		public MouseButtonEventHandler TreeViewItemPreviewMouseDownHandler { get; set; } = null;
		public MouseButtonEventHandler TreeViewItemPreviewMouseUpHandler { get; set; } = null;

		public virtual TreeViewItem CreateNodeByType(Back back, TreeViewItem parrent, out ProjectNodeType nodeType)
		{
			nodeType = (ProjectNodeType)Enum.Parse(typeof(ProjectNodeType), back.BackType.Name);
			return CreateTreeViewItem(nodeType, back, parrent);
		}

		public virtual TreeViewItem CreateTreeViewItem(ProjectNodeType nodeType, IDbObject context, TreeViewItem parrent)
		{
			ComparableTreeItemHeader nodeHeader = new ComparableTreeItemHeader();
			TreeItemHeaderViewModel nodeHeaderViewModel = (TreeItemHeaderViewModel)nodeHeader.DataContext;
			CreationInitTreeItemHeaderViewModel(nodeHeaderViewModel);
			nodeHeaderViewModel.SetModel(new ProjectNode(nodeType, context));
			TreeViewItem treeViewItem = new TreeViewItem { Header = nodeHeader };
			ResetTreeItemsSortDesriptions(treeViewItem.Items);
			CreationInitTreeViewItem(treeViewItem, nodeHeader, nodeHeaderViewModel);

			if (TreeViewItemSelectedHandler != null) treeViewItem.Selected += TreeViewItemSelectedHandler;
			if (TreeViewItemPreviewMouseDownHandler != null) treeViewItem.PreviewMouseDown += TreeViewItemPreviewMouseDownHandler;
			if (TreeViewItemPreviewMouseUpHandler != null) treeViewItem.PreviewMouseUp += TreeViewItemPreviewMouseUpHandler;

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

		public virtual void ResetTreeItemsSortDesriptions(ItemCollection items)
		{
			items.SortDescriptions.Clear();
			items.SortDescriptions.Add(new System.ComponentModel.SortDescription("Header", System.ComponentModel.ListSortDirection.Ascending));
		}

		protected virtual void CreationInitTreeItemHeaderViewModel(TreeItemHeaderViewModel treeItemHeaderViewModel) { }

		protected virtual void CreationInitTreeViewItem(TreeViewItem treeViewItem, TreeItemHeader treeItemHeader, TreeItemHeaderViewModel treeItemHeaderViewModel) { }

		protected Command ConvertTreeViewItemCommand(TreeViewItem sender, Command<TreeViewItem> command)
		{
			return new Command(() => command.Execute(sender));
		}
	}
}
