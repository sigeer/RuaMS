using Application.Templates.Providers;

namespace Application.Templates.XmlWzReader.Provider
{
    public abstract class GenericKeyedProvider<TSubProvider> : IProvider 
        where TSubProvider : AbstractProvider<AbstractTemplate> 
    {
        public abstract ProviderType ProviderName { get; }
        protected GenericKeyedProvider(TemplateOptions options)
        {
            _categoryData = new();
        }
        protected Dictionary<int, TSubProvider> _categoryData;
        public TSubProvider? GetSubProvider(int key) => _categoryData.GetValueOrDefault(key);

        public void Dispose()
        {
            foreach (var item in _categoryData.Values)
            {
                item.Dispose();
            }
            _categoryData.Clear();
        }
    }
}
