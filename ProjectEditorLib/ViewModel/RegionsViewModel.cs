using Db.Context;
using Db.Context.BackPart;
using System.Windows;

namespace ProjectEditorLib.ViewModel
{
	class RegionsViewModel : BaseDbObjectViewModel, IDbObjectViewModel
	{
		public RegionsViewModel() : base() { }

		#region Properties

		public string CountText
		{
			get { return (string)GetValue(CountTextProperty); }
			set { SetValue(CountTextProperty, value); }
		}
		public static readonly DependencyProperty CountTextProperty = DependencyProperty.Register(nameof(CountText), typeof(string), typeof(RegionsViewModel));

		#endregion

		#region Methods

		public override void SetFromContext(IDbObject dbObject)
		{
			if(dbObject is CountRegions regions)
			{
				CountText = regions.Count.ToString();
				context = regions;
			}
			else
			{
				CountText = string.Empty;
			}
		}

		public override IDbObject UpdateContext(IDbObject dbObject)
		{
			CountRegions regions;

			if(dbObject == null)
			{
				regions = new CountRegions();
			}
			else
			{
				regions = dbObject as CountRegions;
			}

			if(regions != null)
			{
				regions.Count = int.Parse(CountText);
				dbObject = regions;
				context = regions;
			}

			return dbObject;
		}

		#endregion
	}
}
