using ActCreator.Controller;
using ActCreator.Controller.Controls;
using ActCreator.View.Controls;
using Dml.Controller.Validation;
using Dml.Model.Template;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.Forms.MessageBox;

#if INCLUDED_UPDATER_API
using UpdaterAPI;
using UpdaterAPI.Resources;
#endif

namespace ActCreator
{
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
		private readonly SaveFileDialog saveFileDialog;

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
				x.SetGameNameList(controller.GameNameList);
			});
			DataFooter.SubscribeRemoving((x) =>
			{
				controller.BackDataControllers.Remove(x.Controller);
				DataFooter.UpdateBackDataIds();
			});
			DataFooter.SubscribeClearing(() =>
			{
				controller.BackDataControllers.Clear();
			});

			saveFileDialog = new SaveFileDialog
			{
				DefaultExt = Dml.Model.Files.BaseDmxFile.Extension,
				Filter = "Файл акту (*" + Dml.Model.Files.BaseDmxFile.Extension + ")|*" + Dml.Model.Files.BaseDmxFile.Extension
			};
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

				foreach (ShortBackDataController backDataController in controller.BackDataControllers)
				{
					DataFooter.AddLoadedBackData(backDataController);
					DataFooter.UpdateBackDataIds();
				}

				UpdateViewBackData();

#if INCLUDED_UPDATER_API
				AssemblyLoader.LoadWinScp();
				try
				{
					bool _ = false;
					UpdateInformer informer = new UpdateInformer();
					informer.Notify(ref _);
				}
				catch
				{
					MessageBox.Show("Невозможно подключиться к шаре!", "ActCreator Update", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
				}
#endif
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
				saveFileDialog.FileName = controller.GetDmxFileName();
				if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					controller.ExportDmx(saveFileDialog.FileName);

					if (MessageBox.Show("Файл збережений.\nВідкрити папку з файлом?",
										"ActCreator | Export",
										MessageBoxButtons.YesNo,
										MessageBoxIcon.Information,
										MessageBoxDefaultButton.Button2)
						== System.Windows.Forms.DialogResult.Yes)
					{
						Process.Start("explorer", Path.GetDirectoryName(saveFileDialog.FileName));
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
				if (control is ShortBackData backData)
				{
					backData.SetViewByTemplate(controller.TemplateType);
					backData.SetGameNameList(controller.GameNameList);
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
			WindowValidator.MoveToValidPosition(this);
		}
	}
}
