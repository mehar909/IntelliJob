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
    }
}