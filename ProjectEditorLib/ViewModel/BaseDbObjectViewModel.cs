using ProjectsDb.Context;
using Mvvm.Commands;
using System.Windows;

namespace ProjectEditorLib.ViewModel
{
	public abstract class BaseDbObjectViewModel : DependencyObject, IDbObjectViewModel
	{
		protected IDbObject context = null;

		public BaseDbObjectViewModel()
		{
			InitCommands();
		}

		#region Properties

		public bool HaveUnsavedChanges
		{
			get { return (bool)GetValue(HaveUnsavedChangesProperty); }
			set { SetValue(HaveUnsavedChangesProperty, value); }
		}
		public static readonly DependencyProperty HaveUnsavedChangesProperty = DependencyProperty.Register(nameof(HaveUnsavedChanges), typeof(bool), typeof(BaseDbObjectViewModel));

		#endregion

		#region Commands

		protected virtual void InitCommands()
		{
			ChangesMade = new Command(OnChangesMadeExecute);
		}

		public Command ChangesMade { get; private set; }
		private void OnChangesMadeExecute()
		{
			HaveUnsavedChanges = true;
		}

		#endregion

		#region Methods

		public abstract IDbObject UpdateContext(IDbObject dbObject);
		public abstract void SetFromContext(IDbObject dbObject);
		public virtual bool CancelChanges()
		{
			if(context != null)
			{
				SetFromContext(null);
				SetFromContext(context);
				HaveUnsavedChanges = false;
				return true;
			}
			return false;
		}

		#endregion
	}
}
