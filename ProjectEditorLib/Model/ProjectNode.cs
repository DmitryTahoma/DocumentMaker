using Db.Context;
using Mvvm;
using Mvvm.Commands;
using System.Collections.Generic;
using System.Windows.Controls;

namespace ProjectEditorLib.Model
{
	public class ProjectNode
	{
		public ProjectNode(ProjectNodeType type, string text)
		{
			Type = type;
			Text = text;
		}

		public ProjectNodeType Type { get; set; }
		public string Text { get; set; }

		public ContextMenu ContextMenu { get; private set; } = null;
		public IDbObject DataContext { get; set; } = null;

		public ObservableRangeCollection<ProjectNode> ProjectNodes { get; set; } = null;

		#region Commands

		public Command AddEpisodeCommand { get; set; }
		public Command AddBackCommand { get; set; }
		public Command AddCraftCommand { get; set; }
		public Command AddMinigameCommand { get; set; }
		public Command AddDialogCommand { get; set; }
		public Command AddHogCommand { get; set; }
		public Command AddRegionsCommand { get; set; }
		public Command RemoveCommand { get; set; }

		#endregion

		#region ContextMenu

		public void InitContextMenu()
		{
			ContextMenu = new ContextMenu();
			List<Control> itemsSource = new List<Control>();
			switch (Type)
			{
				case ProjectNodeType.Project:
					itemsSource.Add(new MenuItem { Header = "Додати епізод", Command = AddEpisodeCommand });
					break;
				case ProjectNodeType.ProjectWithoutEpisodes:
					itemsSource.Add(new MenuItem { Header = "Додати бек", Command = AddBackCommand });
					itemsSource.Add(new MenuItem { Header = "Додати крафт", Command = AddCraftCommand });
					break;
				case ProjectNodeType.Episode:
					itemsSource.Add(new MenuItem { Header = "Додати бек", Command = AddBackCommand });
					itemsSource.Add(new MenuItem { Header = "Додати крафт", Command = AddCraftCommand });
					itemsSource.Add(new Separator());
					itemsSource.Add(new MenuItem { Header = "Видалити епізод", Command = RemoveCommand });
					break;
				case ProjectNodeType.Back:
					itemsSource.Add(new MenuItem { Header = "Додати мініігру", Command = AddMinigameCommand });
					itemsSource.Add(new MenuItem { Header = "Додати діалог", Command = AddDialogCommand });
					itemsSource.Add(new MenuItem { Header = "Додати хог", Command = AddHogCommand });
					itemsSource.Add(new Separator());
					itemsSource.Add(new MenuItem { Header = "Видалити бек", Command = RemoveCommand });
					break;
				case ProjectNodeType.Craft:
					itemsSource.Add(new MenuItem { Header = "Додати мініігру", Command = AddMinigameCommand });
					itemsSource.Add(new MenuItem { Header = "Додати хог", Command = AddHogCommand });
					itemsSource.Add(new Separator());
					itemsSource.Add(new MenuItem { Header = "Видалити крафт", Command = RemoveCommand });
					break;
				case ProjectNodeType.Dialog:
					itemsSource.Add(new MenuItem { Header = "Видалити діалог", Command = RemoveCommand });
					break;
				case ProjectNodeType.Hog:
					itemsSource.Add(new MenuItem { Header = "Додати мініігру", Command = AddMinigameCommand });
					itemsSource.Add(new Separator());
					itemsSource.Add(new MenuItem { Header = "Видалити хог", Command = RemoveCommand });
					break;
				case ProjectNodeType.Minigame:
					itemsSource.Add(new MenuItem { Header = "Додати регіони", Command = AddRegionsCommand });
					itemsSource.Add(new Separator());
					itemsSource.Add(new MenuItem { Header = "Видалити мінігру", Command = RemoveCommand });
					break;
				case ProjectNodeType.Regions:
					itemsSource.Add(new MenuItem { Header = "Видалити регіони", Command = RemoveCommand });
					break;
			}
			ContextMenu.ItemsSource = itemsSource;
		}

		#endregion
	}
}
