using Db.Context;
using Db.Context.BackPart;
using System.Windows;

namespace ProjectEditorLib.ViewModel
{
	public class DialogViewModel : DependencyObject, IDbObjectViewModel
	{
		#region Properties

		public string DialogNumberText
		{
			get { return (string)GetValue(DialogNumberTextProperty); }
			set { SetValue(DialogNumberTextProperty, value); }
		}
		public static readonly DependencyProperty DialogNumberTextProperty = DependencyProperty.Register(nameof(DialogNumberText), typeof(string), typeof(DialogViewModel));

		public string DialogName
		{
			get { return (string)GetValue(DialogNameProperty); }
			set { SetValue(DialogNameProperty, value); }
		}
		public static readonly DependencyProperty DialogNameProperty = DependencyProperty.Register(nameof(DialogName), typeof(string), typeof(DialogViewModel));

		public bool HaveUnsavedChanges
		{
			get { return (bool)GetValue(HaveUnsavedChangesProperty); }
			set { SetValue(HaveUnsavedChangesProperty, value); }
		}
		public static readonly DependencyProperty HaveUnsavedChangesProperty = DependencyProperty.Register(nameof(HaveUnsavedChanges), typeof(bool), typeof(DialogViewModel));

		#endregion

		#region Methods

		public void SetFromContext(IDbObject dbObject)
		{
			if(dbObject is Back dialog)
			{
				DialogNumberText = dialog.Number.ToString();
				DialogName = dialog.Name;
			}
			else
			{
				DialogNumberText = string.Empty;
				DialogName = string.Empty;
			}
		}

		public IDbObject UpdateContext(IDbObject dbObject)
		{
			Back dialog;

			if(dbObject == null)
			{
				dialog = new Back();
			}
			else
			{
				dialog = dbObject as Back;
			}

			if(dialog != null)
			{
				dialog.Name = DialogName;
				dialog.Number = float.Parse(DialogNumberText);
				dbObject = dialog;
			}

			return dbObject;
		}

		#endregion
	}
}
