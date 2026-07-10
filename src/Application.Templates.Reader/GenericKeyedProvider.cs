using Application.Templates.String;

namespace Application.Templates.Reader
{
    public abstract class GenericKeyedProvider<TSubProvider> : IKeyedProvider
        where TSubProvider : AbstractProvider<AbstractTemplate>
    {
        public string Key { get; }

        protected GenericKeyedProvider(string key)
        {
            Key = key;
            _categoryData = new();
        }
        protected Dictionary<int, AbstractProvider<AbstractTemplate>> _categoryData;
        public TSubProvider? GetRequiredSubProvider(StringCategory key)
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

        public IProvider? GetSubProvider(StringCategory key)
        {
            return _categoryData.GetValueOrDefault((int)key);
        }

        public IEnumerable<IProvider> GetSubProviders()
        {
            return _categoryData.Values;
        }

        public string GetBaseDir() => string.Empty;

        public AbstractTemplate? GetItem(int templateId)
        {
            foreach (var sub in _categoryData.Values)
            {
                var item = sub.GetItem(templateId);
                if (item != null) return item;
            }
            return null;
        }
    }
}
