using System.Globalization;

namespace Application.Shared.Languages
{
    public static class SupportedCultureManager
    {
        private static readonly Dictionary<LanguageEnum, CultureInfo> _cultureMap = new()
        {
            {LanguageEnum.zhCN, CultureInfo.GetCultureInfo("zh-CN") },
            {LanguageEnum.enUS, CultureInfo.GetCultureInfo("en-US") }
        };


        public static CultureInfo GetCulture(int language)
        {
            return _cultureMap.GetValueOrDefault((LanguageEnum)language) ?? CultureInfo.GetCultureInfo("zh-CN");
        }
    }
}
