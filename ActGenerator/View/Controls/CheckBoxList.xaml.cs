using Mvvm.Commands;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ActGenerator.View.Controls
{
	/// <summary>
	/// Interaction logic for CheckBoxList.xaml
	/// </summary>
	public partial class CheckBoxList : UserControl
	{
		private const string emptyTextValue = "<не обрано>";

		private bool needUpdateIsSelectedAll = true;

		public CheckBoxList()
		{
			InitializeComponent();

			CheckedList = new List<object>();

			SelectAllCommand = new Command(OnSelectAllCommandExecute);
		}

		#region Properties

		public IEnumerable ItemsSource
		{
			get { return (IEnumerable)GetValue(ItemsSourceProperty); }
			set { SetValue(ItemsSourceProperty, value); }
		}
		public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(CheckBoxList), new FrameworkPropertyMetadata(null, OnItemsSourceChanged));

		public List<object> CheckedList
		{
			get { return (List<object>)GetValue(CheckedListProperty); }
			set { SetValue(CheckedListProperty, value); }
		}
		public static readonly DependencyProperty CheckedListProperty = DependencyProperty.Register(nameof(CheckedList), typeof(List<object>), typeof(CheckBoxList));

		public string Text
		{
			get { return (string)GetValue(TextProperty); }
			set { SetValue(TextProperty, value); }
		}
		public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(CheckBoxList), new PropertyMetadata(emptyTextValue));

		public bool? IsSelectedAll
		{
			get { return (bool?)GetValue(IsSelectedAllProperty); }
			set { SetValue(IsSelectedAllProperty, value); }
		}
		public static readonly DependencyProperty IsSelectedAllProperty = DependencyProperty.Register(nameof(IsSelectedAll), typeof(bool?), typeof(CheckBoxList), new PropertyMetadata(false));

		#endregion

		#region Commands

		public Command SelectAllCommand { get; private set; }
		private void OnSelectAllCommandExecute()
		{
			needUpdateIsSelectedAll = false;
			foreach(CheckBox checkBox in ItemsStackPanel.Children)
			{
				checkBox.IsChecked = IsSelectedAll;
			}
			needUpdateIsSelectedAll = true;
		}

		#endregion

		#region Methods

		private static void OnItemsSourceChanged(DependencyObject s, DependencyPropertyChangedEventArgs e)
		{
			CheckBoxList sender = s as CheckBoxList;

			int count = sender.ItemsSource is ICollection collection ? collection.Count : sender.ItemsSource.Cast<object>().Count();

			while (sender.ItemsStackPanel.Children.Count > count)
			{
				sender.ItemsStackPanel.Children.RemoveAt(0);
			}

			while(sender.ItemsStackPanel.Children.Count < count)
			{
				sender.ItemsStackPanel.Children.Add(sender.CreateCheckBox());
			}

			if(sender.ItemsSource != null)
			{
				IEnumerator<CheckBox> itemsStackPanelEnum = sender.ItemsStackPanel.Children.Cast<CheckBox>().GetEnumerator();
				IEnumerator itemsSourceEnum = sender.ItemsSource.GetEnumerator();

				while(itemsStackPanelEnum.MoveNext() && itemsSourceEnum.MoveNext())
				{
					itemsStackPanelEnum.Current.Content = itemsSourceEnum.Current;
					itemsStackPanelEnum.Current.IsChecked = false;
				}
			}
		}

		private CheckBox CreateCheckBox()
		{
			CheckBox checkBox = new CheckBox();
			checkBox.Checked += CheckBoxChecked;
			checkBox.Unchecked += CheckBoxUnchecked;
			return checkBox;
		}

		private void CheckBoxChecked(object sender, RoutedEventArgs e)
		{
			CheckedList.Add((sender as CheckBox).Content);
			CheckBoxCheckedChanged(sender, e);
		}

		private void CheckBoxUnchecked(object sender, RoutedEventArgs e)
		{
			CheckedList.Remove((sender as CheckBox).Content);
			CheckBoxCheckedChanged(sender, e);
		}

		private void CheckBoxCheckedChanged(object sender, RoutedEventArgs e)
		{
			UpdateText();
			UpdateIsSelectedAll();
		}

		private void UpdateText()
		{
			IEnumerator<CheckBox> checkedListEnum = ItemsStackPanel.Children.Cast<CheckBox>().GetEnumerator();

			bool findedFirst = false;
			while(checkedListEnum.MoveNext())
			{
				if(checkedListEnum.Current.IsChecked == true)
				{
					Text = checkedListEnum.Current.Content.ToString();
					findedFirst = true;
					break;
				}
			}

			if (!findedFirst)
			{
				Text = emptyTextValue;
			}
			else
			{
				while (checkedListEnum.MoveNext())
				{
					if (checkedListEnum.Current.IsChecked == true)
					{
						Text += ", " + checkedListEnum.Current.Content.ToString();
					}
				}
			}
		}

		private void UpdateIsSelectedAll()
		{
			if (!needUpdateIsSelectedAll) return;

			IsSelectedAll = UpdateGeneralCheckBoxState(ItemsStackPanel.Children);
		}

		public static bool? UpdateGeneralCheckBoxState(IEnumerable checkBoxList)
		{
			IEnumerator<CheckBox> checkBoxListEnum = checkBoxList.Cast<CheckBox>().GetEnumerator();

			bool findedFirst = false;
			bool? isChecked = null;

			while (checkBoxListEnum.MoveNext())
			{
				if (checkBoxListEnum.Current.Visibility == Visibility.Visible)
				{
					findedFirst = true;
					isChecked = checkBoxListEnum.Current.IsChecked;
					break;
				}
			}

			if (!findedFirst)
			{
				return false;
			}
			else
			{
				while (checkBoxListEnum.MoveNext())
				{
					if (checkBoxListEnum.Current.Visibility == Visibility.Visible && checkBoxListEnum.Current.IsChecked != isChecked)
					{
						return null;
					}
				}

				return isChecked;
			}
		}

		#endregion
	}
}
