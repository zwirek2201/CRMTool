using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Licencjat_new_server
{
    public static class Cryptography
    {
        public static string HashString(string data, int saltSize = 100)
        {
            byte[] salt;
            salt = saltSize > 0 ? GenerateSalt(saltSize) : new byte[] {1, 2, 3, 4, 5, 6, 7, 8};
            Rfc2898DeriveBytes crypto = new Rfc2898DeriveBytes(data,salt);
            crypto.IterationCount = 10000;
            byte[] hashedData = crypto.GetBytes(64);

            return Convert.ToBase64String(salt) + "|" + Convert.ToBase64String(hashedData);
        }

        public static string HashString(string data, string salt)
        {
            Rfc2898DeriveBytes crypto = new Rfc2898DeriveBytes(data, Convert.FromBase64String(salt));
            crypto.IterationCount = 10000;
            byte[] hashedData = crypto.GetBytes(64);

            return salt + "|" + Convert.ToBase64String(hashedData);
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
    }
}
