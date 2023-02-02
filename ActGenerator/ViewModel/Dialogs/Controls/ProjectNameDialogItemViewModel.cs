using Mvvm.Commands;
using ProjectsDb.Context;
using System.Windows;

namespace ActGenerator.ViewModel.Dialogs.Controls
{
	class ProjectNameDialogItemViewModel : DependencyObject
	{
		public ProjectNameDialogItemViewModel() : base()
		{
			InitCommands();
		}

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

		public bool IsCheckedProject
		{
			get { return (bool)GetValue(IsCheckedProjectProperty); }
			set { SetValue(IsCheckedProjectProperty, value); }
		}
		public static readonly DependencyProperty IsCheckedProjectProperty = DependencyProperty.Register(nameof(IsCheckedProject), typeof(bool), typeof(ProjectNameDialogItemViewModel));

		public bool IsCheckedAlternativeProjectName
		{
			get { return (bool)GetValue(IsCheckedAlternativeProjectNameProperty); }
			set { SetValue(IsCheckedAlternativeProjectNameProperty, value); }
		}
		public static readonly DependencyProperty IsCheckedAlternativeProjectNameProperty = DependencyProperty.Register(nameof(IsCheckedAlternativeProjectName), typeof(bool), typeof(ProjectNameDialogItemViewModel));

		#endregion

		#region Commands

		private void InitCommands()
		{
			ProjectCheckedChanged = new Command(OnProjectCheckedChangedExecute);
			AlternativeProjectNameCheckedChanged = new Command(OnAlternativeProjectNameCheckedChangedExecute);
		}

		public Command ProjectCheckedChangedCommand { get; set; }
		public Command ProjectCheckedChanged { get; private set; }
		private void OnProjectCheckedChangedExecute()
		{
			ProjectCheckedChangedCommand?.Execute();
		}

		public Command AlternativeProjectNameCheckedChangedCommand { get; set; }
		public Command AlternativeProjectNameCheckedChanged { get; private set; }
		private void OnAlternativeProjectNameCheckedChangedExecute()
		{
			AlternativeProjectNameCheckedChangedCommand?.Execute();
		}

		#endregion
	}
}
