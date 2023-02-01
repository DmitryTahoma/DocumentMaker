using ProjectsDb.Context;

namespace ActGenerator.ViewModel.Dialogs.Controls
{
	class ProjectNameDialogItemViewModel
	{
		#region Properties

		public Project Project { get; set; } = null;

		public AlternativeProjectName AlternativeProjectName { get; set; } = null;

		#endregion
	}
}
