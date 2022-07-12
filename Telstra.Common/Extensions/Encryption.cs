using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Telstra.Common
{
    public static class Encryption
    {
        private const string initVector = "13A79C64AA314318";

        // This constant is used to determine the keysize of the encryption algorithm
        private const int keysize = 256;

        private static readonly byte[] salt = new byte[]
            {0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76};

        private static string defaultPhrase = "34061948-18F3-42E5-A6AE-3176E08AF07A";

        public static string EncryptString(string plainText)
        {
            return EncryptString(plainText, defaultPhrase);
        }

        public static string EncryptString(string plainText, string passPhrase)
        {
            var initVectorBytes = Encoding.UTF8.GetBytes(initVector);
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            var password = new Rfc2898DeriveBytes(passPhrase, salt);

            var keyBytes = password.GetBytes(keysize / 8);

            var symmetricKey = new RijndaelManaged { Mode = CipherMode.CBC };

            var encryptor = symmetricKey.CreateEncryptor(keyBytes, initVectorBytes);

            var memoryStream = new MemoryStream();
            var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);

            cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
            cryptoStream.FlushFinalBlock();

            var cipherTextBytes = memoryStream.ToArray();
            memoryStream.Close();
            cryptoStream.Close();

            return Convert.ToBase64String(cipherTextBytes);
        }

        public static string DecryptString(string cipherText)
        {
            return DecryptString(cipherText, defaultPhrase);
        }

        public static string DecryptString(string cipherText, string passPhrase)
        {
            var initVectorBytes = Encoding.UTF8.GetBytes(initVector);
            var cipherTextBytes = Convert.FromBase64String(cipherText);
            var password = new Rfc2898DeriveBytes(passPhrase, salt);
            var keyBytes = password.GetBytes(keysize / 8);

            var symmetricKey = new RijndaelManaged { Mode = CipherMode.CBC };

            var decryptor = symmetricKey.CreateDecryptor(keyBytes, initVectorBytes);

            var memoryStream = new MemoryStream(cipherTextBytes);
            var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);

            var plainTextBytes = new byte[cipherTextBytes.Length];
            var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);

            memoryStream.Close();
            cryptoStream.Close();

            return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
        }
    }


}
