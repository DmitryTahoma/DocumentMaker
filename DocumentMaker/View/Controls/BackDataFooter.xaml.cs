using DocumentMaker.Controller;
using DocumentMaker.Model.Template;
using System;
using System.Windows;
using System.Windows.Controls;

namespace DocumentMaker.View.Controls
{
    /// <summary>
    /// Interaction logic for BackDataFooter.xaml
    /// </summary>
    public partial class BackDataFooter : UserControl
    {
        public static readonly DependencyProperty DataProperty;
        public static readonly DependencyProperty AllTimeProperty;

        static BackDataFooter()
        {
            DataProperty = DependencyProperty.Register("Data", typeof(StackPanel), typeof(BackDataFooter));
            AllTimeProperty = DependencyProperty.Register("AllTime", typeof(string), typeof(BackDataFooter));
        }

        private event ActionWithBackData onAdded;
        private event ActionWithBackData onRemoved;
        private event Action onCleared;

        public BackDataFooter()
        {
            InitializeComponent();
            DataContext = this;
            AllTime = "0";
        }

        public StackPanel Data
        {
            get => GetValue(DataProperty) as StackPanel;
            set => SetValue(DataProperty, value);
        }

        public string AllTime
        {
            get => GetValue(AllTimeProperty).ToString();
            set => SetValue(AllTimeProperty, value);
        }

        public void SubscribeAddition(ActionWithBackData action)
        {
            onAdded += action;
        }

        public void SubscribeRemoving(ActionWithBackData action)
        {
            onRemoved += action;
        }

        public void SubscribeClearing(Action action)
        {
            onCleared += action;
        }

        public void AddLoadedBackData(BackDataController controller)
        {
            if (Data != null)
            {
                BackData backData = new BackData() { Controller = controller };
                AddBackData(backData);
                backData.SetDataFromController();
            }
        }

        private void AddBtnClick(object sender, RoutedEventArgs e)
        {
            if (Data != null)
            {
                BackData backData = new BackData { BackDataId = (uint)(Data.Children.Count + 1) };

                if (Data.Children.Count > 0 && Data.Children[Data.Children.Count - 1] is BackData lastData)
                {
                    backData.BackNumberText = lastData.BackNumberText;
                    backData.BackName = lastData.BackName;
                    backData.CountRegionsText = lastData.CountRegionsText;
                    backData.GameName = lastData.GameName;
                    backData.IsRework = lastData.IsRework;
                    backData.TimeText = lastData.TimeText;
                    backData.IsSketch = lastData.IsSketch;
                    backData.OtherText = lastData.OtherText;
                    backData.SetBackType(lastData.GetBackType());
                }

                AddBackData(backData);
                onAdded?.Invoke(backData);
                OnChangedSomeTime();
            }
        }

        private void DeleteBtnClick(object sender, RoutedEventArgs e)
		{
            if (Data != null &&
                MessageBox.Show("Ви впевнені, що хочете видалити всі пункти?",
                "Підтвердіть видалення",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question,
                MessageBoxResult.No)
                    == MessageBoxResult.Yes)
			{
                Data.Children.Clear();
                OnChangedSomeTime();
                onCleared?.Invoke();
            }
        }

        private void OnRemoveBackData(BackData sender)
        {
            if (Data != null)
            {
                Data.Children.Remove(sender);

                uint id = 1;
                foreach (FrameworkElement elem in Data.Children)
                {
                    if (elem is BackData backData)
                    {
                        backData.BackDataId = id;
                        id++;
                    }
                }
            }

            onRemoved?.Invoke(sender);
            OnChangedSomeTime();
        }

        private void OnChangedSomeTime()
        {
            uint time = 0;

            if (Data != null)
            {
                foreach (FrameworkElement elem in Data.Children)
                {
                    if (elem is BackData backData)
                    {
                        if (uint.TryParse(backData.TimeText, out uint backTime))
                        {
                            time += backTime;
                        }
                    }
                }
            }

            AllTime = time.ToString();
        }

        private void AddBackData(BackData backData)
        {
            backData.SubscribeDeletion(() =>
            {
                OnRemoveBackData(backData);
            });
            backData.SubscribeChangedTime(OnChangedSomeTime);
            Data.Children.Add(backData);
            backData.UpdateInputStates();
        }

        public void SetViewByTemplate(DocumentTemplateType templateType)
        {
            if (templateType == DocumentTemplateType.Painter)
            {
                IsSketchColumn.Width = new GridLength(1, GridUnitType.Star);
            }
            else
            {
                IsSketchColumn.Width = GridLength.Auto;
            }
        }
    }
}
