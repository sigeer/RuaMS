using Application.Templates.Reader;
using Application.Templates.String;
using System.Globalization;

namespace Application.Templates.Reader.Xml.Provider
{
    public sealed class StringProvider : GenericKeyedProvider<StringBaseProvider>, IStringProvider
    {
        CultureInfo _culture;
        public StringProvider(CultureInfo cultureInfo, IWzPathResolver resolver)
            : base(cultureInfo.Name)
        {
            _culture = cultureInfo;

            _categoryData[(int)StringCategory.Item] = new StringItemProvider(resolver, cultureInfo);
            _categoryData[(int)StringCategory.Map] = new StringMapProvider(resolver, cultureInfo);
            _categoryData[(int)StringCategory.Mob] = new StringMobProvider(resolver, cultureInfo);
            _categoryData[(int)StringCategory.Npc] = new StringNpcProvider(resolver, cultureInfo);
            _categoryData[(int)StringCategory.Skill] = new StringSkillProvider(resolver, cultureInfo);
            _categoryData[(int)StringCategory.Quest] = new StringQuestProvider(resolver, cultureInfo);
        }


        public IEnumerable<AbstractTemplate> Search(StringCategory category, string searchText, int maxCount = 50)
        {
            var categoryData = GetRequiredSubProvider(category);
            if (categoryData == null)
                return [];

            return categoryData.Search(searchText, maxCount);
        }
    }
}
