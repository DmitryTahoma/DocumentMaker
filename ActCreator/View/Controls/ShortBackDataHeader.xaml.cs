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
		public static readonly DependencyProperty IsOnlyOtherTypeProperty;

		static ShortBackDataHeader()
		{
			IsOnlyOtherTypeProperty = DependencyProperty.Register("IsOnlyOtherType", typeof(bool), typeof(ShortBackDataHeader));
		}

		public ShortBackDataHeader()
		{
			InitializeComponent();
			DataContext = this;
		}

		public bool IsOnlyOtherType
		{
			get => (bool)GetValue(IsOnlyOtherTypeProperty);
			set => SetValue(IsOnlyOtherTypeProperty, value);
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

			IsOnlyOtherType = templateType == DocumentTemplateType.Tester || templateType == DocumentTemplateType.Programmer || templateType == DocumentTemplateType.Soundman || templateType == DocumentTemplateType.Animator || templateType == DocumentTemplateType.Translator || templateType == DocumentTemplateType.Support;
		}
	}
}
