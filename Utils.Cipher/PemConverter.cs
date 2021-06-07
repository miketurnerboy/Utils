using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;
using System.IO;
using System.Security;

namespace Utils.Cipher
{
    public static class PemConverter
    {
        public static AsymmetricCipherKeyPair DecodePemPrivateKey(string encryptedPrivateKey, SecureString password) 
        {
            object privateKeyObj;
            using (TextReader tReader = new StringReader(encryptedPrivateKey)) {
                PemReader pReader = new(tReader, new PasswordFinder(password));
                privateKeyObj = pReader.ReadObject();
            }

            return (AsymmetricCipherKeyPair)privateKeyObj;
        }

        public static AsymmetricKeyParameter DecodePemPublicKey(string publicKey) {
            object publicKeyObj;
            using (TextReader tReader = new StringReader(publicKey)) {
                PemReader pReader = new(tReader);
                publicKeyObj = pReader.ReadObject();
            }

            return (AsymmetricKeyParameter)publicKeyObj;
        }

        public static string EncodePemPrivateKeyWithPass(AsymmetricCipherKeyPair kp, SecureString password)
        {
            string pemEncoded;
            using (TextWriter textWriter = new StringWriter())
            {
                PemWriter pWriter = new(textWriter);
                pWriter.WriteObject(kp.Public);
                pWriter.Writer.Flush();
                pemEncoded = textWriter.ToString();
            }
            return pemEncoded;
        }

        public static string EncodePemPrivateKey(AsymmetricCipherKeyPair kp) {
            string pemEncoded;
            using (TextWriter textWriter = new StringWriter())
            {
                PemWriter pemWriter = new(textWriter);
                pemWriter.WriteObject(kp.Private);
                pemWriter.Writer.Flush();
                pemEncoded = textWriter.ToString();
            }
            return pemEncoded;
        }
    }
}
