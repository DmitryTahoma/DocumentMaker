using ProjectsDb.Context;
using System.Windows;

namespace ProjectEditorLib.ViewModel
{
	public class DialogViewModel : BaseDbObjectViewModel, IDbObjectViewModel
	{
		public DialogViewModel() : base() { }

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

		#endregion

		#region Methods

		public override void SetFromContext(IDbObject dbObject)
		{
			if(dbObject is Back dialog)
			{
				DialogNumberText = dialog.Number.ToString();
				DialogName = dialog.Name;
				context = dialog;
			}
			else
			{
				DialogNumberText = string.Empty;
				DialogName = string.Empty;
			}
		}

		public override IDbObject UpdateContext(IDbObject dbObject)
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
				context = dialog;
			}

			return dbObject;
		}

		#endregion
	}
}
