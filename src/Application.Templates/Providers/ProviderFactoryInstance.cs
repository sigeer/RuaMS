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
        Dictionary<string, Lazy<IProvider>> _kedProviders = new();
        public void RegisterKeydProvider(string key, Func<IProvider> func)
        {
            if (!_kedProviders.TryAdd(key, new Lazy<IProvider>(func)))
                throw new ProviderDuplicateException(key);
        }
        internal TProvider GetProviderByKey<TProvider>(string key) where TProvider : IProvider
        {
            if (_kedProviders.TryGetValue(key, out var data) && data.Value is TProvider p)
                return p;

            throw new ProviderNotFoundException(key);
        }

        internal void Clear()
        {
            _kedProviders.Clear();
            _providers.Clear();
        }
    }
}
