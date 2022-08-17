using Dml.Model.Template;
using System.Windows;
using System.Windows.Controls;

namespace DocumentMaker.View.Controls
{
	/// <summary>
	/// Interaction logic for FullBackDataHeader.xaml
	/// </summary>
	public partial class FullBackDataHeader : UserControl
	{
		public FullBackDataHeader()
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

		public void HideWorkTypeLabel()
		{
			if(ColWithWorkTypeComboBox != null)
			{
				ColWithWorkTypeComboBox.Width = GridLength.Auto;
			}
			if(WorkTypeLabel != null)
			{
				WorkTypeLabel.Visibility = Visibility.Collapsed;
			}
		}
	}
}
