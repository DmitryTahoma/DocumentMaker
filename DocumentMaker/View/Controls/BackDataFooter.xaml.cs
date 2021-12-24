﻿using System.Windows;
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

        private void AddBtnClick(object sender, RoutedEventArgs e)
        {
            if (Data != null)
            {
                BackData backData = new BackData { BackDataId = (uint)(Data.Children.Count + 1) };
                backData.SubscribeDeletion(() =>
                {
                    OnRemoveBackData(backData);
                });
                backData.SubscribeChangedTime(OnChangedSomeTime);
                Data.Children.Add(backData);

                onAdded?.Invoke(backData);
            }
        }

        private void OnRemoveBackData(BackData sender)
        {
            if(Data != null)
            {
                Data.Children.Remove(sender);

                uint id = 1;
                foreach(FrameworkElement elem in Data.Children)
                {
                    if(elem is BackData backData)
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
                foreach(FrameworkElement elem in Data.Children)
                {
                    if (elem is BackData backData)
                    {
                        if(uint.TryParse(backData.TimeText, out uint backTime))
                        {
                            time += backTime;
                        }
                    }
                }
            }

            AllTime = time.ToString();
        }

        public void SubscribeAddition(ActionWithBackData action)
        {
            onAdded += action;
        }

        public void SubscribeRemoving(ActionWithBackData action)
        {
            onRemoved += action;
        }
    }
}
