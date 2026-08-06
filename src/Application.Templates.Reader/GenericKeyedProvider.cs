using Application.Templates.String;

namespace Application.Templates.Reader
{
    public abstract class GenericKeyedProvider<TSubProvider, TSubTemplate> : IKeyedProvider<TSubTemplate>
        where TSubProvider : IProvider<TSubTemplate>
        where TSubTemplate: AbstractTemplate
    {
        public string Key { get; }

        protected GenericKeyedProvider(string key)
        {
            Key = key;
            _categoryData = new();
        }
        protected Dictionary<int, TSubProvider> _categoryData;

        public void Dispose()
        {
            foreach (var item in _categoryData.Values)
            {
                item.Dispose();
            }
            _categoryData.Clear();
        }

        protected TSubProvider? GetSubProvider(StringCategory key)
        {
            return _categoryData.GetValueOrDefault((int)key);
        }

        protected IEnumerable<TSubProvider> GetSubProviders()
        {
            return _categoryData.Values;
        }

        public string GetBaseDir() => string.Empty;

        public TSubTemplate? GetItem(int templateId)
        {
            foreach (var sub in _categoryData.Values)
            {
                var item = sub.GetItem(templateId);
                if (item != null) return item;
            }
            return null;
        }

        IProvider<TSubTemplate>? IKeyedProvider<TSubTemplate>.GetSubProvider(StringCategory key)
        {
            return GetSubProvider(key);
        }

        IEnumerable<IProvider<TSubTemplate>> IKeyedProvider<TSubTemplate>.GetSubProviders()
        {
            return GetSubProviders().OfType<IProvider<TSubTemplate>>();
        }

        IProvider? IKeyedProvider.GetSubProvider(StringCategory key)
        {
            return GetSubProvider(key);
        }

        IEnumerable<IProvider> IKeyedProvider.GetSubProviders()
        {
            return GetSubProviders().OfType<IProvider>();
        }
    }
}
