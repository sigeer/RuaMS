namespace Application.Utility.Extensions
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

            return string.Concat(
                str.AsSpan(0, start),
                newStr,
                str.AsSpan(start + oldStr.Length)
            );
        }
    }
}
