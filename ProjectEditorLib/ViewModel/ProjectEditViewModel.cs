using Mvvm;
using ProjectEditorLib.Model;

namespace ProjectEditorLib.ViewModel
{
	public class ProjectEditViewModel
	{
		ProjectEditModel model = new ProjectEditModel();

		public ProjectEditViewModel()
		{
			ProjectNodes = new ObservableRangeCollection<ProjectNode>
			{
				new ProjectNode(ProjectNodeType.Project, "Escape")
				{
					ProjectNodes = new ObservableRangeCollection<ProjectNode>
					{
						new ProjectNode(ProjectNodeType.Episode, "1. Котедж")
						{
							ProjectNodes = new ObservableRangeCollection<ProjectNode>()
							{
								new ProjectNode(ProjectNodeType.Back, "1. Вітальня")
								{
									ProjectNodes = new ObservableRangeCollection<ProjectNode>()
									{
										new ProjectNode(ProjectNodeType.Minigame, "1. Скриня"),
										new ProjectNode(ProjectNodeType.Minigame, "2. Шахмати"),
										new ProjectNode(ProjectNodeType.Minigame, "3. Замок до підвалу"),
									},
								},
								new ProjectNode(ProjectNodeType.Back, "2. Кухня")
								{
									ProjectNodes = new ObservableRangeCollection<ProjectNode>
									{
										new ProjectNode(ProjectNodeType.Minigame, "1. Мікрохвильова піч")
										{
											ProjectNodes = new ObservableRangeCollection<ProjectNode>
											{
												new ProjectNode(ProjectNodeType.Regions, "Регіони"),
											},
										},
										new ProjectNode(ProjectNodeType.Hog, "1. Хог"),
										new ProjectNode(ProjectNodeType.Dialog, "1. Ділог"),
									},
								},
								new ProjectNode(ProjectNodeType.Craft, "Магніт на мотузці"),
								new ProjectNode(ProjectNodeType.Craft, "Шуруповерт"),
								new ProjectNode(ProjectNodeType.Craft, "Test"),
							},
						},
					},
				}
			};
		}

		#region Properties

		public ObservableRangeCollection<ProjectNode> ProjectNodes { get; private set; } = new ObservableRangeCollection<ProjectNode>();

		#endregion

		#region Commands

		#endregion

		#region Methods

		#endregion
	}
}
