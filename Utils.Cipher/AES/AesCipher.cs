using System;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace Utils.Cipher.AES
{
    public class AesCipher : ICipherHelper
    {
        private static readonly SecureString _Pass = new char[] { 'S', '3', 'r', 'v', '1', 's', 'i', 'm' }.Aggregate(new SecureString(), (ss, c) => { ss.AppendChar(c); return ss; });
        private static readonly SecureString _Salt = new char[] { 'S', '1', 's', 't', '3', 'm', '4', '5' }.Aggregate(new SecureString(), (ss, c) => { ss.AppendChar(c); return ss; });
        private static readonly SecureString _Iv = new char[] { 'e', '6', '7', '5', 'f', '7', '2', '5' ,'e','6','7','5','f','7','2','5' }.Aggregate(new SecureString(), (ss, c) => { ss.AppendChar(c); return ss; });
        public string Encrypt(string value)
        {
            using AesCryptoServiceProvider csp = new();
            ICryptoTransform e = GetCryptoTransform(csp, true);
            byte[] inputBuffer = Encoding.UTF8.GetBytes(value);
            byte[] output = e.TransformFinalBlock(inputBuffer, 0, inputBuffer.Length);
            string encrypted = Convert.ToBase64String(output);
            return encrypted;
        }

        public string Decrypt(string value)
        {
            using AesCryptoServiceProvider csp = new();
            var d = GetCryptoTransform(csp, false);
            byte[] output = Convert.FromBase64String(value);
            byte[] decryptedOutput = d.TransformFinalBlock(output, 0, output.Length);
            string decypted = Encoding.UTF8.GetString(decryptedOutput);
            return decypted;
        }

        private static ICryptoTransform GetCryptoTransform(AesCryptoServiceProvider csp, bool encrypt)
        {
            csp.Mode = CipherMode.CBC;
            csp.Padding = PaddingMode.PKCS7;
            using (_Pass)
            {
                using (_Salt)
                {
                    Rfc2898DeriveBytes spec = new(Encoding.UTF8.GetBytes(_Pass.ToCharArray()), Encoding.UTF8.GetBytes(_Salt.ToCharArray()), 65536);

                    byte[] key = spec.GetBytes(16);
                    using (_Iv)
                    {
                        csp.IV = Encoding.UTF8.GetBytes(_Iv.ToCharArray());
                        csp.Key = key;
                        return encrypt ? csp.CreateEncryptor() : csp.CreateDecryptor();
                    }
                }
            }
        }
    }
}
