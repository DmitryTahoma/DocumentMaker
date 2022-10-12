using Db.Context;
using Mvvm;
using Mvvm.Commands;
using System.Collections.Generic;
using System.Windows.Controls;

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
	}
}
