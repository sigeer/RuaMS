using Application.Templates.Reader;
using Application.Templates.String;
using System.Globalization;

namespace Application.Templates.Reader.Img.Provider
{
    public sealed class StringProvider : GenericKeyedProvider<StringBaseProvider>, IStringProvider
    {
        CultureInfo _culture;
        public StringProvider(CultureInfo cultureInfo, IWzPathResolver resolver)
            : base(cultureInfo.Name)
        {
            _culture = cultureInfo;

            _categoryData[(int)StringCategory.Item] = new StringItemProvider(cultureInfo, resolver);
            _categoryData[(int)StringCategory.Map] = new StringMapProvider(cultureInfo, resolver);
            _categoryData[(int)StringCategory.Mob] = new StringMobProvider(cultureInfo, resolver);
            _categoryData[(int)StringCategory.Npc] = new StringNpcProvider(cultureInfo, resolver);
            _categoryData[(int)StringCategory.Skill] = new StringSkillProvider(cultureInfo, resolver);
            _categoryData[(int)StringCategory.Quest] = new StringQuestProvider(cultureInfo, resolver);
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
