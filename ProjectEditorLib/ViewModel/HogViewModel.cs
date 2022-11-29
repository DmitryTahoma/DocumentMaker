using Dml.Converters;
using ProjectsDb.Context;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace ProjectEditorLib.ViewModel
{
	public class HogViewModel : BaseDbObjectViewModel, IDbObjectViewModel
	{
		StringConverter stringConverter = new StringConverter();

		public HogViewModel() : base() { }

		#region Properties

		public string HogNumberText
		{
			get { return (string)GetValue(HogNumberTextProperty); }
			set { SetValue(HogNumberTextProperty, value); }
		}
		public static readonly DependencyProperty HogNumberTextProperty = DependencyProperty.Register(nameof(HogNumberText), typeof(string), typeof(HogViewModel));

		public string HogName
		{
			get { return (string)GetValue(HogNameProperty); }
			set { SetValue(HogNameProperty, value); }
		}
		public static readonly DependencyProperty HogNameProperty = DependencyProperty.Register(nameof(HogName), typeof(string), typeof(HogViewModel));

		public string CountRegionsText
		{
			get { return (string)GetValue(CountRegionsTextProperty); }
			set { SetValue(CountRegionsTextProperty, value); }
		}
		public static readonly DependencyProperty CountRegionsTextProperty = DependencyProperty.Register(nameof(CountRegionsText), typeof(string), typeof(HogViewModel));

		#endregion

		#region Methods

		public override void SetFromContext(IDbObject dbObject)
		{
			if(dbObject is Back hog)
			{
				HogNumberText = hog.Number.ToString();
				HogName = hog.Name;
				CountRegionsText = hog.Regions?.FirstOrDefault().Count.ToString();
				context = hog;
			}
			else
			{
				HogNumberText = string.Empty;
				HogName = string.Empty;
				CountRegionsText = string.Empty;
			}
		}

		public override IDbObject UpdateContext(IDbObject dbObject)
		{
			Back hog;

			if(dbObject == null)
			{
				hog = new Back();
			}
			else
			{
				hog = dbObject as Back;
			}

			if(hog != null)
			{
				hog.Name = HogName;
				hog.Number = stringConverter.ConvertToFloat(HogNumberText);
				HogNumberText = hog.Number.ToString();

				CountRegions regions = hog.Regions?.FirstOrDefault();
				if(regions == null)
				{
					regions = new CountRegions { BackId = hog.Id };
					if(hog.Regions == null)
					{
						hog.Regions = new List<CountRegions>();
					}
					hog.Regions.Add(regions);
				}

				regions.Count = int.Parse(CountRegionsText);

				dbObject = hog;
				context = hog;
			}

			return dbObject;
		}

		#endregion
	}
}
