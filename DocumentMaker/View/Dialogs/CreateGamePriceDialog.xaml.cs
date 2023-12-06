using MaterialDesignThemes.Wpf;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DocumentMakerModelLibrary;
using DocumentMakerModelLibrary.Files;
using DocumentMakerModelLibrary.Algorithm;
using DocumentMakerModelLibrary.Controls;
using Dml.Model.Back;
using System.Windows.Forms;
using MessageBox = System.Windows.Forms.MessageBox;
using DocumentMakerModelLibrary.OfficeFiles;

namespace DocumentMaker.View.Dialogs
{
	/// <summary>
	/// Логика взаимодействия для CreateGamePriceDialog.xaml
	/// </summary>
	public partial class CreateGamePriceDialog : System.Windows.Controls.UserControl
	{
		private string[] aNameFillSheet = { "Таблиця", "Список", "Строка" };
		private IList<DmxFile> openedFilesList;
		private IList<GameObject> gameNameList;
		private string sActDate = "";

		private readonly SaveFileDialog saveFileSumDialog = new SaveFileDialog
		{
			DefaultExt = ".xlsx",
			Filter = "Файл (*.xlsx)|"
		};

		static CreateGamePriceDialog()
		{
			
		}

		public CreateGamePriceDialog(IList<DmxFile> _openedFilesList, IList<GameObject> _gameObjects, string _sActDate)
		{
			InitializeComponent();
			DataContext = this;
			openedFilesList = _openedFilesList;
			gameNameList = _gameObjects;
			sActDate = _sActDate;

			DevelopmentTypeComboBox.ItemsSource = aNameFillSheet;
			DevelopmentTypeComboBox.SelectedItem = aNameFillSheet[0];

			ReworkTypeComboBox.ItemsSource = aNameFillSheet;
			ReworkTypeComboBox.SelectedItem = aNameFillSheet[1];

			WorkAllTypeComboBox.ItemsSource = aNameFillSheet;
			WorkAllTypeComboBox.SelectedItem = aNameFillSheet[0];

			IsDevelopmentCheckBox.IsChecked = true;
			IsReWorkCheckBox.IsChecked = true;
			IsWorkAllCheckBox.IsChecked = false;

			DevelopmentTypeComboBox.IsEnabled = (bool)IsDevelopmentCheckBox.IsChecked;
			ReworkTypeComboBox.IsEnabled = (bool)IsReWorkCheckBox.IsChecked;
			WorkAllTypeComboBox.IsEnabled = (bool)IsWorkAllCheckBox.IsChecked;

			GenerateFileBtt.IsEnabled = DevelopmentTypeComboBox.IsEnabled || ReworkTypeComboBox.IsEnabled || WorkAllTypeComboBox.IsEnabled;
		}

		private void ExportGameSum(object sender, RoutedEventArgs e)
		{
			try
			{
				saveFileSumDialog.FileName = "Сума iгор " + sActDate + ".xlsx";

				if (saveFileSumDialog.ShowDialog() == DialogResult.OK)
				{
					Dictionary<string, int> dFillSheet = new Dictionary<string, int>();
					if(DevelopmentTypeComboBox.IsEnabled)
						dFillSheet.Add("Розробка", DevelopmentTypeComboBox.SelectedIndex);
					else
						dFillSheet.Add("Розробка", -1);

					if (ReworkTypeComboBox.IsEnabled)
						dFillSheet.Add("Пiдтримка", ReworkTypeComboBox.SelectedIndex);
					else
						dFillSheet.Add("Пiдтримка", -1);

					if (WorkAllTypeComboBox.IsEnabled)
						dFillSheet.Add("Разом", WorkAllTypeComboBox.SelectedIndex);
					else
						dFillSheet.Add("Разом", -1);

					string path = saveFileSumDialog.FileName;
					XlsxCreateGamePrice createXlsx = new XlsxCreateGamePrice();
					createXlsx.CreateXlsxXml(path, openedFilesList, gameNameList, dFillSheet);
				}
			}
			catch (System.Exception exc)
			{
				MessageBox.Show("Виникла непередбачена помилка під час експорту! Надішліть, будь ласка, скріншот помилки розробнику.\n" + exc.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void ControlKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (e.Key == Key.Escape)
			{
				DialogHost.CloseDialogCommand.Execute(null, null);
			}
		}

		private void UnfocusOnEnter(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				Keyboard.ClearFocus();
			}
		}

		private void ChangedDevelopmentCheckBox(object sender, RoutedEventArgs e)
		{
			DevelopmentTypeComboBox.IsEnabled = (bool)IsDevelopmentCheckBox.IsChecked;
			GenerateFileBtt.IsEnabled = DevelopmentTypeComboBox.IsEnabled || ReworkTypeComboBox.IsEnabled || WorkAllTypeComboBox.IsEnabled;
		}

		private void ChangedReWorkCheckBox(object sender, RoutedEventArgs e)
		{
			ReworkTypeComboBox.IsEnabled = (bool)IsReWorkCheckBox.IsChecked;
			GenerateFileBtt.IsEnabled = DevelopmentTypeComboBox.IsEnabled || ReworkTypeComboBox.IsEnabled || WorkAllTypeComboBox.IsEnabled;
		}

		private void ChangedWorkAllCheckBox(object sender, RoutedEventArgs e)
		{
			WorkAllTypeComboBox.IsEnabled = (bool)IsWorkAllCheckBox.IsChecked;
			GenerateFileBtt.IsEnabled = DevelopmentTypeComboBox.IsEnabled || ReworkTypeComboBox.IsEnabled || WorkAllTypeComboBox.IsEnabled;
		}
	}
}
