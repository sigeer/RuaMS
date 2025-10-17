using Application.Templates.Providers;

namespace Application.Templates.XmlWzReader.Provider
{
    public abstract class GenericKeyedProvider<TSubProvider> : IKeyedProvider
        where TSubProvider : AbstractProvider<AbstractTemplate> 
    {
        public string Key { get; }
        public abstract string ProviderName { get; }
        protected GenericKeyedProvider(string key, TemplateOptions options)
        {
            Key = key;
            _categoryData = new();
        }
        protected Dictionary<int, AbstractProvider<AbstractTemplate>> _categoryData;
        public TSubProvider? GetRequiredSubProvider(int key)
        {
            return GetSubProvider(key) as TSubProvider;
        }

        public void Dispose()
        {
            foreach (var item in _categoryData.Values)
            {
                item.Dispose();
            }
            _categoryData.Clear();
        }

        public IProvider? GetSubProvider(int key)
        {
            return _categoryData.GetValueOrDefault(key);
        }

        public IEnumerable<IProvider> GetSubProviders()
        {
            return _categoryData.Values;
        }

        public string GetBaseDir()
        {
            return string.Empty;
        }
    }
}
