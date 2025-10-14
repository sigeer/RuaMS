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

        public string? FindFile(string? relativePath, CultureInfo? culture = null)
        {
            if (string.IsNullOrEmpty(relativePath))
                return null;

            if (culture != null && culture.Name != "en-US")
            {
                var cultureDir = $"{_defaultDir}-{culture.Name}";
                var cultureFile = Path.Combine(cultureDir, relativePath);

                if (File.Exists(cultureFile))
                    return cultureFile;
            }

            // 回退到默认目录
            return Path.Combine(_defaultDir, relativePath);
        }

        public Stream ReadFile(string? relativePath, CultureInfo? culture = null)
        {
            var filePath = FindFile(relativePath, culture) ?? throw new ImgNotFound($"WzFileProvider没有找到文件", relativePath);
            return File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }
    }
}
