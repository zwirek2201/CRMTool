using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Licencjat_new.CustomClasses
{
    class CryptographyHelper
    {
        #region Methods
        public static string EncodeString(string data)
        {
            byte[] salt = GenerateSalt(100);
            byte[] hashedData = ProtectedData.Protect(Encoding.Unicode.GetBytes(data), salt, DataProtectionScope.CurrentUser);

            string saltString = Convert.ToBase64String(salt);
            string hashedDataString = Convert.ToBase64String(hashedData);

            return saltString + "|" + hashedDataString;
        }

        public static string DecodeString(string data)
        {
            string stringSalt = data.Substring(0, data.IndexOf("|"));
            string stringData = data.Substring(data.IndexOf("|") + 1);

            byte[] saltBytes = Convert.FromBase64String(stringSalt);
            byte[] dataBytes = Convert.FromBase64String(stringData);

            byte[] encodedData = ProtectedData.Unprotect(dataBytes, saltBytes, DataProtectionScope.CurrentUser);

            return Encoding.Unicode.GetString(encodedData);
        }

        public static string HashString(string data, int saltSize = 100)
        {
            byte[] salt;
            salt = saltSize > 0 ? GenerateSalt(saltSize) : new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            Rfc2898DeriveBytes crypto = new Rfc2898DeriveBytes(data, salt);
            crypto.IterationCount = 10000;
            byte[] hashedData = crypto.GetBytes(64);

            return Convert.ToBase64String(salt) + "|" + Convert.ToBase64String(hashedData);
        }

        private static byte[] GenerateSalt(int length)
        {
            byte[] bytes = new byte[length];
            RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider();

            try
            {
                crypto.GetBytes(bytes);
                return bytes;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        #endregion
    }
}
