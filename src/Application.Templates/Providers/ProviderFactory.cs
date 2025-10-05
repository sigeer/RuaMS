using Application.Templates.Exceptions;

namespace Application.Templates.Providers
{
    public class ProviderFactory
    {
        private static string? _globalDataDir;
        public static string GetDataDir()
        {
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
                throw new ProviderNotFoundException("没有设置wz的目录");

            return _globalDataDir;
        }
        public static void Initilaize(Action<ProviderFactoryInstance> action)
        {
            action(Instance);

            if (string.IsNullOrEmpty(Instance.DataDir) || !Directory.Exists(Instance.DataDir))
            {
                if (string.IsNullOrEmpty(_globalDataDir) || !Directory.Exists(_globalDataDir))
                {
                    GetDataDir();
                }
            }
            else
            {
                _globalDataDir = Instance.DataDir;
            }
        }

        public static TProvider GetProvider<TProvider>() where TProvider : IProvider => Instance.GetProvider<TProvider>();
        public static IProvider GetProviderByKey(string key) => Instance.GetProviderByKey(key);

        static ProviderFactoryInstance Instance = new ProviderFactoryInstance();
    }
}
