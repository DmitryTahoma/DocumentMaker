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
        public static readonly DependencyProperty TimeTextProperty;

        static BackData()
        {
            IsRegionsProperty = DependencyProperty.Register("IsRegions", typeof(bool), typeof(BackData));
            BackDataIdProperty = DependencyProperty.Register("BackDataId", typeof(uint), typeof(BackData));
            TimeTextProperty = DependencyProperty.Register("TimeText", typeof(string), typeof(BackData));
        }

        private event Action onDeletion;
        private event Action onChangedTime;

        public BackData()
        {
            InitializeComponent();
            DataContext = this;
        }

        public uint BackDataId 
        {
            get => (uint)GetValue(BackDataIdProperty);
            set => SetValue(BackDataIdProperty, value);
        }

        public bool IsRegions
        {
            get => (bool)GetValue(IsRegionsProperty);
            set => SetValue(IsRegionsProperty, value);
        }

        public string TimeText
        {
            get => (string)GetValue(TimeTextProperty);
            set => SetValue(TimeTextProperty, value);
        }

        private void TypeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox)
            {
                IsRegions = comboBox.SelectedIndex == 1 || comboBox.SelectedIndex == 5;
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
