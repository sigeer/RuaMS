using Application.Templates.Exceptions;

namespace Application.Templates.Providers
{
    public class ProviderFactory
    {
        private static string? _globalDataDir;
        public static string GetEffectDir(string? inputDir)
        {
            if (!string.IsNullOrEmpty(inputDir) && Directory.Exists(inputDir))
                return inputDir;

            if (string.IsNullOrEmpty(_globalDataDir) || !Directory.Exists(_globalDataDir))
            {
                var pathFromEnv = Environment.GetEnvironmentVariable("ms-wz") ?? Environment.GetEnvironmentVariable("RUA_MS_ms-wz");
                if (Directory.Exists(pathFromEnv))
                {
                    _globalDataDir = pathFromEnv;
                }
                else
                {
                    var pathFromDefault = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wz");
                    if (Directory.Exists(_globalDataDir))
                        _globalDataDir = pathFromDefault;
                }
            }

            if (string.IsNullOrEmpty(_globalDataDir) || !Directory.Exists(_globalDataDir))
                throw new DataDirNotFoundException("没有设置wz的目录，默认路径也没有找到wz。");

            return _globalDataDir;
        }
        public static void Initilaize(Action<ProviderFactoryInstance> action)
        {
            action(Instance);

            _globalDataDir = GetEffectDir(_globalDataDir);
        }

        public static TProvider GetProvider<TProvider>() where TProvider : IProvider => Instance.GetProvider<TProvider>();
        public static IProvider GetProviderByKey(string key) => Instance.GetProviderByKey(key);
        public static void Clear() => Instance.Clear();

        static ProviderFactoryInstance Instance = new ProviderFactoryInstance();
    }
}
