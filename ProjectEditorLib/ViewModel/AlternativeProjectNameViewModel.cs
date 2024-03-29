﻿using ProjectsDb.Context;
using Mvvm.Commands;
using ProjectEditorLib.View;
using System.Windows;

namespace ProjectEditorLib.ViewModel
{
	public class AlternativeProjectNameViewModel : BaseDbObjectViewModel, IDbObjectViewModel
	{
		int model = 0;

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

		public Command<AlternativeProjectNameView> DeleteAltProjectName { get; set; } = null;
		public Command<AlternativeProjectNameView> DeleteCommand { get; private set; }
		private void OnDeleteCommandExecute(AlternativeProjectNameView view)
		{
			DeleteAltProjectName.Execute(view);
		}

		#endregion

		#region Methods

		public override void SetFromContext(IDbObject dbObject)
		{
			if(dbObject is AlternativeProjectName alternativeName)
			{
				AltProjectName = alternativeName.Name;
				model = alternativeName.Id;
				context = alternativeName;
			}
			else
			{
				AltProjectName = string.Empty;
				context = null;
			}
		}

		public override IDbObject UpdateContext(IDbObject dbObject)
		{
			throw new System.NotImplementedException();
		}

		public int GetModel()
		{
			return model;
		}

		#endregion
	}
}
