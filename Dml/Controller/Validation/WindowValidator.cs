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

		public static void MoveToPrimaryScreenCenterPosition(Window window)
		{
			window.Top = SystemParameters.PrimaryScreenHeight / 2 - window.Height / 2;
			window.Left = SystemParameters.PrimaryScreenWidth / 2 - window.Width / 2;
		}
	}
}
