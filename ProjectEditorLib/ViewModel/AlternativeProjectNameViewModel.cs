using Db.Context;
using Mvvm.Commands;
using ProjectEditorLib.View;
using System.Windows;

namespace ProjectEditorLib.ViewModel
{
	public class AlternativeProjectNameViewModel : BaseDbObjectViewModel, IDbObjectViewModel
	{
		public AlternativeProjectNameViewModel() : base() { }

		#region Properties

		public string AltProjectName
		{
			get { return (string)GetValue(AltProjectNameProperty); }
			set { SetValue(AltProjectNameProperty, value); }
		}
		public static readonly DependencyProperty AltProjectNameProperty = DependencyProperty.Register(nameof(AltProjectName), typeof(string), typeof(AlternativeProjectNameViewModel));

		#endregion

		#region Commands

		protected override void InitCommands()
		{
			base.InitCommands();

			DeleteCommand = new Command<AlternativeProjectNameView>(OnDeleteCommandExecute);
		}

		public Command<AlternativeProjectNameView> DeleteCommand { get; private set; }
		private void OnDeleteCommandExecute(AlternativeProjectNameView view)
		{

		}

		#endregion

		#region Methods

		public override void SetFromContext(IDbObject dbObject)
		{
			throw new System.NotImplementedException();
		}

		public override IDbObject UpdateContext(IDbObject dbObject)
		{
			throw new System.NotImplementedException();
		}

		#endregion
	}
}
