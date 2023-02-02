using Mvvm.Commands;
using System;
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
		public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(CheckBoxList));

		public bool? IsSelectedAll
		{
			get { return (bool?)GetValue(IsSelectedAllProperty); }
			set { SetValue(IsSelectedAllProperty, value); }
		}
		public static readonly DependencyProperty IsSelectedAllProperty = DependencyProperty.Register(nameof(IsSelectedAll), typeof(bool?), typeof(CheckBoxList), new PropertyMetadata(false));

		public bool IsEmptyText
		{
			get { return (bool)GetValue(IsEmptyTextProperty); }
			set { SetValue(IsEmptyTextProperty, value); }
		}
		public static readonly DependencyProperty IsEmptyTextProperty = DependencyProperty.Register(nameof(IsEmptyText), typeof(bool), typeof(CheckBoxList));

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
			CheckedList = CheckedList.Append((sender as CheckBox).Content).ToList();
			CheckBoxCheckedChanged(sender, e);
		}

		private void CheckBoxUnchecked(object sender, RoutedEventArgs e)
		{
			object obj = (sender as CheckBox).Content;
			CheckedList = CheckedList.Where(x => x != obj).ToList();
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
				IsEmptyText = true;
			}
			else
			{
				IsEmptyText = false;
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
			return UpdateGeneralCheckBoxState(checkBoxList.Cast<CheckBox>().Where(x => x.Visibility == Visibility.Visible), checkBox => checkBox.IsChecked);
		}

		public static bool? UpdateGeneralCheckBoxState<T>(IEnumerable<T> enumerable, Func<T, bool?> propertySelector)
		{
			IEnumerator<T> e = enumerable.GetEnumerator();

			bool findedFirst = false;
			bool? isChecked = null;

			while(e.MoveNext())
			{
				findedFirst = true;
				isChecked = propertySelector(e.Current);
				break;
			}

			if(!findedFirst)
			{
				return false;
			}
			else
			{
				while(e.MoveNext())
				{
					if(propertySelector(e.Current) != isChecked)
					{
						return null;
					}
				}

				return isChecked;
			}
		}

		public void CheckFirst()
		{
			CheckBox first = ItemsStackPanel.Children.Cast<CheckBox>().FirstOrDefault();
			if(first != null)
			{
				first.IsChecked = true;
			}
		}

		#endregion
	}
}
