using Application.Templates.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Application.Templates.Providers
{
    public class ProviderSource
    {
        public ProviderSource(IConfiguration configuration)
        {
            BaseDir = configuration.GetValue("BaseDir", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wz"));
        }

        public static ProviderSource Instance { get; set; } = null!;

        public void UseLogger(ILogger logger)
        {
            LibLog.Logger = logger;
        }


        /// <summary>
        /// wz所在目录
        /// </summary>
        public string BaseDir { get; set; }
        Dictionary<Type, Lazy<IProvider>> _providers = new();

        public ProviderSource RegisterProvider<TProvider>(Func<ProviderOption, IProvider> func) where TProvider : IProvider
        {
            if (!_providers.TryAdd(typeof(TProvider), new Lazy<IProvider>(() =>
            {
                var mergedOption = new ProviderOption();
                mergedOption.DataDir ??= BaseDir;
                return func(mergedOption);
            })))
                throw new ProviderDuplicateException(typeof(TProvider).Name);

            return this;
        }

        public bool TryRegisterProvider<TProvider>(Func<ProviderOption, IProvider> func) where TProvider : IProvider
        {
            return _providers.TryAdd(typeof(TProvider), new Lazy<IProvider>(() =>
            {
                var o = new ProviderOption();
                o.DataDir ??= BaseDir;
                return func(o);
            }));
        }

        public TProvider GetProvider<TProvider>() where TProvider : IProvider
        {
            var type = typeof(TProvider);
            if (_providers.TryGetValue(type, out var data) && data.Value is TProvider p)
            {
                return p;
            }

            throw new ProviderNotFoundException(type.Name.ToString());
        }
        Dictionary<string, Lazy<IKeyedProvider>> _keyedProviders = new();
        public ProviderSource RegisterKeydProvider(string key, Func<ProviderOption, IKeyedProvider> func)
        {
            if (!_keyedProviders.TryAdd(key, new Lazy<IKeyedProvider>(() =>
            {
                var o = new ProviderOption();
                o.DataDir ??= BaseDir;
                return func(o);
            })))
                throw new ProviderDuplicateException(key);

            return this;
        }

        public bool TryRegisterKeydProvider(string key, Func<ProviderOption, IKeyedProvider> func)
        {
            return _keyedProviders.TryAdd(key, new Lazy<IKeyedProvider>(() =>
            {
                var o = new ProviderOption();
                o.DataDir ??= BaseDir;
                return func(o);
            }));
        }
        public TProvider GetProviderByKey<TProvider>(string key) where TProvider : IKeyedProvider
        {
            if (_keyedProviders.TryGetValue(key, out var data) && data.Value is TProvider p)
            {
                return p;
            }

            throw new ProviderNotFoundException(key);
        }

        public void Dispose()
        {
            foreach (var item in _keyedProviders)
            {
                if (item.Value.IsValueCreated)
                    item.Value.Value.Dispose();
            }

            foreach (var item in _providers)
            {
                if (item.Value.IsValueCreated)
                    item.Value.Value.Dispose();
            }
        }

        public void Debug()
        {
            LibLog.Logger.LogDebug("====> 已注册Provider：");
            foreach (var item in _providers)
            {
                LibLog.Logger.LogDebug("Type: {ProviderName} {ProviderType}, 读取目录: {BaseDir}",
                    item.Value.Value.ProviderName, item.Value.Value.GetType().Name, item.Value.Value.GetBaseDir());
            }

            LibLog.Logger.LogDebug("====> 已注册KeyedProvider：");
            foreach (var item in _keyedProviders.Values)
            {
                foreach (var keydItem in item.Value.GetSubProviders())
                {
                    LibLog.Logger.LogDebug("Key: {Key}, Type: {ProviderName} {ProviderType}, 读取目录: {BaseDir}",
                        item.Value.Key, keydItem.ProviderName, keydItem.GetType().Name, keydItem.GetBaseDir());
                }
            }
        }
    }
}
