using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Dml.Controller.Validation
{
	public class StringValidator
	{
		private readonly Regex dateRegex;
		private readonly Regex digitRegex;
		private readonly Regex nameRegex;

		public StringValidator()
		{
			dateRegex = new Regex(@"^[0-3]\d\.[0-1]\d\.\d{4}$", RegexOptions.Compiled);
			digitRegex = new Regex(@"^\d+$", RegexOptions.Compiled);
			nameRegex = new Regex(@"^[\D\S]+$", RegexOptions.Compiled);
		}

		public bool IsFree(string str)
		{
			return !string.IsNullOrWhiteSpace(str);
		}

		public bool IsDate(string str)
		{
			return IsFree(str) && dateRegex.IsMatch(str) && DateTime.TryParse(str, out _);
		}

		public bool IsDigit(string str)
		{
			return IsFree(str) && digitRegex.IsMatch(str);
		}

		public bool IsFullName(string str)
		{
			if (IsFree(str))
			{
				string[] parts = str.Split(' ');
				if (parts.Length == 3)
				{
					foreach (string part in parts)
					{
						if (string.IsNullOrWhiteSpace(part) || part.Length < 2 || !nameRegex.IsMatch(part))
						{
							return false;
						}
					}

					return true;
				}
			}
			return false;
		}

		public bool IsEndMinus(string str)
		{
			return str.Replace(" ", "").Last() == '-';
		}

		public bool IsOrderRegions(string str)
		{
			string[] parts = str.Split(',');
			foreach(string part in parts)
			{
				if(part.Contains('-'))
				{
					string[] subparts = part.Split('-');
					if(subparts.Length > 1
						&& int.TryParse(subparts[0], out int st)
						&& int.TryParse(subparts[1], out int end)
						&& st > end)
					{
						return false;
					}
				}
			}
			return true;
		}

		public static string Trim(string str)
		{
			return Regex.Replace(str.Trim(), @"\s+", " ");
		}
	}
}
