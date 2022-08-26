using ActCreator.Controller.Controls;
using Dml.Model.Template;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ActCreator.View.Controls
{
	/// <summary>
	/// Interaction logic for ShortBackDataFooter.xaml
	/// </summary>
	public partial class ShortBackDataFooter : UserControl
	{
		public static readonly DependencyProperty DataProperty;
		public static readonly DependencyProperty AllTimeProperty;

		static ShortBackDataFooter()
		{
			DataProperty = DependencyProperty.Register("Data", typeof(StackPanel), typeof(ShortBackDataFooter));
			AllTimeProperty = DependencyProperty.Register("AllTime", typeof(string), typeof(ShortBackDataFooter));
		}

		private event ActionWithShortBackData onAdded;
		private event ActionWithShortBackData onRemoved;
		private event Action onCleared;

		public ShortBackDataFooter()
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

		public void SubscribeAddition(ActionWithShortBackData action)
		{
			onAdded += action;
		}

		public void SubscribeRemoving(ActionWithShortBackData action)
		{
			onRemoved += action;
		}

		public void SubscribeClearing(Action action)
		{
			onCleared += action;
		}

		public void AddLoadedBackData(ShortBackDataController controller)
		{
			if (Data != null)
			{
				ShortBackData backData = new ShortBackData() { Controller = controller };
				AddBackData(backData);
				backData.SetDataFromController();
			}
		}

		private void AddBtnClick(object sender, RoutedEventArgs e)
		{
			if (Data != null)
			{
				ShortBackData backData = new ShortBackData { BackDataId = (uint)(Data.Children.Count + 1) };

				if (Data.Children.Count > 0 && Data.Children[Data.Children.Count - 1] is ShortBackData lastData)
				{
					backData.BackNumberText = lastData.BackNumberText;
					backData.BackName = lastData.BackName;
					backData.CountRegionsText = lastData.CountRegionsText;
					backData.GameName = lastData.GameName;
					backData.IsRework = lastData.IsRework;
					backData.TimeText = lastData.TimeText;
					backData.IsSketch = lastData.IsSketch;
					backData.OtherText = lastData.OtherText;
					backData.EpisodeNumberText = lastData.EpisodeNumberText;
					backData.SetBackType(lastData.GetBackType());
				}

				AddBackData(backData);
				onAdded?.Invoke(backData);
				OnChangedSomeTime(); 
				UpdateBackDataIds();
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
				ClearBackData();
			}
		}

		private void OnRemoveBackData(ShortBackData sender)
		{
			if (Data != null)
			{
				Data.Children.Remove(sender);

				uint id = 1;
				foreach (FrameworkElement elem in Data.Children)
				{
					if (elem is ShortBackData backData)
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
					if (elem is ShortBackData backData)
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

		private void AddBackData(ShortBackData backData)
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

		public void ClearBackData()
		{
			if (Data != null)
			{
				Data.Children.Clear();
				OnChangedSomeTime();
				onCleared?.Invoke();
			}
		}

		public void UpdateBackDataIds()
		{
			if (Data != null)
			{
				uint counter = 1;
				foreach (UIElement elem in Data.Children)
				{
					if (elem is ShortBackData backData)
					{
						backData.BackDataId = counter++;
						backData.SetDataFromController();
					}
				}
			}
		}
	}
}
