using ActCreator.Controller;
using Dml.Controller;
using Dml.Controls;
using Dml.Model.Template;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.Forms.MessageBox;

namespace ActCreator
{
	public delegate void ActionWithBackData(BackData backData);

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private readonly MainWindowController controller;
		private readonly FolderBrowserDialog folderBrowserDialog;

		public MainWindow()
		{
			controller = new MainWindowController();

			InitializeComponent();

			DataFooter.SubscribeAddition((x) =>
			{
				controller.BackDataControllers.Add(x.Controller);
				x.SetViewByTemplate(controller.TemplateType);
			});
			DataFooter.SubscribeRemoving((x) =>
			{
				controller.BackDataControllers.Remove(x.Controller);
			});
			DataFooter.SubscribeClearing(() =>
			{
				controller.BackDataControllers.Clear();
			});

			folderBrowserDialog = new FolderBrowserDialog();
		}

		public IList<DocumentTemplate> DocumentTemplatesList => controller.DocumentTemplatesList;

		private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			controller.Save();
		}

		private void WindowLoaded(object sender, RoutedEventArgs e)
		{
			if (controller != null)
			{
				controller.Load();

				DocumentTemplateComboBox.SelectedIndex = (int)controller.TemplateType;

				foreach (BackDataController backDataController in controller.BackDataControllers)
				{
					DataFooter.AddLoadedBackData(backDataController);
				}

				UpdateViewBackData();
			}
		}

		private void ChangedDocumentTemplate(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			if (controller != null
				&& sender is System.Windows.Controls.ComboBox comboBox
				&& comboBox.SelectedItem is DocumentTemplate documentTemplate)
			{
				controller.TemplateType = documentTemplate.Type;
				UpdateViewBackData();
			}
		}

		private void ExportBtnClick(object sender, RoutedEventArgs e)
		{
			if (controller.Validate(out string errorText))
			{
				if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{

				}
			}
			else
			{
				MessageBox.Show(errorText, "ActCreator | Validation", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void UpdateViewBackData()
		{
			foreach (UIElement control in BacksData.Children)
			{
				if (control is BackData backData)
				{
					backData.SetViewByTemplate(controller.TemplateType);
				}
			}
			DataFooter.SetViewByTemplate(controller.TemplateType);
			DataHeader.SetViewByTemplate(controller.TemplateType);
		}
	}
}
