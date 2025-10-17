using Application.Templates.Exceptions;
using Microsoft.Extensions.Logging;

namespace Application.Templates.Providers
{
    public class ProviderFactory
    {
        private static string? _globalDataDir;
        public static string GetEffectDir(string? inputDir)
        {
            if (Directory.Exists(inputDir))
                return inputDir;

            if (!Directory.Exists(_globalDataDir))
            {
                if (Directory.Exists(Instance.DataDir))
                    return Instance.DataDir;

                var pathFromEnv = Environment.GetEnvironmentVariable("ms-wz") ?? Environment.GetEnvironmentVariable("RUA_MS_ms-wz");
                if (Directory.Exists(pathFromEnv))
                    return _globalDataDir = pathFromEnv;

                var pathFromDefault = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wz");
                if (Directory.Exists(pathFromDefault))
                    return pathFromDefault;

                throw new DataDirNotFoundException("没有设置wz的目录，默认路径也没有找到wz。");
            }

            return _globalDataDir;
        }

        /// <summary>
        /// 替换设置
        /// <para><see cref="Apply"/> 后生效。</para>
        /// </summary>
        /// <param name="action"></param>
        public static void Configure(Action<ProviderFactoryInstance> action)
        {
            if (_instance != null)
            {
                _instance.Dispose();
                _instance = null;
            }

            ConfigureWith(action);
        }

        public static void ConfigureWith(Action<ProviderFactoryInstance> action)
        {
            _instance ??= new ProviderFactoryInstance();
            action?.Invoke(_instance);
            _globalDataDir = GetEffectDir(_instance.DataDir);

            LibLog.Logger.LogDebug("WZ - 默认目录：{DataDir}", _globalDataDir);
            Instance.Debug();
        }

        public static TProvider GetProvider<TProvider>() where TProvider : IProvider => Instance.GetProvider<TProvider>();
        public static TProvider GetProviderByKey<TProvider>(string key) where TProvider : IKeyedProvider => Instance.GetProviderByKey<TProvider>(key);
        public static void Clear()
        {
            if (_instance != null)
            {
                _instance.Dispose();
                _instance = null;
            }
        }

        static ProviderFactoryInstance? _instance;
        static ProviderFactoryInstance Instance => _instance ?? throw new Exception("ProviderFactory 尚未初始化");
    }
}
