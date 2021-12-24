using DocumentMaker.Controller;
using DocumentMaker.Model;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DocumentMaker.View.Controls
{
    /// <summary>
    /// Interaction logic for BackData.xaml
    /// </summary>
    public partial class BackData : UserControl
    {
        public static readonly DependencyProperty IsRegionsProperty;
        public static readonly DependencyProperty BackDataIdProperty;
        public static readonly DependencyProperty BackNumberTextProperty;
        public static readonly DependencyProperty BackNameProperty;
        public static readonly DependencyProperty CountRegionsTextProperty;
        public static readonly DependencyProperty IsReworkProperty;
        public static readonly DependencyProperty TimeTextProperty;

        static BackData()
        {
            IsRegionsProperty = DependencyProperty.Register("IsRegions", typeof(bool), typeof(BackData));
            BackDataIdProperty = DependencyProperty.Register("BackDataId", typeof(uint), typeof(BackData));
            BackNumberTextProperty = DependencyProperty.Register("BackNumberText", typeof(string), typeof(BackDataController));
            BackNameProperty = DependencyProperty.Register("BackName", typeof(string), typeof(BackDataController));
            CountRegionsTextProperty = DependencyProperty.Register("CountRegionsText", typeof(string), typeof(BackDataController));
            IsReworkProperty = DependencyProperty.Register("IsRework", typeof(bool), typeof(BackDataController));
            TimeTextProperty = DependencyProperty.Register("TimeText", typeof(string), typeof(BackDataController));
        }

        private BackDataController controller;

        private event Action onDeletion;
        private event Action onChangedTime;

        public BackData()
        {
            InitializeComponent();
            DataContext = this;

            controller = new BackDataController();
        }

        public uint BackDataId
        {
            get => (uint)GetValue(BackDataIdProperty);
            set
            {
                SetValue(BackDataIdProperty, value);
                controller.Id = value;
            }
        }

        public string BackNumberText
        {
            get => (string)GetValue(BackNumberTextProperty);
            set
            {
                SetValue(BackNumberTextProperty, value);
                controller.BackNumberText = value;
            }
        }

        public string BackName
        {
            get => (string)GetValue(BackNameProperty);
            set
            {
                SetValue(BackNameProperty, value);
                controller.BackName = value;
            }
        }

        public string CountRegionsText
        {
            get => (string)GetValue(CountRegionsTextProperty);
            set
            {
                SetValue(CountRegionsTextProperty, value);
                controller.BackCountRegionsText = value;
            }
        }

        public bool IsRegions
        {
            get => (bool)GetValue(IsRegionsProperty);
            set
            {
                if (!value)
                {
                    CountRegionsText = "";
                }
                SetValue(IsRegionsProperty, value);
            }
        }

        public bool IsRework
        {
            get => (bool)GetValue(IsReworkProperty);
            set
            {
                SetValue(IsReworkProperty, value);
                controller.IsRework = value;
            }
        }

        public string TimeText
        {
            get => (string)GetValue(TimeTextProperty);
            set
            {
                SetValue(TimeTextProperty, value);
                controller.SpentTimeText = value;
            }
        }

        public BackDataController Controller { get => controller; }

        private void TypeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (controller != null && sender is ComboBox comboBox)
            {
                controller.Type = (BackType)comboBox.SelectedIndex;
                IsRegions = controller.Type == BackType.Regions || controller.Type == BackType.HogRegions;
            }
        }

        private void MouseEnterTextBlockNum(object sender, MouseEventArgs e)
        {
            if (sender is TextBlock textBlock)
            {
                textBlock.Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 0));
            }
        }

        private void MouseLeaveTextBlockNum(object sender, MouseEventArgs e)
        {
            if (sender is TextBlock textBlock)
            {
                textBlock.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            }
        }

        private void MouseDownTextBlockNum(object sender, MouseButtonEventArgs e)
        {
            if (MessageBox.Show("Ви впевнені, що хочете видалити пункт №" + BackDataId.ToString(),
                "Підтвердіть видалення",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question,
                MessageBoxResult.No)
                    == MessageBoxResult.Yes)
            {
                onDeletion?.Invoke();
            }
        }

        private void TextChangedTextBoxTime(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox)
            {
                onChangedTime?.Invoke();
            }
        }

        public void SubscribeDeletion(Action action)
        {
            onDeletion += action;
        }

        public void SubscribeChangedTime(Action action)
        {
            onChangedTime += action;
        }

    }
}
