using ProjectsDb.Context;
using System.Windows;

namespace ProjectEditorLib.ViewModel
{
	public class CraftViewModel : BaseDbObjectViewModel, IDbObjectViewModel
	{
		public CraftViewModel() : base() { }

		#region Properties

		public string CraftName
		{
			get { return (string)GetValue(CraftNameProperty); }
			set { SetValue(CraftNameProperty, value); }
		}
		public static readonly DependencyProperty CraftNameProperty = DependencyProperty.Register(nameof(CraftName), typeof(string), typeof(CraftViewModel));

		#endregion

		#region Methods

		public override IDbObject UpdateContext(IDbObject dbObject)
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
				context = back;
			}

			return dbObject;
		}

		public override void SetFromContext(IDbObject dbObject)
		{
			if (dbObject is Back back)
			{
				CraftName = back.Name;
				context = back;
			}
			else
			{
				CraftName = string.Empty;
				context = null;
			}
		}

		#endregion
	}
}
