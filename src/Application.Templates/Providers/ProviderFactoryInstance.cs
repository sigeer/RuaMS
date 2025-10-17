using Application.Templates.Exceptions;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Globalization;

namespace Application.Templates.Providers
{
    public class ProviderFactoryInstance
    {
        internal ProviderFactoryInstance()
        {

        }
        /// <summary>
        /// wz所在目录
        /// </summary>
        public string? DataDir { get; set; }
        Dictionary<Type, Lazy<IProvider>> _providers = new();

        public void RegisterProvider<TProvider>(Func<IProvider> func) where TProvider : IProvider
        {
            if (!_providers.TryAdd(typeof(TProvider), new Lazy<IProvider>(func)))
                throw new ProviderDuplicateException(typeof(TProvider).Name);
        }


        public void UseLogger(ILogger logger)
        {
            LibLog.Logger = logger;
        }

        internal TProvider GetProvider<TProvider>() where TProvider : IProvider
        {
            var type = typeof(TProvider);
            if (_providers.TryGetValue(type, out var data) && data.Value is TProvider p)
                return p;

            throw new ProviderNotFoundException(type.Name.ToString());
        }
        Dictionary<string, Lazy<IKeyedProvider>> _keyedProviders = new();
        public void RegisterKeydProvider(string key, Func<IKeyedProvider> func)
        {
            if (!_keyedProviders.TryAdd(key, new Lazy<IKeyedProvider>(func)))
                throw new ProviderDuplicateException(key);
        }
        internal TProvider GetProviderByKey<TProvider>(string key) where TProvider : IKeyedProvider
        {
            if (_keyedProviders.TryGetValue(key, out var data) && data.Value is TProvider p)
                return p;

            throw new ProviderNotFoundException(key);
        }

        internal void Dispose()
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

        internal void Debug()
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
