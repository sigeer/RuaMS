using System.Globalization;

namespace Application.Core.Game.Players
{
    public static class ClientCultureManager
    {
        private static readonly Dictionary<LanguageEnmu, CultureInfo> _cultureMap = new()
        {
            {LanguageEnmu.zhCN, new CultureInfo("zh-CN") },
            {LanguageEnmu.enUS, new CultureInfo("en-US") }
        };
        public static CultureInfo GetCulture(this IChannelClient client)
        {
            return _cultureMap.GetValueOrDefault((LanguageEnmu)client.Language) ?? _cultureMap[LanguageEnmu.zhCN];
        }
    }
}
