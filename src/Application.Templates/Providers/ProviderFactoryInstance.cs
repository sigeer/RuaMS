using Application.Templates.Exceptions;
using Microsoft.Extensions.Logging;
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
        Dictionary<Type, IProvider> _providers = new();

        public void RegisterProvider(IProvider provider)
        {
            if (!_providers.TryAdd(provider.GetType(), provider))
                throw new ProviderDuplicateException(provider.ProviderName.ToString());
        }


        public void UseLogger(ILogger logger)
        {
            LibLog.Logger = logger;
        }

        internal TProvider GetProvider<TProvider>() where TProvider : IProvider
        {
            var type = typeof(TProvider);
            if (_providers.TryGetValue(type, out var data) && data is TProvider p)
                return p;

            throw new ProviderNotFoundException(type.Name.ToString());
        }
        Dictionary<string, IProvider> _kedProviders = new();
        public void RegisterKeydProvider(string key, IProvider provider)
        {
            if (!_kedProviders.TryAdd(key, provider))
                throw new ProviderDuplicateException(key);
        }
        internal IProvider GetProviderByKey(string key)
        {
            if (_kedProviders.TryGetValue(key, out var data) && data is not null)
                return data;

            throw new ProviderNotFoundException(key);
        }

        internal void Clear()
        {
            _kedProviders.Clear();
            _providers.Clear();
        }
    }
}
