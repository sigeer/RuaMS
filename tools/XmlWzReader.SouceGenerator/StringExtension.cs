using System;
using System.Text;

namespace XmlWzReader.SouceGenerator
{
    public static class StringUtils
    {
        public static string FirstCharToLower(this string str)
        {
            if (string.IsNullOrEmpty(str) || char.IsLower(str[0]))
                return str;

            return char.ToLower(str[0]) + str.Substring(1);
        }

        public static string RemovePrefix(string str, string prefix)
        {
            if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(prefix))
                return str;

            return str.StartsWith(prefix) ? str.Substring(prefix.Length) : str;
        }

        public static StringBuilder AppendLineWithPreSpace(this StringBuilder sb, string str, int count)
        {
            for (int i = 0; i < count * 2; i++)
            {
                sb.Append("     ");
            }
            sb.Append(str);
            sb.Append("\r\n");
            return sb;
        }
    }
}
