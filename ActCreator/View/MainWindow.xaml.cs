using ActCreator.Controller;
using Dml.Controller;
using Dml.Controls;
using Dml.Model.Template;
using System.Collections.Generic;
using System.Diagnostics;
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
		public static readonly DependencyProperty SelectedHumanProperty;

		static MainWindow()
		{
			SelectedHumanProperty = DependencyProperty.Register("SelectedHuman", typeof(string), typeof(ComboBox));
		}

		private readonly MainWindowController controller;
		private readonly FolderBrowserDialog folderBrowserDialog;

		public MainWindow()
		{
			controller = new MainWindowController();
			controller.Load();
			SetWindowSettingsFromController();

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

		public string SelectedHuman
		{
			get => (string)GetValue(SelectedHumanProperty);
			set
			{
				SetValue(SelectedHumanProperty, value);
				controller.SelectedHuman = value;
			}
		}

		public IList<string> HumanFullNameList => controller.HumanFullNameList;

		private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			controller.WindowTop = Top;
			controller.WindowLeft = Left;
			controller.WindowHeight = Height;
			controller.WindowWidth = Width;
			controller.WindowState = WindowState == WindowState.Minimized ? WindowState.Normal : WindowState;

			controller.Save();
		}

		private void WindowLoaded(object sender, RoutedEventArgs e)
		{
			if (controller != null)
			{
				DocumentTemplateComboBox.SelectedIndex = (int)controller.TemplateType;
				HumanFullNameComboBox.Text = controller.SelectedHuman;

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
					bool existsFile = controller.DmxExists(folderBrowserDialog.SelectedPath);
					if (!existsFile || (existsFile &&
							MessageBox.Show("Файл вже існує. Ви впевнені, що хочете замінити його?",
							"ActCreator | Export",
							MessageBoxButtons.YesNo,
							MessageBoxIcon.Question)
								== System.Windows.Forms.DialogResult.Yes))
					{
						controller.ExportDmx(folderBrowserDialog.SelectedPath);

						if (MessageBox.Show("Файл збережений.\nВідкрити папку з файлом?",
											"ActCreator | Export",
											MessageBoxButtons.YesNo,
											MessageBoxIcon.Information,
											MessageBoxDefaultButton.Button2)
							== System.Windows.Forms.DialogResult.Yes)
						{
							Process.Start("explorer", folderBrowserDialog.SelectedPath);
						}
					}

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

		private void SetWindowSettingsFromController()
		{
			Top = controller.WindowTop;
			Left = controller.WindowLeft;
			Height = controller.WindowHeight;
			Width = controller.WindowWidth;
			WindowState = controller.WindowState;
		}
	}
}
