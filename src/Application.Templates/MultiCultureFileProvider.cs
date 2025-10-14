using System.Globalization;

namespace Application.Templates
{
    public class MultiCultureFileProvider
    {
        string _defaultDir;
        public MultiCultureFileProvider(string baseDir)
        {
            _defaultDir = baseDir;
        }

        public string? FindFile(string relativePath, CultureInfo? culture = null)
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
            var defaultFile = Path.Combine(_defaultDir, relativePath);
            return File.Exists(defaultFile) ? defaultFile : null;
        }

        public Stream? ReadFile(string relativePath, CultureInfo? culture = null)
        {
            var filePath = FindFile(relativePath, culture);
            return filePath != null ? File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read) : null;
        }

        /// <summary>
        /// 从默认目录列举所有文件（以默认目录为准）
        /// </summary>
        /// <param name="fileFunc"></param>
        /// <returns>文件名，不含路径、文件后缀</returns>
        public string[] ListAllFiles(Func<string, bool> fileFunc)
        {
            return Directory.GetFiles(_defaultDir)
                    .Where(x => fileFunc(x)).Select(x => Path.GetFileNameWithoutExtension(x)).ToArray();
        }
    }
}
