namespace Utils.Cipher.AES
{
    public interface ICipherHelper
    {
        public string Encrypt(string value);

        public string Decrypt(string value);
    }
}