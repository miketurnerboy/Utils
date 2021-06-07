using System;
using System.Security;
using System.Runtime.InteropServices;
using Utils.Core;

namespace Utils.Cipher
{
    public static class SecureStringExtension
    {
        private static void CheckNullRef(this object value)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }
        }

        public static SecureString ConvertToSecure(this String value, bool leaveOriginal = false, bool makeReadOnly = true)
        {
            value.CheckNullRef();
            SecureString secureString;

            unsafe
            {
                fixed (char* chars = value)
                {
                    //create encrypted secure string object
                    secureString = new SecureString(chars, value.Length);
                    if (makeReadOnly)
                        secureString.MakeReadOnly(); //.AddChar and InsertAt methods won't work if true

                    if (!leaveOriginal)
                        value.SecureClear();
                }
            }

            return secureString;
        }

        public static string ConvertToUnsecure(this SecureString value)
        {
            IntPtr valuePtr = IntPtr.Zero;
            try
            {
                valuePtr = Marshal.SecureStringToGlobalAllocUnicode(value);
                return Marshal.PtrToStringUni(valuePtr);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
            }
        }



        public static char[] ToCharArray(this SecureString value)
        {
            IntPtr ptr = Marshal.SecureStringToBSTR(value);
            try
            {
                byte b = 1;
                int i = 0, j = 0;

                // Loop through until we hit the string terminator '\0'
                while (((char)b) != '\0')
                {
                    b = Marshal.ReadByte(ptr, i);
                    j++;
                    i += 2;  // BSTR is unicode and occupies 2 bytes
                }


                char[] val = new char[j];
            
                i = 0;

                for (j=0;j<val.Length;j++)
                {
                    b = Marshal.ReadByte(ptr, i);
                    val[j] = (char)b;
                    i += 2;  // BSTR is unicode and occupies 2 bytes
                }
                return val;
            }
            finally
            {
                // Free AND ZERO OUT our marshalled BSTR pointer to our securetext
                Marshal.ZeroFreeBSTR(ptr);
            }
        }

        public static void SecureClear(this String value)
        {
            object checkInterned = String.IsInterned(value);
            if (checkInterned == null)
            {
                unsafe
                {
                    fixed (char* chars = value)
                    {
                        //zero out original
                        for (int i = 0; i < value.Length; i++)
                            chars[i] = '\0';
                    }
                }
            }
            else
            {
                throw new CipherUtilException ("10","Improper use of SecureClear on a literal or interned object");
            }
        }
    }
}
