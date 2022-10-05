using Db.Context.HumanPart;
using HumanEditorLib.View;
using Mvvm.Commands;
using System.Windows;

namespace HumanEditorLib.ViewModel
{
	public class BankControlViewModel : DependencyObject
	{
		Bank model = null;

		public BankControlViewModel()
		{
			DeleteCommand = new Command<BankControl>(OnDeleteCommandExecute);
			SomePropertyChanged = new Command(OnSomePropertyChangedExecute);
		}

		#region Properties

		public string Name
		{
			get => (string)GetValue(NameProperty);
			set => SetValue(NameProperty, value);
		}
		public static readonly DependencyProperty NameProperty = DependencyProperty.Register(nameof(Name), typeof(string), typeof(BankControlViewModel));

		public string IBT
		{
			get => (string)GetValue(IBTProperty);
			set => SetValue(IBTProperty, value);
		}
		public static readonly DependencyProperty IBTProperty = DependencyProperty.Register(nameof(IBT), typeof(string), typeof(BankControlViewModel));

		#endregion

		#region Commands

		public Command<BankControl> DeleteBank { get; set; } = null;
		public Command<BankControl> DeleteCommand { get; private set; }
		private void OnDeleteCommandExecute(BankControl bankControl)
		{
			DeleteBank.Execute(bankControl);
		}

		public Command PropertyChangedCommand { get; set; } = null;
		public Command SomePropertyChanged { get; private set; }
		private void OnSomePropertyChangedExecute()
		{
			PropertyChangedCommand.Execute();
		}

		#endregion

		#region Methods

		public void SetModel(Bank bank)
		{
			model = bank;
			Name = bank.Name;
			IBT = bank.IBT == 0 ? "" : bank.IBT.ToString();
		}

		public Bank GetModel()
		{
			model.Name = Name;
			if (int.TryParse(IBT, out int ibt) && ibt > 0) model.IBT = ibt;
			return model;
		}

		#endregion
	}
}
