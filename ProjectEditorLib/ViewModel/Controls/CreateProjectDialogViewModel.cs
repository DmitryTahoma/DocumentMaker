using MaterialDesignThemes.Wpf;
using Mvvm;
using Mvvm.Commands;
using System.Windows;

namespace ProjectEditorLib.ViewModel.Controls
{
	public class CreateProjectDialogViewModel : DependencyObject
	{
		DependencyObject validateObj = null;

		public CreateProjectDialogViewModel()
		{
			InitCommands();
		}

		#region Properties

		public bool IsCreate { get; private set; } = false;

		public string ProjectName
		{
			get { return (string)GetValue(ProjectNameProperty); }
			set { SetValue(ProjectNameProperty, value); }
		}
		public static readonly DependencyProperty ProjectNameProperty = DependencyProperty.Register(nameof(ProjectName), typeof(string), typeof(CreateProjectDialogViewModel));

		public string DialogHostId { get; set; } = string.Empty;

		#endregion

		#region Commands

		private void InitCommands()
		{
			BindValidateObj = new Command<DependencyObject>(OnBindValidateObjExecute);
			OnDialogLoaded = new Command(OnOnDialogLoadedExecute);
			CreateProject = new Command(OnCreateProjectExecute, CanExecuteCreateProject);
			SetFocus = new Command<UIElement>(OnSetFocusExecute);
		}

		public Command<DependencyObject> BindValidateObj { get; private set; }
		private void OnBindValidateObjExecute(DependencyObject dependencyObject)
		{
			validateObj = dependencyObject;
		}

		public Command OnDialogLoaded { get; private set; }
		private void OnOnDialogLoadedExecute()
		{
			IsCreate = false;
			ProjectName = string.Empty;
		}

		public Command CreateProject { get; private set; }
		private void OnCreateProjectExecute()
		{
			if(ValidationHelper.GetFirstInvalid(validateObj, true) is UIElement uiElement)
			{
				uiElement.Focus();
				return;
			}

			IsCreate = true;
			DialogHost.Close(DialogHostId);
		}
		private bool CanExecuteCreateProject()
		{
			return validateObj != null && ValidationHelper.IsValid(validateObj);
		}

		public Command<UIElement> SetFocus { get; private set; }
		private void OnSetFocusExecute(UIElement uiElement)
		{
			uiElement.Focus();
		}

		#endregion
	}
}
