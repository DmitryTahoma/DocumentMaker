using System.Collections.Generic;

namespace Dml.Model.Back
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

		public override int GetHashCode()
		{
			int hashCode = -243844509;
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
			hashCode = hashCode * -1521134295 + Type.GetHashCode();
			return hashCode;
		}
	}
}
