using ActGenerator.Model;
using Dml.Model.Template;
using Mvvm;

namespace ActGenerator.ViewModel
{
	class ActGeneratorViewModel
	{
		public ActGeneratorViewModel()
		{
			ProjectsList = new ObservableRangeCollection<string>
			{
				"Lost Lands 8",
				"Tricky doors",
				"Lost Lands Stories",
				"Legendary Tales 3",
			};
			HumanList = new ObservableRangeCollection<HumanDataContext>
			{
				new HumanDataContext("Алєйникова Марина"),
				new HumanDataContext("Баєв Олександр"),
				new HumanDataContext("Байдуж Максим"),
				new HumanDataContext("Байдуж Марина"),
			};
		}

		#region Properties

		public ObservableRangeCollection<string> ProjectsList { get; private set; } = new ObservableRangeCollection<string>();

		public ObservableRangeCollection<HumanDataContext> HumanList { get; private set; } = new ObservableRangeCollection<HumanDataContext>();

		public ObservableRangeCollection<DocumentTemplate> DocumentTemplates { get; private set; } = new ObservableRangeCollection<DocumentTemplate> 
		{
			new DocumentTemplate("Скриптувальник", DocumentTemplateType.Scripter),
			new DocumentTemplate("Технічний дизайнер", DocumentTemplateType.Cutter),
			new DocumentTemplate("Художник", DocumentTemplateType.Painter),
			new DocumentTemplate("Моделлер", DocumentTemplateType.Modeller),
		};

		#endregion
	}
}
