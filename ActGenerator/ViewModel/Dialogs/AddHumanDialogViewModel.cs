using Mvvm;
using Mvvm.Commands;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ActGenerator.ViewModel.Dialogs
{
	public class AddHumanDialogViewModel : DependencyObject
	{
		Grid humenGrid = null;

		public AddHumanDialogViewModel()
		{
			InitCommands();

			State = ViewModelState.Initialized;
		}

		#region Properties

		public ViewModelState State
		{
			get { return (ViewModelState)GetValue(StateProperty); }
			set { SetValue(StateProperty, value); }
		}
		public static readonly DependencyProperty StateProperty = DependencyProperty.Register(nameof(State), typeof(ViewModelState), typeof(AddHumanDialogViewModel));

		#endregion

		#region Commands

		private void InitCommands()
		{
			ViewLoaded = new Command(OnViewLoadedExecute);
			BindHumenGrid = new Command<Grid>(OnBindHumenGridExecute);
		}

		public Command<Grid> BindHumenGrid { get; private set; }
		private void OnBindHumenGridExecute(Grid humenGrid)
		{
			if (this.humenGrid == null)
			{
				this.humenGrid = humenGrid;
			}
		}

		public Command ViewLoaded { get; private set; }
		private async void OnViewLoadedExecute()
		{
			if(State == ViewModelState.Initialized)
			{
				State = ViewModelState.Loading;
				for(int i = 0; i < 100; ++i)
				{
					new Item("Алєйникова Марина Сергіївна", "Технічний дизайнер").AddToGrid(humenGrid);
					await Task.Delay(1);
				}
				State = ViewModelState.Loaded;
			}
		}

		#endregion

		#region Methods

		#endregion

		class Item
		{
			static readonly Style checkBoxItemStyle = Application.Current.FindResource("AddHumanDialogItemCheckBox") as Style;
			static readonly Style textBlockItemStyle = Application.Current.FindResource("AddHumanDialogItemTextBlock") as Style;

			public Item(string name, string template)
			{
				NameCheckBox = new CheckBox { Content = name, Style = checkBoxItemStyle };
				TemplateTextBlock = new TextBlock { Text = template, Style = textBlockItemStyle };
				Grid.SetColumn(TemplateTextBlock, 1);
			}

			public CheckBox NameCheckBox { get; private set; }
			public TextBlock TemplateTextBlock { get; private set; }

			public void AddToGrid(Grid grid)
			{
				Grid.SetRow(NameCheckBox, grid.RowDefinitions.Count);
				Grid.SetRow(TemplateTextBlock, grid.RowDefinitions.Count);
				grid.RowDefinitions.Add(new RowDefinition());
				grid.Children.Add(NameCheckBox);
				grid.Children.Add(TemplateTextBlock);
			}
		}
	}
}
