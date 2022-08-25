using Dml.Controller.Validation;
using Dml.Model.Back;
using Dml.Model.Template;
using DocumentMaker.Controller.Controls;
using DocumentMaker.Model.Back;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DocumentMaker.View.Controls
{
	public delegate void ActionWithFullBackData(FullBackData backData);

	/// <summary>
	/// Interaction logic for FullBackData.xaml
	/// </summary>
	public partial class FullBackData : UserControl, INotifyPropertyChanged
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
		public static readonly DependencyProperty IsOtherTypeProperty;
		public static readonly DependencyProperty IsNotOtherTypeProperty;
		public static readonly DependencyProperty OtherTextProperty;
		public static readonly DependencyProperty WeightTextProperty;
		public static readonly DependencyProperty SumTextProperty;

		static FullBackData()
		{
			IsRegionsProperty = DependencyProperty.Register("IsRegions", typeof(bool), typeof(FullBackData));
			HasBackNumberProperty = DependencyProperty.Register("HasBackNumber", typeof(bool), typeof(FullBackData));
			BackDataIdProperty = DependencyProperty.Register("BackDataId", typeof(uint), typeof(FullBackDataController));
			BackNumberTextProperty = DependencyProperty.Register("BackNumberText", typeof(string), typeof(FullBackDataController));
			BackNameProperty = DependencyProperty.Register("BackName", typeof(string), typeof(FullBackDataController));
			CountRegionsTextProperty = DependencyProperty.Register("CountRegionsText", typeof(string), typeof(FullBackDataController));
			GameNameProperty = DependencyProperty.Register("GameName", typeof(string), typeof(FullBackDataController));
			IsReworkProperty = DependencyProperty.Register("IsRework", typeof(bool), typeof(FullBackDataController));
			IsSketchProperty = DependencyProperty.Register("IsSketch", typeof(bool), typeof(FullBackDataController));
			IsOtherTypeProperty = DependencyProperty.Register("IsOtherType", typeof(bool), typeof(FullBackDataController));
			IsNotOtherTypeProperty = DependencyProperty.Register("IsNotOtherType", typeof(bool), typeof(FullBackDataController));
			OtherTextProperty = DependencyProperty.Register("OtherText", typeof(string), typeof(FullBackDataController));
			WeightTextProperty = DependencyProperty.Register("WeightText", typeof(string), typeof(FullBackDataController));
			SumTextProperty = DependencyProperty.Register("SumText", typeof(string), typeof(FullBackDataController));
		}

		private FullBackDataController controller;
		private readonly InputingValidator inputingValidator;

		private event Action onChangedSum;

		public event PropertyChangedEventHandler PropertyChanged;

		public FullBackData()
		{
			controller = new FullBackDataController();

			InitializeComponent();
			DataContext = this;

			inputingValidator = new InputingValidator();
			SumTextInput.CommandBindings.Add(new CommandBinding(ApplicationCommands.Paste, inputingValidator.BlockingCommand));
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

		public IList<WorkObject> WorkTypesList => controller.WorkTypesList;

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

		public IList<string> GameNameList => controller.GameNameList;

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

		public string WeightText
		{
			get => (string)GetValue(WeightTextProperty);
			set
			{
				SetValue(WeightTextProperty, value);
				controller.WeightText = value;
			}
		}

		public string SumText
		{
			get => (string)GetValue(SumTextProperty);
			set
			{
				SetValue(SumTextProperty, value);
				controller.SumText = value;
			}
		}

		public FullBackDataController Controller
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

		public void SubscribeChangedSum(Action action)
		{
			onChangedSum += action;
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
			GameNameComboBox.Text = controller.GameName;
			IsSketchCheckBox.IsChecked = controller.IsSketch;
			OtherTextInput.Text = controller.OtherText;
			SumTextInput.Text = controller.SumText;

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

		private void WorkTypeChanged(object sender, SelectionChangedEventArgs e)
		{
			if(controller != null && sender is ComboBox comboBox && comboBox.SelectedItem is WorkObject workObject)
			{
				controller.WorkObjectId = workObject.Id;
			}
		}

		private void SumTextInputTextChanged(object sender, TextChangedEventArgs e)
		{
			onChangedSum?.Invoke();
			UpdateWeight();
		}

		private void RegionsValidatingPreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			if (e.Text.Length == 1 && sender is TextBox textBox)
			{
				char k = e.Text[0];
				if ((k < '0' || k > '9') && k != '-' && k != ',' && k != ' ')
				{
					e.Handled = true;
				}
				else
				{
					string text = textBox.Text.Replace(" ", "");
					if (k == ',' || k == '-')
					{
						if (text.Length <= 0)
						{
							e.Handled = true;
							return;
						}
						char last = text.Last();
						if (last == ',' || last == '-')
						{
							e.Handled = true;
							return;
						}
					}

					if (k == '-')
					{
						if (text.LastIndexOf('-') > text.LastIndexOf(','))
						{
							e.Handled = true;
						}
					}
				}
			}
		}

		private void UIntValidating(object sender, System.Windows.Input.TextCompositionEventArgs e)
		{
			inputingValidator.UIntInputing_PreviewTextInput(sender, e);
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

			bool isVisible = WorkTypesList.Count > 0;
			if (WorkTypeComboBox != null)
			{
				WorkTypeComboBox.Visibility = controller.IsRework ? Visibility.Visible : Visibility.Collapsed;
			}
			if(ColWithWorkTypeComboBox != null)
			{
				ColWithWorkTypeComboBox.Width = controller.IsRework ? new GridLength(1.5, GridUnitType.Star) : GridLength.Auto;
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

		public void SetWorkTypesList(IList<WorkObject> workObjects)
		{
			controller.WorkTypesList.Clear();
			if(workObjects != null)
			{
				controller.WorkTypesList.AddRange(workObjects);
			}
			else
			{
				controller.WorkTypesList.Add(new WorkObject { Name = "<error-loading-work-type-list>" });
			}
			NotifyPropertyChanged(nameof(WorkTypesList));
			WorkTypeComboBox.SelectedItem = WorkTypesList.FirstOrDefault(x => x.Id == controller.WorkObjectId % WorkTypesList.Count);
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

		public void SetActSum(uint actSum)
		{
			controller.ActSum = actSum;
			UpdateWeight();
		}

		public void UpdateWeight()
		{
		}

		public void NotifyPropertyChanged(string name)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}
}
