﻿using DocumentMaker.Controller;
using DocumentMaker.View.Controls;
using System.Windows;

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
    }
}
