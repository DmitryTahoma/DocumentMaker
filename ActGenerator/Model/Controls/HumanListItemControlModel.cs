using DocumentMakerModelLibrary;
using DocumentMakerModelLibrary.OfficeFiles.Human;
using System.Collections.Generic;

namespace ActGenerator.Model.Controls
{
	class HumanListItemControlModel
	{
		public HumanData HumanData { get; set; }
		public int? Sum { get; set; }
		public List<FullDocumentTemplate> SelectedTemplates { get; set; }
	}
}
