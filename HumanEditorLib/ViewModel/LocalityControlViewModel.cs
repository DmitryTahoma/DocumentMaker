using Db.Context.HumanPart;
using HumanEditorLib.View;
using Mvvm.Commands;
using System.Windows;

namespace HumanEditorLib.ViewModel
{
	public class LocalityControlViewModel : DependencyObject
	{
		LocalityType model = null;

		public LocalityControlViewModel()
		{
			DeleteCommand = new Command<LocalityControl>(OnDeleteCommandExecute);
		}

		#region Properties

		public string Name
		{
			get => (string)GetValue(NameProperty);
			set => SetValue(NameProperty, value);
		}
		public static readonly DependencyProperty NameProperty = DependencyProperty.Register(nameof(Name), typeof(string), typeof(LocalityControlViewModel));

		public string ShortName
		{
			get => (string)GetValue(ShortNameProperty);
			set => SetValue(ShortNameProperty, value);
		}
		public static readonly DependencyProperty ShortNameProperty = DependencyProperty.Register(nameof(ShortName), typeof(string), typeof(LocalityControlViewModel));

		#endregion

		#region Commands

		public Command<LocalityControl> DeleteLocalityType { get; set; } = null;
		public Command<LocalityControl> DeleteCommand { get; private set; }
		private void OnDeleteCommandExecute(LocalityControl localityControl)
		{
			DeleteLocalityType.Execute(localityControl);
		}

		#endregion

		#region Methods

		public void SetModel(LocalityType localityType)
		{
			model = localityType;
			Name = localityType.Name;
			ShortName = localityType.ShortName;
		}

		public LocalityType GetModel()
		{
			model.Name = Name;
			model.ShortName = ShortName;
			return model;
		}

		#endregion
	}
}
