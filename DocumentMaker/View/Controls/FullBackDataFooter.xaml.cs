using Dml.Model.Template;
using DocumentMaker.Controller.Controls;
using System;
using System.Windows;
using System.Windows.Controls;

namespace DocumentMaker.View.Controls
{
	public delegate void ActionWithBool(bool b);

	/// <summary>
	/// Interaction logic for FullBackDataFooter.xaml
	/// </summary>
	public partial class FullBackDataFooter : UserControl
	{
		public static readonly DependencyProperty DataProperty;
		public static readonly DependencyProperty AllSumProperty;
		public static readonly DependencyProperty WeightAllTextProperty;

		static FullBackDataFooter()
		{
			DataProperty = DependencyProperty.Register("Data", typeof(StackPanel), typeof(FullBackDataFooter));
			AllSumProperty = DependencyProperty.Register("AllSum", typeof(string), typeof(FullBackDataFooter));
			WeightAllTextProperty = DependencyProperty.Register("WeightAllText", typeof(string), typeof(FullBackDataFooter));
		}

		private readonly FullBackDataFooterController controller;

		private event ActionWithFullBackData onAdded;
		private event Action onChangedSum;

		public FullBackDataFooter()
		{
			controller = new FullBackDataFooterController();
			InitializeComponent();
			DataContext = this;
		}

		public StackPanel Data
		{
			get => GetValue(DataProperty) as StackPanel;
			set => SetValue(DataProperty, value);
		}

		public string AllSum
		{
			get => (string)GetValue(AllSumProperty);
			set => SetValue(AllSumProperty, value);
		}

		public string WeightAllText
		{
			get => GetValue(WeightAllTextProperty).ToString();
			set => SetValue(WeightAllTextProperty, value);
		}

		public void SubscribeAddition(ActionWithFullBackData action)
		{
			onAdded += action;
		}

		public void SubscribeChangingSum(Action action)
		{
			onChangedSum += action;
		}

		public FullBackData AddLoadedBackData(FullBackDataController controller)
		{
			if (Data != null)
			{
				FullBackData backData = new FullBackData() { Controller = controller };
				AddBackData(backData);
				backData.SetDataFromController();
				return backData;
			}
			return null;
		}

		private void AddBtnClick(object sender, RoutedEventArgs e)
		{
			if (Data != null)
			{
				FullBackData backData = new FullBackData { BackDataId = (uint)(Data.Children.Count + 1) };

				if (Data.Children.Count > 0 && Data.Children[Data.Children.Count - 1] is FullBackData lastData)
				{
					backData.BackNumberText = lastData.BackNumberText;
					backData.BackName = lastData.BackName;
					backData.CountRegionsText = lastData.CountRegionsText;
					backData.GameName = lastData.GameName;
					backData.IsRework = lastData.IsRework;
					backData.IsSketch = lastData.IsSketch;
					backData.OtherText = lastData.OtherText;
					backData.SetBackType(lastData.GetBackType());
				}

				AddBackData(backData);
				onAdded?.Invoke(backData);
				OnChangedSomeSum();
			}
		}

		private void OnChangedSomeSum()
		{
			if (Data != null)
			{
				uint sums = 0;

				foreach (UIElement elem in Data.Children)
				{
					if (elem is FullBackData backData && uint.TryParse(backData.SumText, out uint sum))
					{
						sums += sum;
					}
				}

				AllSum = sums.ToString();
			}
			UpdateWeight();
			onChangedSum?.Invoke();
		}

		private void AddBackData(FullBackData backData)
		{
			backData.SubscribeChangedSum(OnChangedSomeSum);
			Data.Children.Add(backData);
			backData.UpdateInputStates();
		}

		public void SetViewByTemplate(DocumentTemplateType templateType)
		{
		}

		public void SetActSum(uint actSum)
		{
			controller.ActSum = actSum;
			UpdateWeight();
		}

		public void UpdateWeight()
		{
			if (controller.ActSum != 0 && double.TryParse(AllSum, out double sum))
			{
				WeightAllText = (sum / controller.ActSum).ToString();
				if (WeightAllText.Length > 5)
				{
					WeightAllText = WeightAllText.Substring(0, 5) + "..";
				}
			}
			else
			{
				WeightAllText = "0";
			}
		}

		public void UpdateBackDataIds()
		{
			if (Data != null)
			{
				uint counter = 1;
				foreach (UIElement elem in Data.Children)
				{
					if (elem is FullBackData backData)
					{
						backData.BackDataId = counter++;
						backData.SetDataFromController();
					}
				}
			}
		}
	}
}
