using ProjectsDb.Context;
using System.Windows;

namespace ActGenerator.ViewModel.Dialogs.Controls
{
	class ProjectNameDialogItemViewModel : DependencyObject
	{
		#region Properties

		public Project Project
		{
			get { return (Project)GetValue(ProjectProperty); }
			set { SetValue(ProjectProperty, value); }
		}
		public static readonly DependencyProperty ProjectProperty = DependencyProperty.Register(nameof(Project), typeof(Project), typeof(ProjectNameDialogItemViewModel));

		public AlternativeProjectName AlternativeProjectName
		{
			get { return (AlternativeProjectName)GetValue(AlternativeProjectNameProperty); }
			set { SetValue(AlternativeProjectNameProperty, value); }
		}
		public static readonly DependencyProperty AlternativeProjectNameProperty = DependencyProperty.Register(nameof(AlternativeProjectName), typeof(AlternativeProjectName), typeof(ProjectNameDialogItemViewModel));

		#endregion
	}
}
