using System;
using System.Security.Cryptography;
using System.Text;

namespace DocumentMaker.Security
{
	public class Crypter
	{
		public string Protect(string stringToEncrypt, string optionalEntropy, DataProtectionScope scope)
		{
			return Convert.ToBase64String(
				ProtectedData.Protect(
					Encoding.UTF8.GetBytes(stringToEncrypt),
					optionalEntropy != null ? Encoding.UTF8.GetBytes(optionalEntropy) : null,
					scope));
		}

		public string Unprotect(string encryptedString, string optionalEntropy, DataProtectionScope scope)
		{
			return Encoding.UTF8.GetString(
				ProtectedData.Unprotect(
					Convert.FromBase64String(encryptedString),
					optionalEntropy != null ? Encoding.UTF8.GetBytes(optionalEntropy) : null,
					scope));
		}
	}
}
