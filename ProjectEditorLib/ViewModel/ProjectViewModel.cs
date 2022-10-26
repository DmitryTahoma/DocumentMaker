using Db.Context;
using Db.Context.BackPart;
using Mvvm.Commands;
using ProjectEditorLib.View;
using System.Windows;
using System.Windows.Controls;

namespace ProjectEditorLib.ViewModel
{
	public class ProjectViewModel : BaseDbObjectViewModel, IDbObjectViewModel
	{
		UIElementCollection projectNamesCollection = null;

		public ProjectViewModel() : base() { }

		#region Properties

		public string ProjectName
		{
			get { return (string)GetValue(ProjectNameProperty); }
			set { SetValue(ProjectNameProperty, value); }
		}
		public static readonly DependencyProperty ProjectNameProperty = DependencyProperty.Register(nameof(ProjectName), typeof(string), typeof(ProjectViewModel));

		#endregion

		#region Commands

		protected override void InitCommands()
		{
			base.InitCommands();

			BindAltProjectNamesCollection = new Command<UIElementCollection>(OnBindAltProjectNamesCollectionExecute);
			AddAltProjectName = new Command(OnAddAltProjectNameExecute);
			DeleteAltProjectName = new Command<AlternativeProjectNameView>(OnDeleteAltProjectNameExecute);
		}

		public Command<UIElementCollection> BindAltProjectNamesCollection { get; private set; }
		private void OnBindAltProjectNamesCollectionExecute(UIElementCollection collection)
		{
			projectNamesCollection = collection;
		}

		public Command AddAltProjectName { get; private set; }
		private void OnAddAltProjectNameExecute()
		{
			AlternativeProjectNameView altProjectName = new AlternativeProjectNameView();
			AlternativeProjectNameViewModel altProjectNameViewModel = (AlternativeProjectNameViewModel)altProjectName.DataContext;
			altProjectNameViewModel.DeleteAltProjectName = DeleteAltProjectName;
			projectNamesCollection.Add(altProjectName);
		}

		public Command<AlternativeProjectNameView> DeleteAltProjectName { get; private set; }
		private void OnDeleteAltProjectNameExecute(AlternativeProjectNameView sender)
		{
			projectNamesCollection.Remove(sender);
		}

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
