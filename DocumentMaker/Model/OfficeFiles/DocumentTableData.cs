using System.Collections;
using System.Collections.Generic;

namespace DocumentMaker.Model.OfficeFiles
{
    class DocumentTableData : IEnumerable<DocumentTableRowData>
    {
        private List<DocumentTableRowData> rows;

        public DocumentTableData(IEnumerable<BackDataModel> backModels)
        {
            rows = new List<DocumentTableRowData>();

            foreach (BackDataModel model in backModels)
            {
                rows.Add(new DocumentTableRowData(model));
            }
        }

        public IEnumerator<DocumentTableRowData> GetEnumerator()
        {
            return rows.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return rows.GetEnumerator();
        }
    }
}
