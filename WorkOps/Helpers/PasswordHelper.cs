using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace WorkOps.Helpers
{
    public static class PasswordHelper
    {
        // Hash password using SHA256 with salt
        public static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException("password");

            // Generate salt
            byte[] salt = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }

            // Hash password with salt using PBKDF2
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000);
            byte[] hash = pbkdf2.GetBytes(20);

            // Combine salt and hash
            byte[] hashBytes = new byte[36];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);

            // Convert to Base64
            return Convert.ToBase64String(hashBytes);
        }

        // Verify password against stored hash (PBKDF2). Falls back to plain-text compare for legacy seed rows.
        public static bool VerifyPassword(string enteredPassword, string storedHash)
        {
            if (string.IsNullOrEmpty(enteredPassword) || string.IsNullOrEmpty(storedHash))
                return false;

            try
            {
                byte[] hashBytes = Convert.FromBase64String(storedHash);
                if (hashBytes.Length != 36)
                    return enteredPassword == storedHash;

                byte[] salt = new byte[16];
                Array.Copy(hashBytes, 0, salt, 0, 16);

                var pbkdf2 = new Rfc2898DeriveBytes(enteredPassword, salt, 10000);
                byte[] hash = pbkdf2.GetBytes(20);

                for (int i = 0; i < 20; i++)
                {
                    if (hashBytes[i + 16] != hash[i])
                        return false;
                }

                return true;
            }
            catch (FormatException)
            {
                return enteredPassword == storedHash;
            }
        }

        // Validate password complexity
        public static bool IsPasswordValid(string password, out string errorMessage)
        {
            errorMessage = null;

            if (string.IsNullOrEmpty(password))
            {
                errorMessage = "Password is required";
                return false;
            }

            if (password.Length < 8)
            {
                errorMessage = "Password must be at least 8 characters long";
                return false;
            }

            if (!ContainsUppercase(password))
            {
                errorMessage = "Password must contain at least one uppercase letter";
                return false;
            }

            if (!ContainsLowercase(password))
            {
                errorMessage = "Password must contain at least one lowercase letter";
                return false;
            }

            if (!ContainsNumber(password))
            {
                errorMessage = "Password must contain at least one number";
                return false;
            }

            if (!ContainsSpecialCharacter(password))
            {
                errorMessage = "Password must contain at least one special character (!@#$%^&*)";
                return false;
            }

            return true;
        }

        private static bool ContainsUppercase(string password)
        {
            foreach (char c in password)
                if (char.IsUpper(c)) return true;
            return false;
        }

        private static bool ContainsLowercase(string password)
        {
            foreach (char c in password)
                if (char.IsLower(c)) return true;
            return false;
        }

        private static bool ContainsNumber(string password)
        {
            foreach (char c in password)
                if (char.IsDigit(c)) return true;
            return false;
        }

        private static bool ContainsSpecialCharacter(string password)
        {
            string specialChars = "!@#$%^&*()_+-=[]{}|;:',.<>?/~`";
            foreach (char c in password)
                if (specialChars.Contains(c)) return true;
            return false;
        }

        // Generate random password
        public static string GenerateRandomPassword(int length = 10)
        {
            // Exclude & < > so passwords pass ASP.NET request validation when typed on login forms
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^*";
            const string specialChars = "!@#$%^*";
            var random = new Random();
            var result = new StringBuilder();

            // Ensure at least one of each type
            result.Append("ABCDEFGHIJKLMNOPQRSTUVWXYZ"[random.Next(26)]); // Uppercase
            result.Append("abcdefghijklmnopqrstuvwxyz"[random.Next(26)]); // Lowercase
            result.Append("0123456789"[random.Next(10)]); // Number
            result.Append(specialChars[random.Next(specialChars.Length)]); // Special

            // Fill remaining
            for (int i = 4; i < length; i++)
            {
                result.Append(chars[random.Next(chars.Length)]);
            }

            // Shuffle
            return new string(result.ToString().ToCharArray().OrderBy(x => random.Next()).ToArray());
        }
    }
}