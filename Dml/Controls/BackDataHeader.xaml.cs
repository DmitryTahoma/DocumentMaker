using Dml.Model.Template;
using System.Windows;
using System.Windows.Controls;

namespace Dml.Controls
{
    /// <summary>
    /// Interaction logic for BackDataHeader.xaml
    /// </summary>
    public partial class BackDataHeader : UserControl
    {
        public BackDataHeader()
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
