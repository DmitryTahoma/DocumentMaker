using System;
using System.Windows.Input;

namespace Mvvm.Commands
{
	public class Command : ICommand
	{
		bool haveParameter;
		Action action;
		Action<object> paramAction;
		Func<object, bool> canExecute;

		public event EventHandler CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}

		public Command(Action<object> actionCommand, Func<object, bool> canExecute = null)
		{
			action = null;
			paramAction = actionCommand;
			this.canExecute = canExecute;
			haveParameter = true;
		}
		public Command(Action actionCommand, Func<object, bool> canExecute = null)
		{
			action = actionCommand;
			paramAction = null;
			this.canExecute = canExecute;
			haveParameter = false;
		}
		public Command(Action<object> actionCommand, Func<bool> canExecute) : this(actionCommand)
		{
			SetCanExecuteWithoutParams(canExecute);
		}
		public Command(Action actionCommand, Func<bool> canExecute) : this(actionCommand)
		{
			SetCanExecuteWithoutParams(canExecute);
		}

		public bool CanExecute(object parameter)
		{
			return canExecute == null || canExecute(parameter);
		}

		public void Execute(object parameter)
		{
			if (haveParameter)
				paramAction(parameter);
			else 
				action();
		}

		public void Execute() => Execute(null);

		private void SetCanExecuteWithoutParams(Func<bool> func)
		{
			if(func != null)
			{
				canExecute = (_) => func.Invoke();
			}
		}
	}
}
