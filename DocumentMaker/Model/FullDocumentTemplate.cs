using Dml;
using Dml.Model.Template;
using DocumentMaker.Model.Back;
using DocumentMaker.Model.OfficeFiles;
using System.Collections.Generic;

namespace DocumentMaker.Model
{
	public class FullDocumentTemplate : DocumentTemplate
	{
		private readonly ObservableRangeCollection<WorkObject> workTypesList;

		public FullDocumentTemplate() : base()
		{
			workTypesList = new ObservableRangeCollection<WorkObject>();
		}

		public IList<WorkObject> WorkTypesList => workTypesList;

		public void LoadWorkTypesList(string fullpath)
		{
			XlsxLoader loader = new XlsxLoader();
			loader.LoadWorkTypes(fullpath, Type, workTypesList);
		}
	}
}
