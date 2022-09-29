namespace Db.Context.HumanPart
{
	public class Bank
	{
		public int Id { get; set; }
		public string Name { get; set; }
		/// <summary>
		/// Inter-Branch Turnover (МФО Межфилиальный оборот)
		/// </summary>
		public int IBT { get; set; }

		public void Set(Bank other)
		{
			Name = other.Name;
			IBT = other.IBT;
		}
	}
}
