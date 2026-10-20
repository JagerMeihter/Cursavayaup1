using System;
using System.Security.Cryptography;
using System.Text;

namespace Cursavayaup1.Helpers
{
    public static class PasswordHelper
    {
        // Хэширование пароля (SHA256 + соль)
        public static string HashPassword(string password)
        {
            // Соль (можно сделать более сложную)
            string salt = "DonatePlatform2026";
            string salted = password + salt;

            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(salted));
                StringBuilder sb = new StringBuilder();
                foreach (byte b in bytes)
                    sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        // Проверка пароля
        public static bool VerifyPassword(string password, string hash)
        {
            string hashOfInput = HashPassword(password);
            return hashOfInput.Equals(hash, StringComparison.OrdinalIgnoreCase);
        }
    }
}

// ганжубас