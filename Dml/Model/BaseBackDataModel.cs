using Dml.Model.Back;
using Dml.Model.Session.Attributes;
using Dml.UndoRedo;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Dml.Model
{
	public abstract class BaseBackDataModel
	{
		private readonly ObservableCollection<BackDataType> dataTypesList;
		private readonly ObservableRangeCollection<GameObject> gameNameList;
		protected IUndoRedoActionsStack actionsStack = null;

		public BaseBackDataModel()
		{
			dataTypesList = new ObservableCollection<BackDataType>
			{
				new BackDataType { Name = "Бек", Type = BackType.Back },
				new BackDataType { Name = "Регіони", Type = BackType.Regions },
				new BackDataType { Name = "Діалог", Type = BackType.Dialog },
				new BackDataType { Name = "Мініігра", Type = BackType.Mg },
				new BackDataType { Name = "Хог", Type = BackType.Hog },
				new BackDataType { Name = "Хог регіони", Type = BackType.HogRegions },
				new BackDataType { Name = "Крафт", Type = BackType.Craft },
				new BackDataType { Name = "Інше", Type = BackType.Other },
			};
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
		public IList<BackDataType> BackDataTypesList => dataTypesList;
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
