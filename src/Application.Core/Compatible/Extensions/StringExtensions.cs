namespace Application.Core.Compatible.Extensions
{
    public static class StringExtensions
    {
        public static string replaceFirst(this string str, string oldStr, string newStr)
        {
            if (string.IsNullOrEmpty(oldStr))
                return str;

            var start = str.IndexOf(oldStr);
            if (start == -1)
                return str;

            return str.Substring(0, start) + newStr + str.Substring(start + oldStr.Length, str.Length - start - oldStr.Length);
        }
    }
}
