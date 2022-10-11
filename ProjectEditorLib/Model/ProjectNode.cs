using Db.Context;
using Mvvm;
using Mvvm.Commands;
using System.Collections.Generic;
using System.Windows.Controls;

namespace ProjectEditorLib.Model
{
	public class ProjectNode
	{
		private ProjectNode()
		{
			AddEpisode = ConvertCommand(AddEpisodeCommand);
			AddBack = ConvertCommand(AddBackCommand);
			AddCraft = ConvertCommand(AddCraftCommand);
			AddMinigame = ConvertCommand(AddMinigameCommand);
			AddDialog = ConvertCommand(AddDialogCommand);
			AddHog = ConvertCommand(AddHogCommand);
			AddRegions = ConvertCommand(AddRegionsCommand);
			Remove = ConvertCommand(RemoveCommand);
		}

		public ProjectNode(ProjectNodeType type, string text) : this()
		{
			Type = type;
			Text = text;

			InitContextMenu();
		}

		public ProjectNodeType Type { get; set; }
		public string Text { get; set; }

		public ContextMenu ContextMenu { get; private set; } = null;
		public IDbObject DataContext { get; set; } = null;

		public ObservableRangeCollection<ProjectNode> ProjectNodes { get; set; } = null;

		#region Commands

		private Command ConvertCommand(Command<ProjectNode> command)
		{
			return new Command(() => command?.Execute(this));
		}

		public Command<ProjectNode> AddEpisodeCommand { get; set; }
		private Command AddEpisode { get; }

		public Command<ProjectNode> AddBackCommand { get; set; }
		private Command AddBack { get; }

		public Command<ProjectNode> AddCraftCommand { get; set; }
		private Command AddCraft { get; }

		public Command<ProjectNode> AddMinigameCommand { get; set; }
		private Command AddMinigame { get; }

		public Command<ProjectNode> AddDialogCommand { get; set; }
		private Command AddDialog { get; }

		public Command<ProjectNode> AddHogCommand { get; set; }
		private Command AddHog { get; }

		public Command<ProjectNode> AddRegionsCommand { get; set; }
		private Command AddRegions { get; }

		public Command<ProjectNode> RemoveCommand { get; set; }
		private Command Remove { get; }

		#endregion

		#region ContextMenu

		private void InitContextMenu()
		{
			ContextMenu = new ContextMenu();
			List<Control> itemsSource = new List<Control>();
			switch (Type)
			{
				case ProjectNodeType.Project:
					itemsSource.Add(new MenuItem { Header = "Додати епізод", Command = AddEpisode });
					break;
				case ProjectNodeType.ProjectWithoutEpisodes:
					itemsSource.Add(new MenuItem { Header = "Додати бек", Command = AddBack });
					itemsSource.Add(new MenuItem { Header = "Додати крафт", Command = AddCraft });
					break;
				case ProjectNodeType.Episode:
					itemsSource.Add(new MenuItem { Header = "Додати бек", Command = AddBack });
					itemsSource.Add(new MenuItem { Header = "Додати крафт", Command = AddCraft });
					itemsSource.Add(new Separator());
					itemsSource.Add(new MenuItem { Header = "Видалити епізод", Command = Remove });
					break;
				case ProjectNodeType.Back:
					itemsSource.Add(new MenuItem { Header = "Додати мініігру", Command = AddMinigame });
					itemsSource.Add(new MenuItem { Header = "Додати діалог", Command = AddDialog });
					itemsSource.Add(new MenuItem { Header = "Додати хог", Command = AddHog });
					itemsSource.Add(new Separator());
					itemsSource.Add(new MenuItem { Header = "Видалити бек", Command = Remove });
					break;
				case ProjectNodeType.Craft:
					itemsSource.Add(new MenuItem { Header = "Додати мініігру", Command = AddMinigame });
					itemsSource.Add(new MenuItem { Header = "Додати хог", Command = AddHog });
					itemsSource.Add(new Separator());
					itemsSource.Add(new MenuItem { Header = "Видалити крафт", Command = Remove });
					break;
				case ProjectNodeType.Dialog:
					itemsSource.Add(new MenuItem { Header = "Видалити діалог", Command = Remove });
					break;
				case ProjectNodeType.Hog:
					itemsSource.Add(new MenuItem { Header = "Додати мініігру", Command = AddMinigame });
					itemsSource.Add(new Separator());
					itemsSource.Add(new MenuItem { Header = "Видалити хог", Command = Remove });
					break;
				case ProjectNodeType.Minigame:
					itemsSource.Add(new MenuItem { Header = "Додати регіони", Command = AddRegions });
					itemsSource.Add(new Separator());
					itemsSource.Add(new MenuItem { Header = "Видалити мінігру", Command = Remove });
					break;
				case ProjectNodeType.Regions:
					itemsSource.Add(new MenuItem { Header = "Видалити регіони", Command = Remove });
					break;
			}
			ContextMenu.ItemsSource = itemsSource;
		}

		#endregion
	}
}
