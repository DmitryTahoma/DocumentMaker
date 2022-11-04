using ProjectsDb.Context;

namespace ProjectEditorLib.Model
{
	public class ProjectNode
	{
		public ProjectNode(ProjectNodeType type, IDbObject context)
		{
			Type = type;
			Context = context;
		}

		public ProjectNodeType Type { get; set; }
		public IDbObject Context { get; set; }
	}
}
