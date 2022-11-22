using System.Reflection;
using System.Security.Cryptography;

namespace DocumentMaker.Security
{
	public class CryptedConnectionString
	{
		readonly Crypter crypter = new Crypter();
		readonly string cryptKey = Assembly.GetExecutingAssembly().Location;

		public string ConnectionString { get; set; }
		public bool IsCrypted { get; set; }

		public void Crypt()
		{
			if (!IsCrypted)
			{
				ConnectionString = crypter.Protect(ConnectionString, cryptKey, DataProtectionScope.CurrentUser);
				IsCrypted = true;
			}
		}

		public string GetDecryptedConnectionString()
		{
			return IsCrypted ? crypter.Unprotect(ConnectionString, cryptKey, DataProtectionScope.CurrentUser) : ConnectionString;
		}

		public bool TryGetDecryptedConnectionString(out string decryptedConnectionString)
		{
			try
			{
				decryptedConnectionString = GetDecryptedConnectionString();
				return true;
			}
			catch
			{
				decryptedConnectionString = null;
				return false;
			}
		}
	}
}
