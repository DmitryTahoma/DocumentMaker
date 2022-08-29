using Dml;
using Dml.Model;
using DocumentMaker.Model.Back;

namespace DocumentMaker.Model.Controls
{
	public class FullBackDataModel : BaseBackDataModel
	{
		public FullBackDataModel() : base()
		{
			WorkTypesList = new ObservableRangeCollection<WorkObject>();
		}

		public string WeightText { get; set; }
		public double Weight { get; set; }
		public string SumText { get; set; }
		public uint WorkObjectId { get; set; }
		public bool IsOtherType { get; set; }
		public ObservableRangeCollection<WorkObject> WorkTypesList { get; private set; }
	}
}
