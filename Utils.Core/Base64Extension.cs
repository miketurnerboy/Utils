using System;
using System.Text;

namespace Utils.Core
{
    public static class Base64Extension
    {
        public static string EncodeB64(this string plainText) {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        public static string DecodeB64(this string b64EncodedData)
        {
            var b64EncodedBytes = Convert.FromBase64String(b64EncodedData);
            return Encoding.UTF8.GetString(b64EncodedBytes);
        }
    }
}
