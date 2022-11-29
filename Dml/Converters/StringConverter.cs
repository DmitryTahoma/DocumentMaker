using System.Text.RegularExpressions;

namespace Dml.Converters
{
	public class StringConverter
	{
		public float ConvertToFloat(string str)
		{
			return float.Parse(Regex.Replace(str.Replace(',', '.'), @"\s+", ""));
		}

		public bool TryConvertToFloat(string str, out float res)
		{
			try
			{
				res = ConvertToFloat(str);
				return true;
			}
			catch
			{
				res = new float();
				return false;
			}
		}
	}
}
