using Application.Templates.Providers;
using Application.Templates.String;
using System.Globalization;

namespace Application.Templates.XmlWzReader.Provider
{
    public sealed class StringProvider : GenericKeyedProvider<StringBaseProvider>
    {
        CultureInfo _culture;
        public override ProviderType ProviderName => ProviderType.String;
        public StringProvider(TemplateOptions options, CultureInfo cultureInfo)
            : base(options)
        {
            _culture = cultureInfo;

            _categoryData[(int)StringCategory.Item] = new StringItemProvider(options, cultureInfo);
            _categoryData[(int)StringCategory.Map] = new StringMapProvider(options, cultureInfo);
            _categoryData[(int)StringCategory.Mob] = new StringMobProvider(options, cultureInfo);
            _categoryData[(int)StringCategory.Npc] = new StringNpcProvider(options, cultureInfo);
            _categoryData[(int)StringCategory.Skill] = new StringSkillProvider(options, cultureInfo);
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
