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

namespace DocumentMakerThemes.View.Controls
{
	/// <summary>
	/// Interaction logic for WaitingControl.xaml
	/// </summary>
	public partial class WaitingControl : UserControl
	{
		public WaitingControl()
		{
			InitializeComponent();
		}

		#region Properties

		public string Text
		{
			get { return (string)GetValue(TextProperty); }
			set { SetValue(TextProperty, value); }
		}
		public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(WaitingControl));

		#endregion
	}
}
