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
                    return pathFromEnv;

                var pathFromDefault = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wz");
                if (Directory.Exists(pathFromDefault))
                    return pathFromDefault;

                throw new DataDirNotFoundException("没有设置wz的目录，默认路径也没有找到wz。");
            }

            return _globalDataDir;
        }

        /// <summary>
        /// 在 Server.Start 之前调用都有效。
        /// </summary>
        /// <param name="action"></param>
        public static void Configure(Action<ProviderFactoryInstance> action)
        {
            action(Instance);
            _globalDataDir = GetEffectDir(Instance.DataDir);
        }

        public static TProvider GetProvider<TProvider>() where TProvider : IProvider => Instance.GetProvider<TProvider>();
        public static TProvider GetProviderByKey<TProvider>(string key) where TProvider : IProvider => Instance.GetProviderByKey<TProvider>(key);
        public static void Clear() => Instance.Clear();

        static ProviderFactoryInstance Instance = new ProviderFactoryInstance();
    }
}
