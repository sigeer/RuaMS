using Application.Templates.Exceptions;
using Microsoft.Extensions.Logging;

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
        public string? BaseDir { get; set; }
        Dictionary<Type, IProvider> _cached = new();
        public void RegisterProvider(IProvider provider)
        {
            if (!_cached.TryAdd(provider.GetType(), provider))
                throw new ProviderDuplicateException(provider.ProviderName.ToString());
        }

        public void UseLogger(ILogger logger)
        {
            LibLog.Logger = logger;
        }

        internal TProvider GetProvider<TProvider>() where TProvider : IProvider
        {
            var type = typeof(TProvider);
            if (_cached.TryGetValue(type, out var data) && data is TProvider p)
                return p;

            throw new ProviderNotFoundException(type.Name.ToString());
        }
    }
}
