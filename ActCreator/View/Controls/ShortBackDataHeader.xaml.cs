using Dml.Model.Template;
using System.Windows;
using System.Windows.Controls;

namespace ActCreator.View.Controls
{
	/// <summary>
	/// Interaction logic for ShortBackDataHeader.xaml
	/// </summary>
	public partial class ShortBackDataHeader : UserControl
	{
		public ShortBackDataHeader()
		{
			InitializeComponent();
		}

		public void SetViewByTemplate(DocumentTemplateType templateType)
		{
			if (templateType == DocumentTemplateType.Painter)
			{
				IsSketchTextBlock.Visibility = Visibility.Visible;
				IsSketchColumn.Width = new GridLength(1, GridUnitType.Star);
			}
			else
			{
				IsSketchTextBlock.Visibility = Visibility.Collapsed;
				IsSketchColumn.Width = GridLength.Auto;
			}
		}
	}
}
