using ProjectsDb.Context;
using System.Windows.Controls;

namespace ProjectEditorLib.Model.TreeViewItemHandling
{
	interface ITreeViewItemFactory
	{
		TreeViewItem CreateNodeByType(Back back, TreeViewItem parrent, out ProjectNodeType nodeType);
		TreeViewItem CreateTreeViewItem(ProjectNodeType nodeType, IDbObject context, TreeViewItem parrent);
		void ResetTreeItemsSortDesriptions(ItemCollection items);
	}
}
