using System.Windows;
using System.Windows.Controls;

namespace DocumentMaker.View.Controls
{
    /// <summary>
    /// Interaction logic for InputWithText.xaml
    /// </summary>
    public partial class InputWithText : UserControl
    {
        public static readonly DependencyProperty InputTextProperty;

        static InputWithText()
        {
            InputTextProperty = DependencyProperty.Register("InputText", typeof(string), typeof(InputWithText));
        }

        public InputWithText()
        {
            InitializeComponent();
            DataContext = this;
        }

        public string TextInfo { get; set; }

        public string InputText
        {
            get => (string)GetValue(InputTextProperty);
            set => SetValue(InputTextProperty, value);
        }
    }
}
