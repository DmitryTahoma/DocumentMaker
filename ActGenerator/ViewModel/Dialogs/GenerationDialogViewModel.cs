using Mvvm.Commands;
using System.Windows;
using System.Windows.Forms;

namespace ActGenerator.ViewModel.Dialogs
{
	public class GenerationDialogViewModel : DependencyObject
	{
		const string defaultStateText = "Готовий до генерації";

		FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();

		public GenerationDialogViewModel()
		{
			InitCommands();
		}

		#region Properties

		public string LabelText
		{
			get { return (string)GetValue(LabelTextProperty); }
			set { SetValue(LabelTextProperty, value); }
		}
		public static readonly DependencyProperty LabelTextProperty = DependencyProperty.Register(nameof(LabelText), typeof(string), typeof(GenerationDialogViewModel), new PropertyMetadata(string.Empty));

		public double ProgressValue
		{
			get { return (double)GetValue(ProgressValueProperty); }
			set { SetValue(ProgressValueProperty, value); }
		}
		public static readonly DependencyProperty ProgressValueProperty = DependencyProperty.Register(nameof(ProgressValue), typeof(double), typeof(GenerationDialogViewModel));

		public double ProgressMaximum
		{
			get { return (double)GetValue(ProgressMaximumProperty); }
			set { SetValue(ProgressMaximumProperty, value); }
		}
		public static readonly DependencyProperty ProgressMaximumProperty = DependencyProperty.Register(nameof(ProgressMaximum), typeof(double), typeof(GenerationDialogViewModel));

		public string SelectedFolderPath
		{
			get { return (string)GetValue(SelectedFolderPathProperty); }
			set { SetValue(SelectedFolderPathProperty, value); }
		}
		public static readonly DependencyProperty SelectedFolderPathProperty = DependencyProperty.Register(nameof(SelectedFolderPath), typeof(string), typeof(GenerationDialogViewModel));

		public bool FolderSelected
		{
			get { return (bool)GetValue(FolderSelectedProperty); }
			private set { SetValue(FolderSelectedProperty, value); }
		}
		public static readonly DependencyProperty FolderSelectedProperty = DependencyProperty.Register(nameof(FolderSelected), typeof(bool), typeof(GenerationDialogViewModel));

		public bool GenerationStarted
		{
			get { return (bool)GetValue(GenerationStartedProperty); }
			private set { SetValue(GenerationStartedProperty, value); }
		}
		public static readonly DependencyProperty GenerationStartedProperty = DependencyProperty.Register(nameof(GenerationStarted), typeof(bool), typeof(GenerationDialogViewModel));

		#endregion

		#region Commands

		private void InitCommands()
		{
			DialogLoaded = new Command(OnDialogLoadedExecute);
			SelectFolder = new Command(OnSelectFolderExecute, CanExecuteSelectFolder);
		}

		public Command DialogLoaded { get; private set; }
		private void OnDialogLoadedExecute()
		{
			LabelText = defaultStateText;
			ProgressValue = 0;
			ProgressMaximum = 1000;
			SelectedFolderPath = string.Empty;
			FolderSelected = false;
			GenerationStarted = false;
		}

		public Command SelectFolder { get; private set; }
		private void OnSelectFolderExecute()
		{
			if(folderBrowserDialog.ShowDialog() == DialogResult.OK)
			{
				FolderSelected = true;
				SelectedFolderPath = folderBrowserDialog.SelectedPath;
			}
		}

		#endregion

		#region Methods

		private bool CanExecuteSelectFolder()
		{
			return !GenerationStarted;
		}

		#endregion
	}
}
