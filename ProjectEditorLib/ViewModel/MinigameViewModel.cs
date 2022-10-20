using Db.Context;
using Db.Context.BackPart;
using System.Windows;

namespace ProjectEditorLib.ViewModel
{
	public class MinigameViewModel : DependencyObject, IDbObjectViewModel
	{
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

		public bool HaveUnsavedChanges
		{
			get { return (bool)GetValue(HaveUnsavedChangesProperty); }
			set { SetValue(HaveUnsavedChangesProperty, value); }
		}
		public static readonly DependencyProperty HaveUnsavedChangesProperty = DependencyProperty.Register(nameof(HaveUnsavedChanges), typeof(bool), typeof(MinigameViewModel));

		#endregion

		#region Methods

		public void SetFromContext(IDbObject dbObject)
		{
			if(dbObject is Back minigame)
			{
				MinigameName = minigame.Name;
				MinigameNumberText = minigame.Number.ToString();
			}
			else
			{
				MinigameName = string.Empty;
				MinigameNumberText = string.Empty;
			}
		}

		public IDbObject UpdateContext(IDbObject dbObject)
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
			}

			return dbObject;
		}

		#endregion
	}
}
