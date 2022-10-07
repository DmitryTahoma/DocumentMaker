using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ProjectEditorLib.View
{
	/// <summary>
	/// Interaction logic for TreeItemHeader.xaml
	/// </summary>
	public partial class TreeItemHeader : UserControl
	{
		public TreeItemHeader()
		{
			InitializeComponent();
		}

		public PackIconKind IconKind
		{
			get { return (PackIconKind)GetValue(IconKindProperty); }
			set { SetValue(IconKindProperty, value); }
		}
		public static readonly DependencyProperty IconKindProperty = DependencyProperty.Register(nameof(IconKind), typeof(PackIconKind), typeof(TreeItemHeader));

		public string Text
		{
			get { return (string)GetValue(TextProperty); }
			set { SetValue(TextProperty, value); }
		}
		public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(TreeItemHeader));
	}
}
