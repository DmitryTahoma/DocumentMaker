using Db.Context;
using Db.Context.BackPart;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace ProjectEditorLib.ViewModel
{
	public class HogViewModel : DependencyObject, IDbObjectViewModel
	{
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

		public bool HaveUnsavedChanges
		{
			get { return (bool)GetValue(HaveUnsavedChangesProperty); }
			set { SetValue(HaveUnsavedChangesProperty, value); }
		}
		public static readonly DependencyProperty HaveUnsavedChangesProperty = DependencyProperty.Register(nameof(HaveUnsavedChanges), typeof(bool), typeof(HogViewModel));

		#endregion

		#region Methods

		public void SetFromContext(IDbObject dbObject)
		{
			if(dbObject is Back hog)
			{
				HogNumberText = hog.Number.ToString();
				HogName = hog.Name;
				CountRegionsText = hog.Regions?.FirstOrDefault().Count.ToString();
			}
			else
			{
				HogNumberText = string.Empty;
				HogName = string.Empty;
				CountRegionsText = string.Empty;
			}
		}

		public IDbObject UpdateContext(IDbObject dbObject)
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
				hog.Number = float.Parse(HogNumberText);

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
			}

			return dbObject;
		}

		#endregion
	}
}
