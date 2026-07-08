using Application.Templates.Exceptions;
using System.Globalization;

namespace Application.Templates.Reader.Resolvers
{
    public static class ResolverExtensions
    {
        /// <summary>
        /// 获取完整路径
        /// </summary>
        /// <param name="resolver"></param>
        /// <param name="relativePath"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        /// <exception cref="ImgNotFound"></exception>
        public static string ResolveFullPath(this IWzPathResolver resolver, string? relativePath, CultureInfo? culture = null)
        {
            if (string.IsNullOrEmpty(relativePath))
                throw new ImgNotFound($"WzFileProvider没有找到文件", relativePath);

            string filePath = Path.Combine(resolver.BaseDir, relativePath);
            if (culture != null && culture.Name != "en-US")
            {
                var cultureDir = $"{resolver.BaseDir}-{culture.Name}";
                var cultureFile = Path.Combine(cultureDir, relativePath);

                if (File.Exists(cultureFile))
                    filePath = cultureFile;
            }
            return filePath;
        }
    }
}
