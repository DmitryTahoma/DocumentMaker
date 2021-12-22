using System.Windows.Controls;

namespace DocumentMaker.View.Controls
{
    /// <summary>
    /// Interaction logic for InputWithText.xaml
    /// </summary>
    public partial class InputWithText : UserControl
    {
        public InputWithText()
        {
            InitializeComponent();
            DataContext = this;
        }

        public string TextInfo { get; set; }

        public string InputText { get; set; }
    }
}
