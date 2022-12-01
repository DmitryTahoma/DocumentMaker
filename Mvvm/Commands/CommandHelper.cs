using System.Windows.Input;

namespace Mvvm.Commands
{
	public static class CommandHelper
	{
		public static bool UpdateAllCanExecute(object _)
		{
			CommandManager.InvalidateRequerySuggested();
			return true;
		}
	}
}
