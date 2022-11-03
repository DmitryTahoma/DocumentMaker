using ProjectEditorLib.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace ProjectEditorLib.Converters
{
	[ValueConversion(typeof(ProjectNodeType), typeof(string))]
	public class ProjectNodeTypeToStringConverter : IValueConverter
	{
		IDictionary<ProjectNodeType, string> map = new Dictionary<ProjectNodeType, string>
		{
			{ ProjectNodeType.Project, "Проєкт" },
			{ ProjectNodeType.Episode, "Епізод" },
			{ ProjectNodeType.Back, "Бек" },
			{ ProjectNodeType.Craft, "Крафт" },
			{ ProjectNodeType.Dialog, "Діалог" },
			{ ProjectNodeType.Hog, "Хог" },
			{ ProjectNodeType.Minigame, "Міні-гра" },
			{ ProjectNodeType.Regions, "Регіони" },
		};

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(value is ProjectNodeType projectNodeType && map.TryGetValue(projectNodeType, out string res))
			{
				return res;
			}

			throw new ArgumentException();
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(value is string str && map.FirstOrDefault(x => x.Value == str) is KeyValuePair<ProjectNodeType, string> pair)
			{
				return pair.Key;
			}

			throw new ArgumentException();
		}
	}
}
