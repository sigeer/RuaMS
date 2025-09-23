using Application.Templates.Providers;
using Application.Templates.String;

namespace Application.Templates.XmlWzReader.Provider
{
    public sealed class StringProvider : GenericKeyedProvider<StringBaseProvider>
    {
        public override ProviderType ProviderName => ProviderType.String;
        public StringProvider(TemplateOptions options)
            : base(options)
        {
            _categoryData[(int)StringCategory.Item] = new StringItemProvider(options);
            _categoryData[(int)StringCategory.Map] = new StringMapProvider(options);
            _categoryData[(int)StringCategory.Mob] = new StringMobProvider(options);
            _categoryData[(int)StringCategory.Npc] = new StringNpcProvider(options);
            _categoryData[(int)StringCategory.Skill] = new StringSkillProvider(options);
        }


        public IEnumerable<AbstractTemplate> Search(StringCategory category, string searchText, int maxCount = 50)
        {
            var categoryData = GetSubProvider((int)category);
            if (categoryData == null)
                return [];

            return categoryData.Search(searchText, maxCount);
        }

        public StringBaseProvider GetSubProvider(StringCategory key)
        {
            return GetSubProvider((int)key) ?? throw new ArgumentException($"不支持的{nameof(StringCategory)}, value={key}。");
        }
    }
}
