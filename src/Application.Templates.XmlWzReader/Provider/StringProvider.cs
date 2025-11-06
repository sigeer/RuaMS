using Application.Templates.Providers;
using Application.Templates.String;
using System.Globalization;

namespace Application.Templates.XmlWzReader.Provider
{
    public sealed class StringProvider : GenericKeyedProvider<StringBaseProvider>
    {
        CultureInfo _culture;
        public override string ProviderName => ProviderNames.String;
        public StringProvider(ProviderOption options, CultureInfo cultureInfo)
            : base(cultureInfo.Name, options)
        {
            _culture = cultureInfo;

            _categoryData[(int)StringCategory.Item] = new StringItemProvider(options, cultureInfo);
            _categoryData[(int)StringCategory.Map] = new StringMapProvider(options, cultureInfo);
            _categoryData[(int)StringCategory.Mob] = new StringMobProvider(options, cultureInfo);
            _categoryData[(int)StringCategory.Npc] = new StringNpcProvider(options, cultureInfo);
            _categoryData[(int)StringCategory.Skill] = new StringSkillProvider(options, cultureInfo);
            _categoryData[(int)StringCategory.Quest] = new StringQuestProvider(options, cultureInfo);
        }


        public IEnumerable<AbstractTemplate> Search(StringCategory category, string searchText, int maxCount = 50)
        {
            var categoryData = GetRequiredSubProvider((int)category);
            if (categoryData == null)
                return [];

            return categoryData.Search(searchText, maxCount);
        }

        public StringBaseProvider GetSubProvider(StringCategory key)
        {
            return GetRequiredSubProvider((int)key) ?? throw new ArgumentException($"不支持的{nameof(StringCategory)}, value={key}。");
        }
    }
}
