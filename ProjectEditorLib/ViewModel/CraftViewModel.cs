using Db.Context;
using Db.Context.BackPart;
using System.Windows;

namespace ProjectEditorLib.ViewModel
{
	public class CraftViewModel : DependencyObject, IDbObjectViewModel
	{
		#region Properties

		public string CraftName
		{
			get { return (string)GetValue(CraftNameProperty); }
			set { SetValue(CraftNameProperty, value); }
		}
		public static readonly DependencyProperty CraftNameProperty = DependencyProperty.Register(nameof(CraftName), typeof(string), typeof(CraftViewModel));

		public bool HaveUnsavedChanges
		{
			get { return (bool)GetValue(HaveUnsavedChangesProperty); }
			set { SetValue(HaveUnsavedChangesProperty, value); }
		}
		public static readonly DependencyProperty HaveUnsavedChangesProperty = DependencyProperty.Register(nameof(HaveUnsavedChanges), typeof(bool), typeof(CraftViewModel));

		#endregion

		#region Methods

		public IDbObject UpdateContext(IDbObject dbObject)
		{
			Back back;

			if(dbObject == null)
			{
				back = new Back();
			}
			else
			{
				back = dbObject as Back;
			}

			if(back != null)
			{
				back.Name = CraftName;

				dbObject = back;
			}

			return dbObject;
		}

		public void SetFromContext(IDbObject dbObject)
		{
			if (dbObject is Back back)
			{
				CraftName = back.Name;
			}
			else
			{
				CraftName = string.Empty;
			}
		}

		#endregion
	}
}
