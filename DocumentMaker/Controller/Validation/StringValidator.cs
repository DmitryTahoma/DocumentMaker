using System;
using System.Text.RegularExpressions;

namespace DocumentMaker.Controller.Validation
{
    internal class StringValidator
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
                        if (string.IsNullOrWhiteSpace(part) || part.Length < 3 || !nameRegex.IsMatch(part))
                        {
                            return false;
                        }
                    }

                    return true;
                }
            }
            return false;
        }
    }
}
