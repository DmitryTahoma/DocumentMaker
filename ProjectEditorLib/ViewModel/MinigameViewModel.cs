using Db.Context;
using Db.Context.BackPart;
using System.Windows;

namespace ProjectEditorLib.ViewModel
{
	public class MinigameViewModel : BaseDbObjectViewModel, IDbObjectViewModel
	{
		public MinigameViewModel() : base() { }

		#region Properties

		public string MinigameNumberText
		{
			get { return (string)GetValue(MinigameNumberTextProperty); }
			set { SetValue(MinigameNumberTextProperty, value); }
		}
		public static readonly DependencyProperty MinigameNumberTextProperty = DependencyProperty.Register(nameof(MinigameNumberText), typeof(string), typeof(MinigameViewModel));

		public string MinigameName
		{
			get { return (string)GetValue(MinigameNameProperty); }
			set { SetValue(MinigameNameProperty, value); }
		}
		public static readonly DependencyProperty MinigameNameProperty = DependencyProperty.Register(nameof(MinigameName), typeof(string), typeof(MinigameViewModel));

		#endregion

		#region Methods

		public override void SetFromContext(IDbObject dbObject)
		{
			if(dbObject is Back minigame)
			{
				MinigameName = minigame.Name;
				MinigameNumberText = minigame.Number.ToString();
				context = minigame;
			}
			else
			{
				MinigameName = string.Empty;
				MinigameNumberText = string.Empty;
			}
		}

		public override IDbObject UpdateContext(IDbObject dbObject)
		{
			Back minigame;

			if(dbObject == null)
			{
				minigame = new Back();
			}
			else
			{
				minigame = dbObject as Back;
			}

			if(minigame != null)
			{
				minigame.Name = MinigameName;
				minigame.Number = float.Parse(MinigameNumberText);
				dbObject = minigame;
				context = minigame;
			}

			return dbObject;
		}

		#endregion
	}
}
