﻿using Dml.Controller.Validation;
using MaterialDesignThemes.Wpf;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DocumentMaker.View.Dialogs
{
	/// <summary>
	/// Interaction logic for CorrectSupportDialog.xaml
	/// </summary>
	public partial class CorrectSupportDialog : UserControl
	{
		private readonly InputingValidator inputingValidator;

		public CorrectSupportDialog()
		{
			InitializeComponent();
			DataContext = this;

			inputingValidator = new InputingValidator();
			SumInput.CommandBindings.Add(new CommandBinding(ApplicationCommands.Paste, inputingValidator.BlockingCommand));
		}

		public bool IsCorrection { get; private set; }
		public string NumberText { get; set; }
		public bool TakeSumFromDevelopment { get; set; }

		private void ControlKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Escape)
			{
				DialogHost.CloseDialogCommand.Execute(null, null);
			}
		}

		private void CorrectionClick(object sender, RoutedEventArgs e)
		{
			IsCorrection = true;
			DialogHost.CloseDialogCommand.Execute(null, null);
		}

		private void UIntValidating(object sender, System.Windows.Input.TextCompositionEventArgs e)
		{
			inputingValidator.UIntInputing_PreviewTextInput(sender, e);
		}
	}
}