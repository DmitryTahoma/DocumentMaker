using DocumentMaker.Controller;
using DocumentMaker.View.Controls;
using System.Diagnostics;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.Forms.MessageBox;

namespace DocumentMaker
{
    public delegate void ActionWithBackData(BackData backData);

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static readonly DependencyProperty TechnicalTaskDateTextProperty;
        public static readonly DependencyProperty ActDateTextProperty;
        public static readonly DependencyProperty AdditionNumTextProperty;
        public static readonly DependencyProperty FullHumanNameProperty;
        public static readonly DependencyProperty HumanIdTextProperty;
        public static readonly DependencyProperty AddressTextProperty;
        public static readonly DependencyProperty PaymentAccountTextProperty;
        public static readonly DependencyProperty BankNameProperty;
        public static readonly DependencyProperty MfoTextProperty;
        public static readonly DependencyProperty ContractNumberTextProperty;
        public static readonly DependencyProperty ContractDateTextProperty;

        static MainWindow()
        {
            TechnicalTaskDateTextProperty = DependencyProperty.Register("TechnicalTaskDateText", typeof(string), typeof(InputWithText));
            ActDateTextProperty = DependencyProperty.Register("ActDateText", typeof(string), typeof(InputWithText));
            AdditionNumTextProperty = DependencyProperty.Register("AdditionNumText", typeof(string), typeof(InputWithText));
            FullHumanNameProperty = DependencyProperty.Register("FullHumanName", typeof(string), typeof(InputWithText));
            HumanIdTextProperty = DependencyProperty.Register("HumanIdText", typeof(string), typeof(InputWithText));
            AddressTextProperty = DependencyProperty.Register("AddressText", typeof(string), typeof(InputWithText));
            PaymentAccountTextProperty = DependencyProperty.Register("PaymentAccountText", typeof(string), typeof(InputWithText));
            BankNameProperty = DependencyProperty.Register("BankName", typeof(string), typeof(InputWithText));
            MfoTextProperty = DependencyProperty.Register("MfoText", typeof(string), typeof(InputWithText));
            ContractNumberTextProperty = DependencyProperty.Register("ContractNumberText", typeof(string), typeof(InputWithText));
            ContractDateTextProperty = DependencyProperty.Register("ContractDateText", typeof(string), typeof(InputWithText));
        }

        private MainWindowController controller; 
        private FolderBrowserDialog folderBrowserDialog;

        public MainWindow()
        {
            InitializeComponent();

            controller = new MainWindowController();
            DataFooter.SubscribeAddition((x) =>
            {
                controller.BackDataControllers.Add(x.Controller);
            });
            DataFooter.SubscribeRemoving((x) =>
            {
                controller.BackDataControllers.Remove(x.Controller);
            });

            folderBrowserDialog = new FolderBrowserDialog();
        }

        public string TechnicalTaskDateText
        {
            get => (string)GetValue(TechnicalTaskDateTextProperty);
            set
            {
                SetValue(TechnicalTaskDateTextProperty, value);
                controller.TechnicalTaskDateText = value;
            }
        }

        public string ActDateText
        {
            get => (string)GetValue(ActDateTextProperty);
            set
            {
                SetValue(ActDateTextProperty, value);
                controller.ActDateText = value;
            }
        }

        public string AdditionNumText
        {
            get => (string)GetValue(AdditionNumTextProperty);
            set
            {
                SetValue(AdditionNumTextProperty, value);
                controller.AdditionNumText = value;
            }
        }

        public string FullHumanName
        {
            get => (string)GetValue(FullHumanNameProperty);
            set
            {
                SetValue(FullHumanNameProperty, value);
                controller.FullHumanName = value;
            }
        }

        public string HumanIdText
        {
            get => (string)GetValue(HumanIdTextProperty);
            set
            {
                SetValue(HumanIdTextProperty, value);
                controller.HumanIdText = value;
            }
        }

        public string AddressText
        {
            get => (string)GetValue(AddressTextProperty);
            set
            {
                SetValue(AddressTextProperty, value);
                controller.AddressText = value;
            }
        }

        public string PaymentAccountText
        {
            get => (string)GetValue(PaymentAccountTextProperty);
            set
            {
                SetValue(PaymentAccountTextProperty, value);
                controller.PaymentAccountText = value;
            }
        }

        public string BankName
        {
            get => (string)GetValue(BankNameProperty);
            set
            {
                SetValue(BankNameProperty, value);
                controller.BankName = value;
            }
        }

        public string MfoText
        {
            get => (string)GetValue(MfoTextProperty);
            set
            {
                SetValue(MfoTextProperty, value);
                controller.MfoText = value;
            }
        }

        public string ContractNumberText
        {
            get => (string)GetValue(ContractNumberTextProperty);
            set
            {
                SetValue(ContractNumberTextProperty, value);
                controller.ContractNumberText = value;
            }
        }

        public string ContractDateText
        {
            get => (string)GetValue(ContractDateTextProperty);
            set
            {
                SetValue(ContractDateTextProperty, value);
                controller.ContractDateText = value;
            }
        }

        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            controller.Save();
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            if (controller != null)
            {
                controller.Load();

                TechnicalTaskDateTextInput.InputText = controller.TechnicalTaskDateText;
                ActDateTextInput.InputText = controller.ActDateText;
                AdditionNumTextInput.InputText = controller.AdditionNumText;
                FullHumanNameInput.InputText = controller.FullHumanName;
                HumanIdTextInput.InputText = controller.HumanIdText;
                AddressTextInput.InputText = controller.AddressText;
                PaymentAccountTextInput.InputText = controller.PaymentAccountText;
                BankNameInput.InputText = controller.BankName;
                MfoTextInput.InputText = controller.MfoText;
                ContractNumberTextInput.InputText = controller.ContractNumberText;
                ContractDateTextInput.InputText = controller.ContractDateText;

                foreach (BackDataController backDataController in controller.BackDataControllers)
                {
                    DataFooter.AddLoadedBackData(backDataController);
                }
            }
        }

        private void ExportBtnClick(object sender, RoutedEventArgs e)
        {
            if (controller.Validate(out string errorText))
            {
                if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    bool isShowResult = true;
                    controller.Export(folderBrowserDialog.SelectedPath);

                    if (controller.HasNoMovedFiles)
                    {
                        if (MessageBox.Show("Файли за заданними путями вже існують.\n\n" + controller.GetInfoNoMovedFiles() + "\nЗамінити?",
                                            "DocumentMaker | Export",
                                            MessageBoxButtons.YesNo,
                                            MessageBoxIcon.Question)
                                                == System.Windows.Forms.DialogResult.Yes)
                        {
                            controller.ReplaceCreatedFiles();

                            if (controller.HasNoMovedFiles)
                            {
                                MessageBox.Show("Не вдалось перемістити наступні файли. Можливо вони відкриті в іншій програмі.\n\n" + controller.GetInfoNoMovedFiles(),
                                                "DocumentMaker | Export",
                                                MessageBoxButtons.OK,
                                                MessageBoxIcon.Warning);

                                controller.RemoveTemplates();
                                isShowResult = false;
                            }
                        }
                        else
                        {
                            controller.RemoveTemplates();
                        }
                    }

                    if (isShowResult && MessageBox.Show("Файли збережені.\nВідкрити папку з файлами?",
                                        "DocumentMaker | Export",
                                        MessageBoxButtons.YesNo,
                                        MessageBoxIcon.Information,
                                        MessageBoxDefaultButton.Button2)
                        == System.Windows.Forms.DialogResult.Yes)
                    {
                        Process.Start("explorer", folderBrowserDialog.SelectedPath);
                    }
                }
            }
            else
            {
                MessageBox.Show(errorText, "DocumentMaker | Validation", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
