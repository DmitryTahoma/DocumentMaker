using Mvvm;
using Mvvm.Commands;
using ProjectEditorLib.Model;
using System;
using System.Windows.Controls;

namespace ProjectEditorLib.ViewModel
{
	public class ProjectEditViewModel
	{
		ProjectEditModel model = new ProjectEditModel();

		public ProjectEditViewModel()
		{
			ProjectNode proj = new ProjectNode(ProjectNodeType.Project, "Escape");
			InitProjectNodeCommands(proj);
			ProjectNodes.Add(proj);
		}

		#region Properties

		public ObservableRangeCollection<ProjectNode> ProjectNodes { get; private set; } = new ObservableRangeCollection<ProjectNode>();

		#endregion

		#region Commands

		#region ProjectNodeCommands

		private Command CreateCommand(Action<ProjectNode> action, ProjectNode param)
		{
			return new Command(() => action?.Invoke(param));
		}

		private void InitProjectNodeCommands(ProjectNode node)
		{
			node.AddEpisodeCommand = CreateCommand(OnAddEpisodeCommandExecute, node);
			node.AddBackCommand = CreateCommand(OnAddBackCommandExecute, node);
			node.AddCraftCommand = CreateCommand(OnAddCraftCommandExecute, node);
			node.AddMinigameCommand = CreateCommand(OnAddMinigameCommandExecute, node);
			node.AddDialogCommand = CreateCommand(OnAddDialogCommandExecute, node);
			node.AddHogCommand = CreateCommand(OnAddHogCommandExecute, node);
			node.AddRegionsCommand = CreateCommand(OnAddRegionsCommandExecute, node);
			node.RemoveCommand = CreateCommand(OnRemoveCommandExecute, node);
			node.InitContextMenu();
		}

		private void OnAddEpisodeCommandExecute(ProjectNode sender)
		{
			AddNodeToTree(sender, ProjectNodeType.Episode);
		}

		private void OnAddBackCommandExecute(ProjectNode sender)
		{
			AddNodeToTree(sender, ProjectNodeType.Back);
		}

		private void OnAddCraftCommandExecute(ProjectNode sender)
		{
			AddNodeToTree(sender, ProjectNodeType.Craft);
		}

		private void OnAddMinigameCommandExecute(ProjectNode sender)
		{
			AddNodeToTree(sender, ProjectNodeType.Minigame);
		}

		private void OnAddDialogCommandExecute(ProjectNode sender)
		{
			AddNodeToTree(sender, ProjectNodeType.Dialog);
		}

		private void OnAddHogCommandExecute(ProjectNode sender)
		{
			AddNodeToTree(sender, ProjectNodeType.Hog);
		}

		private void OnAddRegionsCommandExecute(ProjectNode sender)
		{
			AddNodeToTree(sender, ProjectNodeType.Regions);
		}

		private void OnRemoveCommandExecute(ProjectNode sender)
		{

		}

		#endregion

		#endregion

		#region Methods

		private void AddNodeToTree(ProjectNode parent, ProjectNodeType type)
		{
			ProjectNode node = new ProjectNode(type, type.ToString());
			InitProjectNodeCommands(node);
			if (parent.ProjectNodes == null)
			{
				parent.ProjectNodes = new ObservableRangeCollection<ProjectNode>();
			}
			parent.ProjectNodes.Add(node);

			ProjectNodes.UpdateCollection();
		}

		#endregion
	}
}
