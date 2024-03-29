﻿using ProjectsDb.Context;

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

		public override string ToString()
		{
			return ConextToString(Type, Context);
		}

		public string ConextToString(ProjectNodeType type, IDbObject context)
		{
			return context == null
				? string.Empty
				: type == ProjectNodeType.Regions && context is CountRegions regions
				? "Регіони (" + regions.Count.ToString() + ")"
				: type == ProjectNodeType.Craft && context is Back craft ? craft.Name : context.ToString();
		}
	}
}
