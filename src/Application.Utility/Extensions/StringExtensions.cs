using System.Text.RegularExpressions;

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

        public static string[] SplitSuffixNumber(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return [input];

            var match = Regex.Match(input, @"^(.*?)(\d+)$");
            if (match.Success)
            {
                return new string[]
                {
                    match.Groups[1].Value,  // 前缀
                    match.Groups[2].Value   // 数字
                };
            }

            return [input];
        }
    }
}
