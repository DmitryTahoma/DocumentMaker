using Dml.Controller;
using Dml.Model.Back;
using Dml.Model.Template;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Dml.Controls
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
		public static readonly DependencyProperty IsSketchProperty;
		public static readonly DependencyProperty TimeTextProperty;
		public static readonly DependencyProperty IsOtherTypeProperty;
		public static readonly DependencyProperty IsNotOtherTypeProperty;
		public static readonly DependencyProperty OtherTextProperty;

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
			IsSketchProperty = DependencyProperty.Register("IsSketch", typeof(bool), typeof(BackDataController));
			TimeTextProperty = DependencyProperty.Register("TimeText", typeof(string), typeof(BackDataController));
			IsOtherTypeProperty = DependencyProperty.Register("IsOtherType", typeof(bool), typeof(BackDataController));
			IsNotOtherTypeProperty = DependencyProperty.Register("IsNotOtherType", typeof(bool), typeof(BackDataController));
			OtherTextProperty = DependencyProperty.Register("OtherText", typeof(string), typeof(BackDataController));
		}

		private BackDataController controller;

		private event Action onDeletion;
		private event Action onChangedTime;

		public BackData()
		{
			controller = new BackDataController();

			InitializeComponent();
			DataContext = this;
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

		public IList<BackDataType> BackDataTypesList => controller.BackDataTypesList;

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

		public bool IsSketch
		{
			get => (bool)GetValue(IsSketchProperty);
			set
			{
				SetValue(IsSketchProperty, value);
				controller.IsSketch = value;
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

		public bool IsOtherType
		{
			get => (bool)GetValue(IsOtherTypeProperty);
			set => SetValue(IsOtherTypeProperty, value);
		}

		public bool IsNotOtherType
		{
			get => (bool)GetValue(IsNotOtherTypeProperty);
			set => SetValue(IsNotOtherTypeProperty, value);
		}

		public string OtherText
		{
			get => (string)GetValue(OtherTextProperty);
			set
			{
				SetValue(OtherTextProperty, value);
				controller.OtherText = value;
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
			foreach (BackDataType backData in BackDataTypesList)
			{
				if (backData.Type == controller.Type)
				{
					BackTypeComboBox.SelectedItem = backData;
					break;
				}
			}
			BackNumberTextInput.Text = controller.BackNumberText;
			BackNameInput.Text = controller.BackName;
			CountRegionsTextInput.Text = controller.BackCountRegionsText;
			GameNameInput.Text = controller.GameName;
			IsReworkCheckBox.IsChecked = controller.IsRework;
			IsSketchCheckBox.IsChecked = controller.IsSketch;
			TimeTextInput.Text = controller.SpentTimeText;
			OtherTextInput.Text = controller.OtherText;

			UpdateInputStates();
		}

		private void TypeChanged(object sender, SelectionChangedEventArgs e)
		{
			if (controller != null && sender is ComboBox comboBox && comboBox.SelectedItem is BackDataType dataType)
			{
				controller.Type = dataType.Type;
				UpdateInputStates();
			}
		}

		private void DeleteBtnClick(object sender, RoutedEventArgs e)
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
			if (!IsRegions && CountRegionsTextInput != null)
			{
				CountRegionsTextInput.Text = controller.BackCountRegionsText;
			}

			HasBackNumber = controller.Type != BackType.Craft;
			if (!HasBackNumber && BackNumberTextInput != null)
			{
				BackNumberTextInput.Text = controller.BackNumberText;
			}

			IsOtherType = controller.Type == BackType.Other;
			if (OtherTextInput != null)
			{
				OtherTextInput.Visibility = IsOtherType ? Visibility.Visible : Visibility.Collapsed;
			}
			IsNotOtherType = !IsOtherType;
			if (GridWithGeneralData != null)
			{
				GridWithGeneralData.Visibility = IsOtherType ? Visibility.Hidden : Visibility.Visible;
			}
		}

		public void SetViewByTemplate(DocumentTemplateType templateType)
		{
			if (templateType == DocumentTemplateType.Painter)
			{
				IsSketchCheckBox.Visibility = Visibility.Visible;
				IsSketchColumn.Width = new GridLength(1, GridUnitType.Star);
			}
			else
			{
				IsSketchCheckBox.Visibility = Visibility.Collapsed;
				IsSketchColumn.Width = GridLength.Auto;
			}
		}

		public void SetBackType(BackType type)
		{
			controller.Type = type;
			SetDataFromController();
			UpdateInputStates();
		}

		public BackType GetBackType()
		{
			return controller.Type;
		}
	}
}
