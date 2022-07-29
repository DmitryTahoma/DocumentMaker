namespace DocumentMaker.Model.Back
{
	public class BackDataType
	{
		public BackDataType()
		{
			Name = "Бек";
			Type = BackType.Back;
		}

		public string Name { get; set; }
		public BackType Type { get; set; }

		public override string ToString()
		{
			return Name;
		}

		public override bool Equals(object obj)
		{
			return obj is BackDataType other 
				&& Type == other.Type;
		}
	}
}
