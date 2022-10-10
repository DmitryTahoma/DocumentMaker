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
					case ProjectNodeType.Project: return PackIconKind.AlphaPBoxOutline;
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
			if (value is PackIconKind packIconKind)
			{
				switch (packIconKind)
				{
					case PackIconKind.AlphaPBoxOutline: return ProjectNodeType.Project;
					case PackIconKind.AlphaEBoxOutline: return ProjectNodeType.Episode;
					case PackIconKind.AlphaBBoxOutline: return ProjectNodeType.Back;
					case PackIconKind.AlphaCBoxOutline: return ProjectNodeType.Craft;
					case PackIconKind.AlphaDBoxOutline: return ProjectNodeType.Dialog;
					case PackIconKind.AlphaHBoxOutline: return ProjectNodeType.Hog;
					case PackIconKind.AlphaMBoxOutline: return ProjectNodeType.Minigame;
					case PackIconKind.AlphaRBoxOutline: return ProjectNodeType.Regions;
				}
			}

			throw new ArgumentException();
		}
	}
}
