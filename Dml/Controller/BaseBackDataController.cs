using Dml.Controller.Validation;
using Dml.Model;
using Dml.Model.Back;
using Dml.UndoRedo;
using System.Linq;

namespace Dml.Controller
{
	public abstract class BaseBackDataController
	{
		protected readonly StringValidator validator;
		private readonly BaseBackDataModel model;

		public BaseBackDataController(BaseBackDataModel _model)
		{
			validator = new StringValidator();
			model = _model;
		}

		public uint Id { get => model.Id; set => model.Id = value; }
		public BackType Type { get => model.Type; set => model.Type = value; }
		public string EpisodeNumberText { get => model.EpisodeNumberText; set => model.EpisodeNumberText = value; }
		public string BackNumberText { get => model.BackNumberText; set => model.BackNumberText = value; }
		public string BackName { get => model.BackName; set => model.BackName = value; }
		public string BackCountRegionsText { get => model.BackCountRegionsText; set => model.BackCountRegionsText = value; }
		public ObservableRangeCollection<GameObject> GameNameList => model.GameNameList;
		public string GameName { get => model.GameName; set => model.GameName = value; }
		public bool IsRework { get => model.IsRework; set => model.IsRework = value; }
		public bool IsSketch { get => model.IsSketch; set => model.IsSketch = value; }
		public string SpentTimeText { get => model.SpentTimeText; set => model.SpentTimeText = value; }
		public string OtherText { get => model.OtherText; set => model.OtherText = value; }
		public ObservableRangeCollection<BackDataType> BackDataTypesList => model.BackDataTypesList;
		public bool IsActionsStackingEnabled => model.IsActionsStackingEnabled;
		public bool CollapsingActionByTargetEnabled => model.CollapsingActionByTargetEnabled;
		public bool HaveUnsavedChanges { get => model.HaveUnsavedChanges; set => model.HaveUnsavedChanges = value; }

		public virtual BaseBackDataModel GetModel()
		{
			return model;
		}

		public virtual bool Validate(ref string errorText)
		{
			errorText += "Строка таблиці №" + Id.ToString() + ": ";

			if (BackNumberText != null)
				BackNumberText = BackNumberText.Replace(',', '.');

			if (Type == BackType.Other && !validator.IsFree(OtherText))
				errorText += "Строка з текстом не може бути пустою.";
			else if (Type != BackType.Other)
			{
				GameObject selectedGame = GameNameList.FirstOrDefault(x => x.Name == GameName);

				if (Type != BackType.Craft
					&& Type != BackType.Predmets
					&& Type != BackType.Morf
					&& Type != BackType.Collection
					&& Type != BackType.Character
					&& Type != BackType.Interface
					&& !validator.IsFree(BackNumberText))
					errorText += "Строка \"Номер беку\" не може бути пустою.";
				else if (Type != BackType.Craft
					&& Type != BackType.Predmets
					&& Type != BackType.Morf
					&& Type != BackType.Collection
					&& Type != BackType.Character
					&& Type != BackType.Interface
					&& !validator.IsUFloat(BackNumberText))
					errorText += "Невірно заповнений номер беку.\nПриклад: 15, 9.1, 15.6";
				else if (Type != BackType.Predmets
					&& Type != BackType.Morf
					&& Type != BackType.Collection
					&& !validator.IsFree(BackName))
					errorText += "Строка \"Ім’я беку\" не може бути пустою.";
				else if ((Type == BackType.Regions || Type == BackType.HogRegions) && !validator.IsFree(BackCountRegionsText))
					errorText += "Кількість регіонів не може бути пустою.\nПриклад: 11";
				else if ((Type == BackType.Regions || Type == BackType.HogRegions) && validator.IsEndMinus(BackCountRegionsText))
					errorText += "Регіони записані некорректно. Незавершена послідовність.\nПриклад: 1-2, 3-4, 8-15";
				else if ((Type == BackType.Regions || Type == BackType.HogRegions) && !validator.IsOrderRegions(BackCountRegionsText))
					errorText += "Регіони записані некорректно. Кінець послідовності менший за початок.\nПриклад: 1-2, 3-4, 8-15";
				else if (!validator.IsFree(GameName))
					errorText += "Строка \"Назва гри\" не може бути пустою.";
				else if (selectedGame != null && selectedGame.HaveEpisodes && !selectedGame.HaveEpisode(EpisodeNumberText))
					errorText += "Оберіть коректний епізод.";
				else if (selectedGame != null && !selectedGame.HaveEpisodes && !string.IsNullOrEmpty(EpisodeNumberText))
					errorText += "У вибраній грі \"" + selectedGame.Name + "\" немає епізодів.";
				else if (selectedGame != null && selectedGame.HaveEpisodes && selectedGame.HaveEpisode(EpisodeNumberText) && EpisodeNumberText == "Всі")
				{
					if(Type != BackType.Predmets 
						&& Type != BackType.Morf 
						&& Type != BackType.Collection 
						&& Type != BackType.Character
						&& Type != BackType.Interface)
						errorText += "Для обраного типу роботи не можна вказувати епізоди \"Всі\" ";
					else
						return true;
				}	
					
				else
					return true;
			}
			else
				return true;

			return false;
		}

		public virtual void TrimAllStrings()
		{
			EpisodeNumberText = EpisodeNumberText?.Trim();
			BackNumberText = BackNumberText?.Trim();
			BackName = BackName?.Trim();
			BackCountRegionsText = BackCountRegionsText?.Trim();
			GameName = GameName?.Trim();
			SpentTimeText = SpentTimeText?.Trim();
			OtherText = OtherText?.Trim();
		}

		public virtual void SetActionsStack(IUndoRedoActionsStack actionsStack)
		{
			model.SetActionsStack(actionsStack);
		}

		public virtual void AddUndoRedoLink(IUndoRedoAction action)
		{
			model.AddUndoRedoLink(action);
		}
	}
}
