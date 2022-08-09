using System.Windows;

namespace Dml.Controller.Validation
{
	public class WindowValidator
	{
		public static void MoveToValidPosition(Window window)
		{
			if (window.Top + window.Height > SystemParameters.VirtualScreenHeight + SystemParameters.VirtualScreenTop)
			{
				window.Top = SystemParameters.VirtualScreenHeight + SystemParameters.VirtualScreenTop - window.Height;
			}
			if (window.Left + window.Width > SystemParameters.VirtualScreenWidth + SystemParameters.VirtualScreenLeft)
			{
				window.Left = SystemParameters.VirtualScreenWidth + SystemParameters.VirtualScreenLeft - window.Width;
			}
			if (window.Top < SystemParameters.VirtualScreenTop)
			{
				window.Top = SystemParameters.VirtualScreenTop;
			}
			if (window.Left < SystemParameters.VirtualScreenLeft)
			{
				window.Left = SystemParameters.VirtualScreenLeft;
			}
		}
	}
}
