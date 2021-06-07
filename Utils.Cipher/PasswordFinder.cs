using Org.BouncyCastle.OpenSsl;
using System.Security;

namespace Utils.Cipher
{
    internal class PasswordFinder : IPasswordFinder
    {
        private readonly SecureString _password;

        public PasswordFinder(SecureString password)
        {
            _password = password;
        }

        public char[] GetPassword()
        {
            return _password.ConvertToUnsecure().ToCharArray();
        }
    }
}