using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IntelliJob
{
    public class Utils
    {
        public static bool IsValidExtension(string fileName)
        {
            bool isValid = false;
            string[] fileExtension = { ".jpg", ".png", ".jpeg" };
            for (int i=0; i<fileExtension.Length; i++)
            {
                if (fileName.Contains(fileExtension[i])) {
                isValid = true;
                break;
                }
            }
            return isValid;
        }

        public static bool IsValidExtension4Resume(string fileName)
        {
            bool isValid = false;
            string[] fileExtension = { ".doc", ".docx", ".pdf" };
            for (int i = 0; i < fileExtension.Length; i++)
            {
                if (fileName.Contains(fileExtension[i]))
                {
                    isValid = true;
                    break;
                }
            }
            return isValid;
        }
        public static string ExtractDuplicateValue(string message)
        {
            // Extract the value within the parentheses from the message
            int startIndex = message.IndexOf('(');
            int endIndex = message.IndexOf(')', startIndex);
            if (startIndex != -1 && endIndex != -1)
            {
                return message.Substring(startIndex + 1, endIndex - startIndex - 1).Trim();
            }
            return string.Empty;
        }

        public static string GenerateNumericCode(int digits)
        {
            var rng = new System.Security.Cryptography.RNGCryptoServiceProvider();
            byte[] data = new byte[4];
            rng.GetBytes(data);
            int value = Math.Abs(BitConverter.ToInt32(data, 0));
            int mod = (int)Math.Pow(10, digits);
            return (value % mod).ToString().PadLeft(digits, '0');
        }

        public static string GenerateSalt()
        {
            var rng = new System.Security.Cryptography.RNGCryptoServiceProvider();
            byte[] saltBytes = new byte[16];
            rng.GetBytes(saltBytes);
            return Convert.ToBase64String(saltBytes);
        }

        public static string ComputeSha256Hash(string input)
        {
            using (var sha = System.Security.Cryptography.SHA256.Create())
            {
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(input);
                byte[] hash = sha.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }
    }
}