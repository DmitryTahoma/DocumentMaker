using Dml.Controller.Validation;
using Dml.Model.Back;
using Dml.Model.Template;
using Dml.UndoRedo;
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
		public static readonly DependencyProperty IsCheckedProperty;
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
		public static readonly DependencyProperty OtherTextProperty;
		public static readonly DependencyProperty WeightTextProperty;
		public static readonly DependencyProperty SumTextProperty;

		static FullBackData()
		{
			IsCheckedProperty = DependencyProperty.Register("IsChecked", typeof(bool?), typeof(FullBackData));
			IsRegionsProperty = DependencyProperty.Register("IsRegions", typeof(bool), typeof(FullBackData));
			HasBackNumberProperty = DependencyProperty.Register("HasBackNumber", typeof(bool), typeof(FullBackData));
			BackDataIdProperty = DependencyProperty.Register("BackDataId", typeof(uint), typeof(FullBackDataController));
			EpisodeNumberTextProperty = DependencyProperty.Register("EpisodeNumberText", typeof(string), typeof(FullBackDataController));
			BackNumberTextProperty = DependencyProperty.Register("BackNumberText", typeof(string), typeof(FullBackDataController));
			BackNameProperty = DependencyProperty.Register("BackName", typeof(string), typeof(FullBackDataController));
			CountRegionsTextProperty = DependencyProperty.Register("CountRegionsText", typeof(string), typeof(FullBackDataController));
			GameNameProperty = DependencyProperty.Register("GameName", typeof(string), typeof(FullBackDataController));
			IsReworkProperty = DependencyProperty.Register("IsRework", typeof(bool), typeof(FullBackDataController));
			IsSketchProperty = DependencyProperty.Register("IsSketch", typeof(bool), typeof(FullBackDataController));
			OtherTextProperty = DependencyProperty.Register("OtherText", typeof(string), typeof(FullBackDataController));
			WeightTextProperty = DependencyProperty.Register("WeightText", typeof(string), typeof(FullBackDataController));
			SumTextProperty = DependencyProperty.Register("SumText", typeof(string), typeof(FullBackDataController));
		}

		private FullBackDataController controller;
		private readonly InputingValidator inputingValidator;
		private bool isCheckedChangedWithoutCallback;
		private bool needUpdateWeight;

		private event ActionWithBool onChangedSum;
		private event Action onSelectionChanged;

		public event PropertyChangedEventHandler PropertyChanged;

		public FullBackData()
		{
			controller = new FullBackDataController();

			InitializeComponent();
			DataContext = this;
			IsChecked = false;

			inputingValidator = new InputingValidator();
			isCheckedChangedWithoutCallback = false;
			needUpdateWeight = true;
			SumTextInput.CommandBindings.Add(new CommandBinding(ApplicationCommands.Paste, inputingValidator.BlockingCommand));
			EpisodeNumberComboBox.CommandBindings.Add(new CommandBinding(ApplicationCommands.Paste, inputingValidator.BlockingCommand));
		}

		public bool? IsChecked
		{
			get => (bool?)GetValue(IsCheckedProperty);
			set => SetValue(IsCheckedProperty, value);
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

		public IList<string> EpisodeNumberList => controller.GameNameList.FirstOrDefault(x => x.Name == GameName)?.Episodes;

		public string EpisodeNumberText
		{
			get => (string)GetValue(EpisodeNumberTextProperty);
			set
			{
				SetValue(EpisodeNumberTextProperty, value);
				HaveUnsavedChanges = true;
				controller.EpisodeNumberText = value;
			}
		}

		public string BackNumberText
		{
			get => (string)GetValue(BackNumberTextProperty);
			set
			{
				SetValue(BackNumberTextProperty, value);
				HaveUnsavedChanges = true;
				controller.BackNumberText = value;
			}
		}

		public string BackName
		{
			get => (string)GetValue(BackNameProperty);
			set
			{
				SetValue(BackNameProperty, value);
				HaveUnsavedChanges = true;
				controller.BackName = value;
			}
		}

		public string CountRegionsText
		{
			get => (string)GetValue(CountRegionsTextProperty);
			set
			{
				SetValue(CountRegionsTextProperty, value);
				HaveUnsavedChanges = true;
				controller.BackCountRegionsText = value;
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
				HaveUnsavedChanges = true;
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
			}
		}

		public bool IsSketch
		{
			get => (bool)GetValue(IsSketchProperty);
			set
			{
				SetValue(IsSketchProperty, value);
				HaveUnsavedChanges = true;
				controller.IsSketch = value;
			}
		}

		public string OtherText
		{
			get => (string)GetValue(OtherTextProperty);
			set
			{
				SetValue(OtherTextProperty, value);
				HaveUnsavedChanges = true;
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

		public bool HaveUnsavedChanges { get => controller.HaveUnsavedChanges; set => controller.HaveUnsavedChanges = value; }

		public void SubscribeChangedSum(ActionWithBool action)
		{
			onChangedSum += action;
		}

		public void SetDataFromController()
		{
			bool actionsStackingEnable = controller.IsActionsStackingEnabled;
			controller.DisableActionsStacking();

			BackDataIdLabel.Text = controller.Id.ToString();
			foreach (BackDataType backData in BackDataTypesList)
			{
				if (backData.Type == controller.Type)
				{
					BackTypeComboBox.SelectedItem = backData;
					break;
				}
			}
			foreach (WorkObject workObject in WorkTypesList)
			{
				if (workObject.Id == controller.WorkObjectId)
				{
					WorkTypeComboBox.SelectedItem = workObject;
					break;
				}
			}
			EpisodeNumberComboBox.Text = controller.EpisodeNumberText;
			BackNumberTextInput.Text = controller.BackNumberText;
			BackNameInput.Text = controller.BackName;
			CountRegionsTextInput.Text = controller.BackCountRegionsText;
			GameNameComboBox.SelectedItem = GameNameList.FirstOrDefault(x => x.Name == controller.GameName);
			IsSketchCheckBox.IsChecked = controller.IsSketch;
			OtherTextInput.Text = controller.OtherText;
			needUpdateWeight = false;
			SumTextInput.Text = controller.SumText;
			needUpdateWeight = true;

			UpdateInputStates();

			if (actionsStackingEnable) controller.EnableActionsStacking();
		}

		private void TypeChanged(object sender, SelectionChangedEventArgs e)
		{
			if (controller != null && sender is ComboBox comboBox && comboBox.SelectedItem is BackDataType dataType)
			{
				controller.Type = dataType.Type;
				HaveUnsavedChanges = true;
				UpdateInputStates();
			}
		}

		private void WorkTypeChanged(object sender, SelectionChangedEventArgs e)
		{
			if (controller != null && sender is ComboBox comboBox && comboBox.SelectedItem is WorkObject workObject)
			{
				controller.WorkObjectId = workObject.Id;
				HaveUnsavedChanges = true;
			}
		}

		private void SumTextInputTextChanged(object sender, TextChangedEventArgs e)
		{
			HaveUnsavedChanges = true;
			if (controller.IsActionsStackingEnabled && sender is TextBox textBox)
			{
				controller.AddUndoRedoLink(new UndoRedoLink(() =>
				{
					textBox.Text = controller.SumText;
					textBox.Focus();
					textBox.SelectionStart = textBox.Text.Length;
					textBox.SelectionLength = 0;
				}));
			}
			onChangedSum?.Invoke(needUpdateWeight);
			if (needUpdateWeight)
			{
				UpdateWeight();
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

		private void UIntValidating(object sender, System.Windows.Input.TextCompositionEventArgs e)
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

			if (OtherTextInput != null)
			{
				OtherTextInput.Visibility = controller.Type == BackType.Other ? Visibility.Visible : Visibility.Collapsed;
			}

			if (GridWithGeneralData != null)
			{
				GridWithGeneralData.Visibility = controller.Type == BackType.Other ? Visibility.Hidden : Visibility.Visible;
			}

			if (WorkTypeComboBox != null)
			{
				WorkTypeComboBox.Visibility = !controller.IsOtherType ? Visibility.Visible : Visibility.Collapsed;
			}
			if (ColWithWorkTypeComboBox != null)
			{
				ColWithWorkTypeComboBox.Width = !controller.IsOtherType ? new GridLength(1.5, GridUnitType.Star) : GridLength.Auto;
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

		public void SetBackDataTypesList(IList<BackDataType> backDataTypes)
		{
			BackType selectedType = controller.Type;
			controller.BackDataTypesList.Clear();
			if (backDataTypes != null)
			{
				controller.BackDataTypesList.AddRange(backDataTypes);
			}
			NotifyPropertyChanged(nameof(BackDataTypesList));
			BackTypeComboBox.SelectedItem = BackDataTypesList.FirstOrDefault(x => x.Type == selectedType) ?? BackDataTypesList.FirstOrDefault();
		}

		public void SetWorkTypesList(IList<WorkObject> workObjects)
		{
			controller.WorkTypesList.Clear();
			if (workObjects != null)
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
			GameNameComboBox.Text = selectedGame;
			NotifyPropertyChanged(nameof(EpisodeNumberList));
			controller.EpisodeNumberText = selectedEpisode;
			EpisodeNumberComboBox.Text = controller.EpisodeNumberText;
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

		public void SetActSum(uint actSum, bool needUpdateSum)
		{
			controller.ActSum = actSum;
			if (needUpdateSum)
			{
				UpdateSumText();
			}
		}

		private void UpdateWeight()
		{
			if (uint.TryParse(SumText, out uint sum))
			{
				controller.Weight = (double)sum / controller.ActSum;
			}
		}

		private void UpdateSumText()
		{
			needUpdateWeight = false;
			SumText = ((int)(controller.ActSum * controller.Weight)).ToString();
			SumTextInput.Text = controller.SumText;
			needUpdateWeight = true;
		}

		public void NotifyPropertyChanged(string name)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

		public void SubscribeSelectionChanged(Action action)
		{
			onSelectionChanged += action;
		}

		public void SetIsCheckedWithoutCallback(bool? value)
		{
			isCheckedChangedWithoutCallback = true;
			IsChecked = value;
			isCheckedChangedWithoutCallback = false;
		}

		private void NumberCheckedChanged(object sender, RoutedEventArgs e)
		{
			if (isCheckedChangedWithoutCallback)
				isCheckedChangedWithoutCallback = false;
			else if (sender is CheckBox checkBox)
				onSelectionChanged?.Invoke();
		}

		public void UnsubscribeAllEvents()
		{
			onChangedSum = null;
			onSelectionChanged = null;
		}

		public void SetSumTextChangesWithLink(string sumText)
		{
			controller.SetSumTextChangesWithLink(sumText, new UndoRedoLink(() =>
			{
				needUpdateWeight = false;
				SumTextInput.Text = controller.SumText;
				needUpdateWeight = true;
			}));
		}

		public void SetSumTextChangesWithAction(string sumText)
		{
			bool isActionStackingEnable = controller.IsActionsStackingEnabled;
			controller.EnableActionsStacking();

			bool isCollapsingActionByTargetEnabled = controller.CollapsingActionByTargetEnabled;
			controller.DisableCollapsingActionByTargetEnabled();

			controller.SumText = sumText;

			if (!isActionStackingEnable) controller.DisableActionsStacking();
			if (isCollapsingActionByTargetEnabled) controller.EnableCollapsingActionByTargetEnabled();
		}
	}
}
