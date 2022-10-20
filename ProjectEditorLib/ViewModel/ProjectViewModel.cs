using Db.Context;
using Db.Context.BackPart;
using System.Windows;

namespace ProjectEditorLib.ViewModel
{
	public class ProjectViewModel : BaseDbObjectViewModel, IDbObjectViewModel
	{
		public ProjectViewModel() : base() { }

		#region Properties

		public string ProjectName
		{
			get { return (string)GetValue(ProjectNameProperty); }
			set { SetValue(ProjectNameProperty, value); }
		}
		public static readonly DependencyProperty ProjectNameProperty = DependencyProperty.Register(nameof(ProjectName), typeof(string), typeof(ProjectViewModel));

		#endregion

		#region Methods

		public override void SetFromContext(IDbObject dbObject)
		{
			if(dbObject is Project project)
			{
				ProjectName = project.Name;
			}
			else
			{
				ProjectName = string.Empty;
			}
		}

		public override IDbObject UpdateContext(IDbObject dbObject)
		{
			Project project;

			if(dbObject == null)
			{
				project = new Project();
			}
			else
			{
				project = dbObject as Project;
			}

			if(project != null)
			{
				project.Name = ProjectName;
				dbObject = project;
			}

			return dbObject;
		}

		#endregion
	}
}
