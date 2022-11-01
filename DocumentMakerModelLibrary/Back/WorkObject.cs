namespace DocumentMakerModelLibrary.Back
{
	public class WorkObject
	{
		public WorkObject()
		{
			Id = 0;
			Name = "";
		}

		public uint Id { get; set; }
		public string Name { get; set; }

		public override string ToString()
		{
			return Name;
		}

		public override bool Equals(object obj)
		{
			return obj is WorkObject other
				&& Id == other.Id;
		}

		public override int GetHashCode()
		{
			return 2108858624 + Id.GetHashCode();
		}
	}
}
