using System;
using System.Windows.Input;

namespace Mvvm.Commands
{
	public class Command<T> : ICommand
	{
		Action<T> action;
		Func<T, bool> canExecute;

		public event EventHandler CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}

		public Command(Action<T> actionCommand, Func<T, bool> canExecute = null)
		{
			action = actionCommand;
			this.canExecute = canExecute;
		}

		public bool CanExecute(object parameter)
		{
			return canExecute == null || (parameter is T tParam && canExecute(tParam));
		}

		public void Execute(object parameter)
		{
			action((T)parameter);
		}
	}
}
