namespace Dml
{
	public class StructContainer<T> where T : struct 
	{
		public StructContainer()
		{
			Obj = default;
		}

		public StructContainer(T obj)
		{
			Obj = obj;
		}

		public T Obj { get; set; }

		public static implicit operator StructContainer<T>(T obj)
		{
			return new StructContainer<T>(obj);
		}

		public static explicit operator T(StructContainer<T> container)
		{
			return container.Obj;
		}
	};
}
