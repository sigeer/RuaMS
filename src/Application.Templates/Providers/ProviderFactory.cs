using Application.Templates.Exceptions;

namespace Application.Templates.Providers
{
    public class ProviderFactory
    {
        private static string? _baseDir;
        public static void Initilaize(Action<ProviderFactoryInstance> action)
        {
            action(Instance);
            if (string.IsNullOrEmpty(Instance.BaseDir) || !Directory.Exists(Instance.BaseDir))
            {
                GetBaseDir();
            }
            else
            {
                _baseDir = Instance.BaseDir;
            }
        }

        public static string GetBaseDir()
        {
            if (string.IsNullOrEmpty(_baseDir) || !Directory.Exists(_baseDir))
            {
                var pathFromEnv = Environment.GetEnvironmentVariable("ms-wz") ?? Environment.GetEnvironmentVariable("RUA_MS_ms-wz");
                if (Directory.Exists(pathFromEnv))
                {
                    return _baseDir = pathFromEnv;
                }
                else
                {
                    _baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wz");

                    if (Directory.Exists(_baseDir))
                        return _baseDir;
                }
                throw new DataDirNotFoundException();
            }

            return _baseDir;
        }

        public static TProvider GetProvider<TProvider>() where TProvider : IProvider => Instance.GetProvider<TProvider>();

        static ProviderFactoryInstance Instance = new ProviderFactoryInstance();
    }
}
