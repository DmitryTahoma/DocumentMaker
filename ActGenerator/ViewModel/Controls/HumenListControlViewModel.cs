using ActGenerator.View.Dialogs;
using ActGenerator.ViewModel.Interfaces;
using Dml;
using Dml.Model.Template;
using MaterialDesignThemes.Wpf;
using Mvvm.Commands;

namespace ActGenerator.ViewModel.Controls
{
	public class HumenListControlViewModel : IContainDialogHostId
	{
		AddHumanDialog addHumanDialog = new AddHumanDialog();

		public HumenListControlViewModel()
		{
			InitCommands();
		}

		#region Properties

		public ObservableRangeCollection<DocumentTemplate> DocumentTemplatesList => new ObservableRangeCollection<DocumentTemplate>
			{
				new DocumentTemplate("Скриптувальник", DocumentTemplateType.Scripter),
				new DocumentTemplate("Технічний дизайнер", DocumentTemplateType.Cutter),
				new DocumentTemplate("Художник", DocumentTemplateType.Painter),
				new DocumentTemplate("Моделлер", DocumentTemplateType.Modeller),
				new DocumentTemplate("Тестувальник", DocumentTemplateType.Tester),
				new DocumentTemplate("Програміст", DocumentTemplateType.Programmer),
				new DocumentTemplate("Звукорежисер", DocumentTemplateType.Soundman),
				new DocumentTemplate("Аніматор", DocumentTemplateType.Animator),
				new DocumentTemplate("Перекладач", DocumentTemplateType.Translator),
				new DocumentTemplate("Підтримка", DocumentTemplateType.Support),
			};

		public string DialogHostId { get; set; } = null;

		#endregion

		#region Commands

		private void InitCommands()
		{
			AddHumanCommand = new Command(OnAddHumanCommandExecute);
		}

		public Command AddHumanCommand { get; private set; }
		private async void OnAddHumanCommandExecute()
		{
			await DialogHost.Show(addHumanDialog, DialogHostId);
		}

		#endregion
	}
}
