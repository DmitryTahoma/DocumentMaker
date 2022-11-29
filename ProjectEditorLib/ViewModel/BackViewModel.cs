using Dml.Converters;
using ProjectsDb.Context;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace ProjectEditorLib.ViewModel
{
	public class BackViewModel : BaseDbObjectViewModel, IDbObjectViewModel
	{
		StringConverter stringConverter = new StringConverter();

		public BackViewModel() : base() { }

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

		public override void SetFromContext(IDbObject dbObject)
		{
			if(dbObject is Back back)
			{
				BackNumberText = back.Number.ToString();
				BackName = back.Name;
				CountRegionsText = back.Regions?.FirstOrDefault().Count.ToString();
				context = back;
			}
			else
			{
				BackNumberText = string.Empty;
				BackName = string.Empty;
				CountRegionsText = string.Empty;
			}
		}

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

			if (back != null)
			{
				back.Name = BackName;
				back.Number = stringConverter.ConvertToFloat(BackNumberText);
				BackNumberText = back.Number.ToString();

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
				context = back;
			}

			return dbObject;
		}

		#endregion
	}
}
