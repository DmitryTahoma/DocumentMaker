using System.Windows;
using System.Windows.Controls;

namespace ActGenerator.View.Controls
{
	/// <summary>
	/// Interaction logic for DocumentListItemControl.xaml
	/// </summary>
	public partial class DocumentListItemControl : UserControl
	{
		public DocumentListItemControl()
		{
			InitializeComponent();
		}

		#region Properties

		public string FullName
		{
			get { return (string)GetValue(FullNameProperty); }
			set { SetValue(FullNameProperty, value); }
		}
		public static readonly DependencyProperty FullNameProperty = DependencyProperty.Register(nameof(FullName), typeof(string), typeof(DocumentListItemControl), new PropertyMetadata(null));

		public string FileName
		{
			get { return (string)GetValue(FileNameProperty); }
			set { SetValue(FileNameProperty, value); }
		}
		public static readonly DependencyProperty FileNameProperty = DependencyProperty.Register(nameof(FileName), typeof(string), typeof(DocumentListItemControl), new PropertyMetadata(null));

		#endregion
	}
}
