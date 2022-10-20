using Db.Context;
using Mvvm.Commands;
using System.Windows;

namespace ProjectEditorLib.ViewModel
{
	public abstract class BaseDbObjectViewModel : DependencyObject, IDbObjectViewModel
	{
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

		protected void InitCommands()
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

		#endregion
	}
}
