using DocumentMaker.Security;

namespace ProjectEditorLib.ViewModel
{
	public interface ICryptedConnectionStringRequired
	{
		void SetCryptedConnectionString(CryptedConnectionString cryptedConnectionString);
	}
}
