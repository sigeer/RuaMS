using Application.Templates.Exceptions;
using Microsoft.Extensions.Logging;

namespace Application.Templates.Reader
{
    public class ProviderSource
    {
        public ProviderSource(IWzPathResolver resolver)
        {
            Resolver = resolver;
        }

        public static ProviderSource Instance { get; set; } = null!;

        public void UseLogger(ILogger logger)
        {
            LibLog.Logger = logger;
        }

        public IWzPathResolver Resolver { get; }

        Dictionary<ProviderType, Lazy<IProvider>> _providersByName = new();

        //public ProviderSource RegisterProvider<TProvider>(Func<ProviderOption, IProvider> func) where TProvider : IProvider
        //{
        //    var type = typeof(TProvider);
        //    if (!_providersByType.TryAdd(type, new Lazy<IProvider>(() => CreateProvider(func))))
        //        throw new ProviderDuplicateException(type.Name);
        //    return this;
        //}

        //public bool TryRegisterProvider<TProvider>(Func<ProviderOption, IProvider> func) where TProvider : IProvider
        //{
        //    return _providersByType.TryAdd(typeof(TProvider), new Lazy<IProvider>(() => CreateProvider(func)));
        //}

        //public TProvider GetProvider<TProvider>() where TProvider : IProvider
        //{
        //    var type = typeof(TProvider);
        //    if (_providersByType.TryGetValue(type, out var data) && data.Value is TProvider p)
        //        return p;
        //    throw new ProviderNotFoundException(type.Name);
        //}

        public ProviderSource RegisterProvider(Func<IProvider> factory)
        {
            var p = factory();
            if (!_providersByName.TryAdd(p.Type, new Lazy<IProvider>(factory)))
                throw new ProviderDuplicateException(p.Type.ToString());
            return this;
        }

        public bool TryRegisterProvider(Func<IProvider> factory)
        {
            var p = factory();
            return _providersByName.TryAdd(p.Type, new Lazy<IProvider>(factory));
        }

        public IProvider GetProvider(ProviderType providerName)
        {
            return _providersByName.GetValueOrDefault(providerName)?.Value ?? throw new ProviderNotFoundException(providerName.ToString());

        }
        public TProvider GetProvider<TProvider>(ProviderType providerName) where TProvider : IProvider
        {
            if (_providersByName.TryGetValue(providerName, out var data))
                return (TProvider)data.Value;
            throw new ProviderNotFoundException(providerName.ToString());
        }

        Dictionary<string, Lazy<IKeyedProvider>> _keyedProviders = new();
        public ProviderSource RegisterKeydProvider(string key, Func<IKeyedProvider> func)
        {
            if (!_keyedProviders.TryAdd(key, new Lazy<IKeyedProvider>(func)))
                throw new ProviderDuplicateException(key);
            return this;
        }

        public bool TryRegisterKeydProvider(string key, Func<IKeyedProvider> func)
        {
            return _keyedProviders.TryAdd(key, new Lazy<IKeyedProvider>(func));
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

            foreach (var item in _providersByName.Values.Concat(_providersByName.Values))
            {
                if (item.IsValueCreated)
                    item.Value.Dispose();
            }
        }

        public void Debug()
        {
            LibLog.Logger.LogDebug("使用 {ResolverType}", Resolver.GetType().Name);
            LibLog.Logger.LogDebug("====> 已注册Provider（按名称）：");
            foreach (var item in _providersByName)
            {
                LibLog.Logger.LogDebug("  Name: {Name}, Type: {ProviderType}",
                    item.Key, item.Value.Value.GetType().Name);
            }

            LibLog.Logger.LogDebug("====> 已注册KeyedProvider：");
            foreach (var item in _keyedProviders.Values)
            {
                foreach (var keydItem in item.Value.GetSubProviders())
                {
                    LibLog.Logger.LogDebug("Key: {Key}, Type: {ProviderType}",
                        item.Value.Key, keydItem.GetType().Name);
                }
            }
        }
    }
}
