using Db.Context.HumanPart;
using HumanEditorLib.View;
using Mvvm.Commands;
using System.Windows;

namespace HumanEditorLib.ViewModel
{
	public class StreetControlViewModel : DependencyObject
	{
		StreetType model = null;

		public StreetControlViewModel()
		{
			DeleteCommand = new Command<StreetControl>(OnDeleteCommandExecute);
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

		#region Commands

		public Command<StreetControl> DeleteStreetType { get; set; } = null;
		public Command<StreetControl> DeleteCommand { get; private set; }
		private void OnDeleteCommandExecute(StreetControl streetControl)
		{
			DeleteStreetType.Execute(streetControl);
		}

		#endregion

		public void SetModel(StreetType streetType)
		{
			model = streetType;
			Name = streetType.Name;
			ShortName = streetType.ShortName;
		}

		public StreetType GetModel()
		{
			model.Name = Name;
			model.ShortName = ShortName;
			return model;
		}
	}
}
