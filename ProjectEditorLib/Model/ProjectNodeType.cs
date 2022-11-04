namespace ProjectEditorLib.Model
{
	/// <summary>
	/// Тип узла проекта.
	/// 
	/// Используется, как индекс выбранной IDbObjectView в ProjectEdit.
	/// </summary>
	public enum ProjectNodeType
	{
		Project = 0,
		Episode = 1,
		Back = 2,
		Craft = 3,
		Dialog = 4,
		Hog = 5,
		Minigame = 6,
		Regions = 7,
	}
}
