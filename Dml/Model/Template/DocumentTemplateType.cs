namespace Dml.Model.Template
{
	public enum DocumentTemplateType
	{
		Empty = -1,
		Scripter,
		Cutter,
		Painter,
		Modeller,
		Tester,
		Programmer,
		Soundman,
		Animator,
		Translator,
		Support,
	}

	public enum DocumentType
	{
		Empty = -1,
		FOP,
		FOPF,
		GIG,
		Staff,
	}

	public enum WorkType
	{
		Empty = -1,
		Development,
		Rework,
		All,
	}

	public enum FillSheetType
	{
		Empty = -1,
		Table,
		List,
		ListFrol,
	}
}
