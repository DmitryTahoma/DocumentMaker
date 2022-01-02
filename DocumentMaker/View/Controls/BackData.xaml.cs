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
        public static readonly DependencyProperty HasBackNumberProperty;
        public static readonly DependencyProperty BackDataIdProperty;
        public static readonly DependencyProperty BackNumberTextProperty;
        public static readonly DependencyProperty BackNameProperty;
        public static readonly DependencyProperty CountRegionsTextProperty;
        public static readonly DependencyProperty GameNameProperty;
        public static readonly DependencyProperty IsReworkProperty;
        public static readonly DependencyProperty TimeTextProperty;

        static BackData()
        {
            IsRegionsProperty = DependencyProperty.Register("IsRegions", typeof(bool), typeof(BackData));
            HasBackNumberProperty = DependencyProperty.Register("HasBackNumber", typeof(bool), typeof(BackData));
            BackDataIdProperty = DependencyProperty.Register("BackDataId", typeof(uint), typeof(BackData));
            BackNumberTextProperty = DependencyProperty.Register("BackNumberText", typeof(string), typeof(BackDataController));
            BackNameProperty = DependencyProperty.Register("BackName", typeof(string), typeof(BackDataController));
            CountRegionsTextProperty = DependencyProperty.Register("CountRegionsText", typeof(string), typeof(BackDataController));
            GameNameProperty = DependencyProperty.Register("GameName", typeof(string), typeof(BackDataController));
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

        public string GameName
        {
            get => (string)GetValue(GameNameProperty);
            set
            {
                SetValue(GameNameProperty, value);
                controller.GameName = value;
            }
        }

        public bool IsRegions
        {
            get => (bool)GetValue(IsRegionsProperty);
            set => SetValue(IsRegionsProperty, value);
        }

        public bool HasBackNumber
        {
            get => (bool)GetValue(HasBackNumberProperty);
            set => SetValue(HasBackNumberProperty, value);
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

        public BackDataController Controller
        {
            get => controller;
            set
            {
                if (value != null)
                {
                    controller = value;
                }
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

        public void SetDataFromController()
        {
            BackDataIdLabel.Text = controller.Id.ToString();
            BackTypeComboBox.SelectedIndex = (int)controller.Type;
            BackNumberTextInput.Text = controller.BackNumberText;
            BackNameInput.Text = controller.BackName;
            CountRegionsTextInput.Text = controller.BackCountRegionsText;
            GameNameInput.Text = controller.GameName;
            IsReworkCheckBox.IsChecked = controller.IsRework;
            TimeTextInput.Text = controller.SpentTimeText;

            UpdateInputStates();
        }

        private void TypeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (controller != null && sender is ComboBox comboBox)
            {
                controller.Type = (BackType)comboBox.SelectedIndex;
                UpdateInputStates();
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

        public void UpdateInputStates()
        {
            IsRegions = controller.Type == BackType.Regions || controller.Type == BackType.HogRegions;
            if (!IsRegions)
            {
                CountRegionsText = "";
                CountRegionsTextInput.Text = controller.BackCountRegionsText;
            }

            HasBackNumber = controller.Type != BackType.Craft;
            if (!HasBackNumber)
            {
                BackNumberText = "";
                BackNumberTextInput.Text = controller.BackNumberText;
            }
        }
    }
}
