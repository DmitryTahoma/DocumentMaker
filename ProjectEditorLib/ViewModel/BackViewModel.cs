using Db.Context;
using Db.Context.BackPart;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace ProjectEditorLib.ViewModel
{
	public class BackViewModel : DependencyObject, IDbObjectViewModel
	{
		#region Properties

		public string BackNumberText
		{
			get { return (string)GetValue(BackNumberTextProperty); }
			set { SetValue(BackNumberTextProperty, value); }
		}
		public static readonly DependencyProperty BackNumberTextProperty = DependencyProperty.Register(nameof(BackNumberText), typeof(string), typeof(BackViewModel));

		public string BackName
		{
			get { return (string)GetValue(BackNameProperty); }
			set { SetValue(BackNameProperty, value); }
		}
		public static readonly DependencyProperty BackNameProperty = DependencyProperty.Register(nameof(BackName), typeof(string), typeof(BackViewModel));

		public string CountRegionsText
		{
			get { return (string)GetValue(CountRegionsTextProperty); }
			set { SetValue(CountRegionsTextProperty, value); }
		}
		public static readonly DependencyProperty CountRegionsTextProperty = DependencyProperty.Register(nameof(CountRegionsText), typeof(string), typeof(BackViewModel));

		#endregion

		#region Methods

		public void SetFromContext(IDbObject dbObject)
		{
			if(dbObject is Back back)
			{
				BackNumberText = back.Number.ToString();
				BackName = back.Name;
				CountRegionsText = back.Regions.FirstOrDefault().Count.ToString();
			}
		}

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

			if (back != null)
			{
				back.Name = BackName;
				back.Number = int.Parse(BackNumberText);

				CountRegions regions = back.Regions?.FirstOrDefault();
				if(regions == null)
				{
					regions = new CountRegions { BackId = back.Id };
					if (back.Regions == null)
					{
						back.Regions = new List<CountRegions>();
					}
					back.Regions.Add(regions);
				}
				
				regions.Count = int.Parse(CountRegionsText);

				dbObject = back;
			}

			return dbObject;
		}

		#endregion
	}
}
