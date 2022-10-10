using Db.Context;
using Mvvm;

namespace ProjectEditorLib.Model
{
	public class ProjectNode
	{
		public ProjectNode(ProjectNodeType type, string text)
		{
			Type = type;
			Text = text;
		}

		public ProjectNodeType Type { get; set; }
		public string Text { get; set; }

		public IDbObject DataContext { get; set; } = null;

		public ObservableRangeCollection<ProjectNode> ProjectNodes { get; set; } = null;
	}
}
