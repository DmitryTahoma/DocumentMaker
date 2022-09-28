using Db.Context.HumanPart;
using System.Windows;

namespace HumanEditorLib.ViewModel
{
	public class StreetControlViewModel : DependencyObject
	{
		StreetType model = null;

		public StreetControlViewModel()
		{

		}

		#region Properties

		public string Name
		{
			get => (string)GetValue(NameProperty);
			set => SetValue(NameProperty, value);
		}
		public static readonly DependencyProperty NameProperty = DependencyProperty.Register(nameof(Name), typeof(string), typeof(StreetControlViewModel));

		public string ShortName
		{
			get => (string)GetValue(ShortNameProperty);
			set => SetValue(ShortNameProperty, value);
		}
		public static readonly DependencyProperty ShortNameProperty = DependencyProperty.Register(nameof(ShortName), typeof(string), typeof(StreetControlViewModel));

		#endregion

		public void SetFromDatabase(StreetType streetType)
		{
			model = streetType;
			Name = streetType.Name;
			ShortName = streetType.ShortName;
		}
	}
}
