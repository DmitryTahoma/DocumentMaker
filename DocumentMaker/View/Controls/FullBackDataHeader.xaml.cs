using Dml.Model.Template;
using System.Windows;
using System.Windows.Controls;

namespace DocumentMaker.View.Controls
{
	public delegate void ActionWithNullableBool(bool? b);
	/// <summary>
	/// Interaction logic for FullBackDataHeader.xaml
	/// </summary>
	public partial class FullBackDataHeader : UserControl
	{
		public static readonly DependencyProperty DataProperty;
		public static readonly DependencyProperty IsCheckedProperty;

		static FullBackDataHeader()
		{
			DataProperty = DependencyProperty.Register("Data", typeof(StackPanel), typeof(FullBackDataHeader));
			IsCheckedProperty = DependencyProperty.Register("IsChecked", typeof(bool?), typeof(FullBackDataHeader));
		}

		private bool isCheckedChangedWithoutCallback;

		private event ActionWithNullableBool onSelectionChanged;

		public FullBackDataHeader()
		{
			InitializeComponent();
			DataContext = this;
			IsChecked = false;

			isCheckedChangedWithoutCallback = false;
		}

		public StackPanel Data
		{
			get => GetValue(DataProperty) as StackPanel;
			set => SetValue(DataProperty, value);
		}

		public bool? IsChecked
		{
			get => (bool?)GetValue(IsCheckedProperty);
			set => SetValue(IsCheckedProperty, value);
		}

		public void SetViewByTemplate(DocumentTemplateType templateType)
		{
			if (templateType == DocumentTemplateType.Painter)
			{
				IsSketchTextBlock.Visibility = Visibility.Visible;
				IsSketchColumn.Width = new GridLength(1, GridUnitType.Star);
			}
			else
			{
				IsSketchTextBlock.Visibility = Visibility.Collapsed;
				IsSketchColumn.Width = GridLength.Auto;
			}
		}

		public void HideWorkTypeLabel()
		{
			if (ColWithWorkTypeComboBox != null)
			{
				ColWithWorkTypeComboBox.Width = GridLength.Auto;
			}
			if (WorkTypeLabel != null)
			{
				WorkTypeLabel.Visibility = Visibility.Collapsed;
			}
		}

		public void SubscribeSelectionChanged(ActionWithNullableBool action)
		{
			onSelectionChanged += action;
		}

		public void UpdateIsCheckedState()
		{
			if(Data != null)
			{
				bool? state = null;

				if (Data.Children.Count <= 0)
				{
					state = false;
				}
				else
				{
					foreach (UIElement elem in Data.Children)
					{
						if (elem is FullBackData backData)
						{
							if (backData.IsChecked.HasValue)
							{
								if (state == null)
								{
									state = backData.IsChecked.Value;
								}
								else if (state != backData.IsChecked.Value)
								{
									state = null;
									break;
								}
							}
						}
					}
				}

				isCheckedChangedWithoutCallback = true;
				IsChecked = state;
				isCheckedChangedWithoutCallback = false;
			}
		}

		private void NumberCheckedChanged(object sender, RoutedEventArgs e)
		{
			if (isCheckedChangedWithoutCallback) 
				isCheckedChangedWithoutCallback = false;
			else if (sender is CheckBox checkBox)
				onSelectionChanged?.Invoke(checkBox.IsChecked);
		}
	}
}
