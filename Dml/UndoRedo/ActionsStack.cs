﻿using System.Collections.Generic;

namespace Dml.UndoRedo
{
	public class ActionsStack : IUndoRedoActionsStack
	{
		private readonly LinkedList<IUndoRedoAction> redoList;
		private readonly LinkedList<IUndoRedoAction> undoList;

		private event UndoRedoActionPushedHandler onPushed;

		public ActionsStack()
		{
			redoList = new LinkedList<IUndoRedoAction>();
			undoList = new LinkedList<IUndoRedoAction>();
		}

		public bool ActionsStackingEnabled { get; set; }
		public bool CollapsingActionByTargetEnabled { get; set; } = true;
		public bool CanRedo => redoList.Count > 0;
		public bool CanUndo => undoList.Count > 0;
		public int MaxCapacity { get; } = 1000;

		public void Push(IUndoRedoAction action)
		{
			if (ActionsStackingEnabled)
			{
				redoList.Clear();

				while (redoList.Count + undoList.Count > MaxCapacity)
					undoList.RemoveFirst();

				if (CanUndo && CollapsingActionByTargetEnabled && action is ITargetValueUndoRedoAction newTargetValueAction && undoList.Last.Value is ITargetValueUndoRedoAction lastTargetValueAction && lastTargetValueAction.GetType() == newTargetValueAction.GetType()
					&& newTargetValueAction.GetTarget() == lastTargetValueAction.GetTarget() && lastTargetValueAction.GetNewValue() == newTargetValueAction.GetOldValue())
				{
					lastTargetValueAction.SetNewValue(newTargetValueAction.GetNewValue());
				}
				else
				{
					undoList.AddLast(action);
					onPushed?.Invoke(action);
				}
			}
		}

		public void Redo()
		{
			if (CanRedo)
			{
				IUndoRedoAction action = redoList.Last.Value;
				redoList.RemoveLast();
				action.Redo();
				undoList.AddLast(action);
			}
		}

		public void Undo()
		{
			if (CanUndo)
			{
				IUndoRedoAction action = undoList.Last.Value;
				undoList.RemoveLast();
				action.Undo();
				redoList.AddLast(action);
			}
		}

		public void AddLinkToLast(IUndoRedoAction action)
		{
			undoList.Last.Value.AddLink(action);
		}

		public void RemoveActionsWithTarget<TObj>(TObj target)
		{
			RemoveActionsWithTarget(target, redoList);
			RemoveActionsWithTarget(target, undoList);
		}

		public void SubscribePushed(UndoRedoActionPushedHandler action)
		{
			onPushed += action;
		}

		public void Clear()
		{
			redoList.Clear();
			undoList.Clear();
		}

		private void RemoveActionsWithTarget<TObj>(TObj target, LinkedList<IUndoRedoAction> list)
		{
			List<ITargetUndoRedoAction<TObj>> removeList = new List<ITargetUndoRedoAction<TObj>>();
			foreach (IUndoRedoAction undoRedoAction in list)
			{
				if (undoRedoAction is ITargetUndoRedoAction<TObj> targetAction && ReferenceEquals(targetAction.Target, target))
				{
					removeList.Add(targetAction);
				}
			}
			foreach (IUndoRedoAction removeItem in removeList)
			{
				list.Remove(removeItem);
			}
		}
	}
}
