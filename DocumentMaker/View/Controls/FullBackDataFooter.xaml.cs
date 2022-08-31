using Dml.Model.Template;
using DocumentMaker.Controller.Controls;
using System.Collections.Generic;
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
		private event ActionWithBool onChangedSum;

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

		public void SubscribeChangingSum(ActionWithBool action)
		{
			onChangedSum += action;
		}

		public void ClearData()
		{
			if (Data != null)
			{
				Data.Children.Clear();
			}
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
				bool addedOne = false;
				FullBackData firstChecked;
				do
				{
					firstChecked = null;
					int index = 1;
					foreach (UIElement elem in Data.Children)
					{
						if (elem is FullBackData fullBackData && fullBackData.IsChecked.HasValue && fullBackData.IsChecked.Value)
						{
							firstChecked = fullBackData;
							firstChecked.IsChecked = false;
							break;
						}
						++index;
					}

					if (firstChecked != null)
					{
						FullBackData backData = new FullBackData
						{
							BackNumberText = firstChecked.BackNumberText,
							BackName = firstChecked.BackName,
							CountRegionsText = firstChecked.CountRegionsText,
							GameName = firstChecked.GameName,
							IsRework = firstChecked.IsRework,
							IsSketch = firstChecked.IsSketch,
							OtherText = firstChecked.OtherText,
						};
						backData.SetBackType(firstChecked.GetBackType());
						backData.Controller.WorkObjectId = firstChecked.Controller.WorkObjectId;
						backData.Controller.EpisodeNumberText = firstChecked.Controller.EpisodeNumberText;
						backData.SetDataFromController();
						InsertBackData(backData, index);
						onAdded?.Invoke(backData);

						addedOne = true;
					}

				} while (firstChecked != null);

				if (!addedOne)
				{
					FullBackData backData = new FullBackData();
					AddBackData(backData);
					onAdded?.Invoke(backData);
				}
				OnChangedSomeSum();
				UpdateBackDataIds();
			}
		}

		private void OnChangedSomeSum(bool updateWeight = true)
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
			onChangedSum?.Invoke(updateWeight);
		}

		private void AddBackData(FullBackData backData)
		{
			backData.SubscribeChangedSum(OnChangedSomeSum);
			Data.Children.Add(backData);
			backData.UpdateInputStates();
		}

		private void InsertBackData(FullBackData backData, int index)
		{
			backData.SubscribeChangedSum(OnChangedSomeSum);
			Data.Children.Insert(index, backData);
			backData.UpdateInputStates();
		}

		public void AddMovedBackData(IEnumerable<FullBackData> backDatas)
		{
			foreach (FullBackData backData in backDatas)
			{
				backData.IsChecked = false;
				AddBackData(backData);
				onAdded?.Invoke(backData);
			}
			OnChangedSomeSum(false);
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

		public void UpdateAllSum()
		{
			OnChangedSomeSum(false);
		}
	}
}
