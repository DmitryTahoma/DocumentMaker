using ActCreator.Controller.Controls;
using Dml.Controller.Validation;
using Dml.Model.Back;
using Dml.Model.Template;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ActCreator.View.Controls
{
	public delegate void ActionWithShortBackData(ShortBackData backData);

	/// <summary>
	/// Interaction logic for ShortBackData.xaml
	/// </summary>
	public partial class ShortBackData : UserControl, INotifyPropertyChanged
	{
		public static readonly DependencyProperty IsRegionsProperty;
		public static readonly DependencyProperty HasBackNumberProperty;
		public static readonly DependencyProperty BackDataIdProperty;
		public static readonly DependencyProperty EpisodeNumberTextProperty;
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

		static ShortBackData()
		{
			IsRegionsProperty = DependencyProperty.Register("IsRegions", typeof(bool), typeof(ShortBackData));
			HasBackNumberProperty = DependencyProperty.Register("HasBackNumber", typeof(bool), typeof(ShortBackData));
			BackDataIdProperty = DependencyProperty.Register("BackDataId", typeof(uint), typeof(ShortBackDataController));
			EpisodeNumberTextProperty = DependencyProperty.Register("EpisodeNumberText", typeof(string), typeof(ShortBackDataController));
			BackNumberTextProperty = DependencyProperty.Register("BackNumberText", typeof(string), typeof(ShortBackDataController));
			BackNameProperty = DependencyProperty.Register("BackName", typeof(string), typeof(ShortBackDataController));
			CountRegionsTextProperty = DependencyProperty.Register("CountRegionsText", typeof(string), typeof(ShortBackDataController));
			GameNameProperty = DependencyProperty.Register("GameName", typeof(string), typeof(ShortBackDataController));
			IsReworkProperty = DependencyProperty.Register("IsRework", typeof(bool), typeof(ShortBackDataController));
			IsSketchProperty = DependencyProperty.Register("IsSketch", typeof(bool), typeof(ShortBackDataController));
			TimeTextProperty = DependencyProperty.Register("TimeText", typeof(string), typeof(ShortBackDataController));
			IsOtherTypeProperty = DependencyProperty.Register("IsOtherType", typeof(bool), typeof(ShortBackDataController));
			IsNotOtherTypeProperty = DependencyProperty.Register("IsNotOtherType", typeof(bool), typeof(ShortBackDataController));
			OtherTextProperty = DependencyProperty.Register("OtherText", typeof(string), typeof(ShortBackDataController));
		}

		private ShortBackDataController controller;
		private readonly InputingValidator inputingValidator;

		private event Action onDeletion;
		private event Action onChangedTime;

		public event PropertyChangedEventHandler PropertyChanged;

		public ShortBackData()
		{
			controller = new ShortBackDataController();

			InitializeComponent();
			DataContext = this;

			inputingValidator = new InputingValidator();
			EpisodeNumberComboBox.CommandBindings.Add(new CommandBinding(ApplicationCommands.Paste, inputingValidator.BlockingCommand));
			TimeTextInput.CommandBindings.Add(new CommandBinding(ApplicationCommands.Paste, inputingValidator.BlockingCommand));
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

		public IList<string> EpisodeNumberList => controller.GameNameList.FirstOrDefault(x => x.Name == GameName)?.Episodes;

		public string EpisodeNumberText
		{
			get => (string)GetValue(EpisodeNumberTextProperty);
			set
			{
				SetValue(EpisodeNumberTextProperty, value);
				controller.EpisodeNumberText = value;
				NotifyPropertyChanged(nameof(EpisodeNumberText));
			}
		}

		public string BackNumberText
		{
			get => (string)GetValue(BackNumberTextProperty);
			set
			{
				SetValue(BackNumberTextProperty, value);
				controller.BackNumberText = value;
				NotifyPropertyChanged(nameof(BackNumberText));
			}
		}

		public string BackName
		{
			get => (string)GetValue(BackNameProperty);
			set
			{
				SetValue(BackNameProperty, value);
				controller.BackName = value;
				NotifyPropertyChanged(nameof(BackName));
			}
		}

		public string CountRegionsText
		{
			get => (string)GetValue(CountRegionsTextProperty);
			set
			{
				SetValue(CountRegionsTextProperty, value);
				controller.BackCountRegionsText = value;
				NotifyPropertyChanged(nameof(CountRegionsText));
			}
		}

		public IList<GameObject> GameNameList => controller.GameNameList;

		public string GameName
		{
			get => (string)GetValue(GameNameProperty);
			set
			{
				SetValue(GameNameProperty, value);
				controller.GameName = value;
				NotifyPropertyChanged(nameof(GameName));
				NotifyPropertyChanged(nameof(EpisodeNumberList));
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
				NotifyPropertyChanged(nameof(IsRework));
			}
		}

		public bool IsSketch
		{
			get => (bool)GetValue(IsSketchProperty);
			set
			{
				SetValue(IsSketchProperty, value);
				controller.IsSketch = value;
				NotifyPropertyChanged(nameof(IsSketch));
			}
		}

		public string TimeText
		{
			get => (string)GetValue(TimeTextProperty);
			set
			{
				SetValue(TimeTextProperty, value);
				controller.SpentTimeText = value;
				NotifyPropertyChanged(nameof(TimeText));
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
				NotifyPropertyChanged(nameof(OtherText));
			}
		}

		public ShortBackDataController Controller
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
			EpisodeNumberComboBox.Text = controller.EpisodeNumberText;
			BackNumberTextInput.Text = controller.BackNumberText;
			BackNameInput.Text = controller.BackName;
			CountRegionsTextInput.Text = controller.BackCountRegionsText;
			GameNameComboBox.Text = controller.GameName;
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
				NotifyPropertyChanged(nameof(controller.Type));
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

		private void UIntValidating(object sender, TextCompositionEventArgs e)
		{
			inputingValidator.UIntInputing_PreviewTextInput(sender, e);
		}

		private void UFloatValidating(object sender, System.Windows.Input.TextCompositionEventArgs e)
		{
			inputingValidator.UFloatInputing_PreviewTextInput(sender, e);
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

		public void SetGameNameList(IList<GameObject> gameObjects)
		{
			string selectedGame = controller.GameName;
			string selectedEpisode = controller.EpisodeNumberText;
			controller.GameNameList.Clear();
			if (gameObjects != null)
			{
				controller.GameNameList.AddRange(gameObjects);
			}
			NotifyPropertyChanged(nameof(GameNameList));
			GameNameComboBox.SelectedItem = GameNameList.FirstOrDefault(x => x.Name == selectedGame);
			if (GameNameComboBox.SelectedItem == null)
			{
				GameNameComboBox.Text = null;
			}
			NotifyPropertyChanged(nameof(EpisodeNumberList));
			EpisodeNumberComboBox.SelectedItem = EpisodeNumberList?.FirstOrDefault(x => x == selectedEpisode);
			if (EpisodeNumberComboBox.SelectedItem == null)
			{
				EpisodeNumberComboBox.Text = null;
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

		public void NotifyPropertyChanged(string name)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}
}
