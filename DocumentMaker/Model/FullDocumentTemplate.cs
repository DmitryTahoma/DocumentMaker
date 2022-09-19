using Dml;
using Dml.Model.Template;
using DocumentMaker.Model.Back;
using DocumentMaker.Model.OfficeFiles;
using System.Collections.Generic;

namespace DocumentMaker.Model
{
	public class FullDocumentTemplate : DocumentTemplate
	{
		private readonly ObservableRangeCollection<WorkObject> workTypesList, reworkWorkTypesList;

		public FullDocumentTemplate(string name, DocumentTemplateType type) : base(name, type)
		{
			workTypesList = new ObservableRangeCollection<WorkObject>();
			reworkWorkTypesList = new ObservableRangeCollection<WorkObject>();
		}

		public IList<WorkObject> WorkTypesList => workTypesList;
		public IList<WorkObject> ReworkWorkTypesList => reworkWorkTypesList;

		public void LoadWorkTypesList(string fullpath)
		{
			XlsxLoader loader = new XlsxLoader();
			loader.LoadWorkTypes(fullpath, Type, workTypesList);
		}

		public void LoadReworkWorkTypesList(string fullpath)
		{
			XlsxLoader loader = new XlsxLoader();
			loader.LoadWorkTypes(fullpath, Type, reworkWorkTypesList);
		}
	}
}
