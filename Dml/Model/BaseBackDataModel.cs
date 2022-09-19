using Dml.Model.Back;
using Dml.Model.Session.Attributes;
using Dml.UndoRedo;

namespace Dml.Model
{
	public abstract class BaseBackDataModel
	{
		private readonly ObservableRangeCollection<BackDataType> backDataTypesList;
		private readonly ObservableRangeCollection<GameObject> gameNameList;
		protected IUndoRedoActionsStack actionsStack = null;

		public BaseBackDataModel()
		{
			backDataTypesList = new ObservableRangeCollection<BackDataType>();
			gameNameList = new ObservableRangeCollection<GameObject>();
		}

		public uint Id { get; set; }
		public BackType Type { get; set; }
		public string EpisodeNumberText { get; set; }
		public string BackNumberText { get; set; }
		public string BackName { get; set; }
		public string BackCountRegionsText { get; set; }
		public ObservableRangeCollection<GameObject> GameNameList => gameNameList;
		public string GameName { get; set; }
		public bool IsRework { get; set; }
		public bool IsSketch { get; set; }
		public string SpentTimeText { get; set; }
		public string OtherText { get; set; }
		public ObservableRangeCollection<BackDataType> BackDataTypesList => backDataTypesList;
		public bool IsActionsStackingEnabled => actionsStack?.ActionsStackingEnabled ?? false;

		[IsNotSavingContent]
		public bool HaveUnsavedChanges { get; set; }

		public virtual void SetActionsStack(IUndoRedoActionsStack actionsStack)
		{
			this.actionsStack = actionsStack;
		}

		public virtual void AddUndoRedoLink(IUndoRedoAction action)
		{
			actionsStack.AddLinkToLast(action);
		}
	}
}
