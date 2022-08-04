namespace Dml.Model.Template
{
	public class DocumentTemplate
	{
		public DocumentTemplate()
		{
			Name = "";
			Type = DocumentTemplateType.Empty;
		}

		public string Name { get; set; }
		public DocumentTemplateType Type { get; set; }

		public override string ToString()
		{
			return Name;
		}
	}
}
