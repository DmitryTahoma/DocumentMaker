using Dml;
using Dml.Model.Template;

namespace ActGenerator.ViewModel.Controls
{
	public class HumenListControlViewModel
	{
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

		#endregion
	}
}
