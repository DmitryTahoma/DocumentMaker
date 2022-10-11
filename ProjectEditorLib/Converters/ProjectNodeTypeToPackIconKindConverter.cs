using MaterialDesignThemes.Wpf;
using ProjectEditorLib.Model;
using System;
using System.Globalization;
using System.Windows.Data;

namespace ProjectEditorLib.Converters
{
	[ValueConversion(typeof(ProjectNodeType), typeof(PackIconKind))]
	public class ProjectNodeTypeToPackIconKindConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is ProjectNodeType projectNodeType)
			{
				switch (projectNodeType)
				{
					case ProjectNodeType.Project:
					case ProjectNodeType.ProjectWithoutEpisodes: return PackIconKind.AlphaPBoxOutline;
					case ProjectNodeType.Episode: return PackIconKind.AlphaEBoxOutline;
					case ProjectNodeType.Back: return PackIconKind.AlphaBBoxOutline;
					case ProjectNodeType.Craft: return PackIconKind.AlphaCBoxOutline;
					case ProjectNodeType.Dialog: return PackIconKind.AlphaDBoxOutline;
					case ProjectNodeType.Hog: return PackIconKind.AlphaHBoxOutline;
					case ProjectNodeType.Minigame: return PackIconKind.AlphaMBoxOutline;
					case ProjectNodeType.Regions: return PackIconKind.AlphaRBoxOutline;
				}
			}

			throw new ArgumentException();
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return Binding.DoNothing;
		}
	}
}
