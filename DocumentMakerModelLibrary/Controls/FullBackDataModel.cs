﻿using Dml;
using Dml.Model;
using Dml.Model.Back;
using Dml.Model.Template;
using Dml.UndoRedo;
using DocumentMakerModelLibrary.Back;

namespace DocumentMakerModelLibrary.Controls
{
	public class FullBackDataModel : BaseBackDataModel
	{
		private readonly PropertySetActionProvider<FullBackDataModel, string> sumTextActionProvider = new PropertySetActionProvider<FullBackDataModel, string>(x => x.SumText);

		private string sumText;

		public FullBackDataModel() : base()
		{
			WorkTypesList = new ObservableRangeCollection<WorkObject>();
		}

		public FullBackDataModel(FullBackDataModel obj, WorkObject work) : this()
		{
			Id = obj.Id;
			Type = obj.Type;
			EpisodeNumberText = obj.EpisodeNumberText;
			BackNumberText = obj.BackNumberText;
			BackName = obj.BackName;
			BackCountRegionsText = obj.BackCountRegionsText;
			GameName = obj.GameName;
			IsRework = obj.IsRework;
			IsSketch = obj.IsSketch;
			SpentTimeText = obj.SpentTimeText;
			OtherText = obj.OtherText;
			actionsStack = obj.actionsStack;
			HaveUnsavedChanges = true;
			WeightText = obj.WeightText;
			Weight = obj.Weight;
			sumText = "0";
			WorkObjectId = work.Id;
			IsOtherType = obj.IsOtherType;
		}

		public string WeightText { get; set; }
		public double Weight { get; set; }
		public string SumText
		{
			get => sumText;
			set
			{
				if (actionsStack?.ActionsStackingEnabled ?? false) actionsStack.Push(sumTextActionProvider.CreateAction(this, value));
				sumText = value;
			}
		}
		public uint WorkObjectId { get; set; }
		public bool IsOtherType { get; set; }
		public ObservableRangeCollection<WorkObject> WorkTypesList { get; private set; }

		public bool EqualsWithoutWork(FullBackDataModel obj, DocumentTemplateType documentTemplate)
		{
			return Type == obj.Type &&
				(
					(
						Type != BackType.Other &&
						EpisodeNumberText == obj.EpisodeNumberText &&
(
							Type == BackType.Predmets
							|| Type == BackType.Morf
							|| Type == BackType.Collection
							|| BackName == obj.BackName
						)
						&& GameName == obj.GameName &&
						IsRework == obj.IsRework &&
						(
							documentTemplate != DocumentTemplateType.Painter ||
							IsSketch == obj.IsSketch
						) &&
						(
							(
								Type != BackType.Regions &&
								Type != BackType.HogRegions
							) ||
							BackCountRegionsText == obj.BackCountRegionsText
						) &&
						(
							Type == BackType.Craft 
							|| Type == BackType.Predmets
							|| Type == BackType.Predmet
							|| Type == BackType.Morf
							|| Type == BackType.Collection
							|| Type == BackType.Character
							|| Type == BackType.Interface
							|| Type == BackType.Marketing
							|| Type == BackType.VideoCadr
							|| Type == BackType.VideoObject
							|| Type == BackType.VideoPredmet
							|| BackNumberText == obj.BackNumberText
						)
					) ||
					(
						Type == BackType.Other &&
						OtherText == obj.OtherText
					)
				)
			;
		}

		public bool EqualsWithoutWorkAndRework(FullBackDataModel obj, DocumentTemplateType documentTemplate)
		{
			return Type == obj.Type &&
				(
					(
						Type != BackType.Other &&
						EpisodeNumberText == obj.EpisodeNumberText &&
						(
							Type == BackType.Predmets
							|| Type == BackType.Morf
							|| Type == BackType.Collection
							|| BackName == obj.BackName
						)
						&& GameName == obj.GameName &&
						(
							documentTemplate != DocumentTemplateType.Painter ||
							IsSketch == obj.IsSketch
						) &&
						(
							(
								Type != BackType.Regions &&
								Type != BackType.HogRegions
							) ||
							BackCountRegionsText == obj.BackCountRegionsText
						) &&
						(
							Type == BackType.Craft 
							|| Type == BackType.Predmets
							|| Type == BackType.Predmet
							|| Type == BackType.Morf
							|| Type == BackType.Collection
							|| Type == BackType.Character
							|| Type == BackType.Interface
							|| Type == BackType.Marketing
							|| Type == BackType.VideoCadr
							|| Type == BackType.VideoObject
							|| Type == BackType.VideoPredmet
							|| BackNumberText == obj.BackNumberText
						)
					) ||
					(
						Type == BackType.Other &&
						OtherText == obj.OtherText
					)
				)
			;
		}

		public void EnableActionsStacking()
		{
			if (actionsStack != null)
				actionsStack.ActionsStackingEnabled = true;
		}

		public void DisableActionsStacking()
		{
			if (actionsStack != null)
				actionsStack.ActionsStackingEnabled = false;
		}

		public void EnableCollapsingActionByTargetEnabled()
		{
			if (actionsStack != null)
				actionsStack.CollapsingActionByTargetEnabled = true;
		}

		public void DisableCollapsingActionByTargetEnabled()
		{
			if (actionsStack != null)
				actionsStack.CollapsingActionByTargetEnabled = false;
		}

		public void SetSumTextChangesWithLink(string sumText, params IUndoRedoAction[] links)
		{
			IUndoRedoAction action = sumTextActionProvider.CreateAction(this, sumText);
			foreach (IUndoRedoAction link in links)
			{
				action.AddLink(link);
			}
			actionsStack.AddLinkToLast(action);
			this.sumText = sumText;
		}
	}
}
