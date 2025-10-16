using Application.Templates.Exceptions;
using System.Globalization;

namespace Application.Templates
{
    public class WzFileProvider
    {
        string _defaultDir;
        public WzFileProvider(string baseDir)
        {
            _defaultDir = baseDir;
        }

        /// <summary>
        /// 相对路径读取文件
        /// </summary>
        /// <param name="relativePath"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        /// <exception cref="ImgNotFound"></exception>
        public Stream ReadFile(string? relativePath, CultureInfo? culture = null)
        {
            if (string.IsNullOrEmpty(relativePath))
                throw new ImgNotFound($"WzFileProvider没有找到文件", relativePath);

            string filePath = Path.Combine(_defaultDir, relativePath);
            if (culture != null && culture.Name != "en-US")
            {
                var cultureDir = $"{_defaultDir}-{culture.Name}";
                var cultureFile = Path.Combine(cultureDir, relativePath);

                if (!File.Exists(cultureFile))
                    filePath = cultureFile;
            }
            return File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }
    }
}
