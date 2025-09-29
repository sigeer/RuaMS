using System.Globalization;

namespace Application.Core.Game.Players
{
    public static class ClientCultureManager
    {
        private static readonly Dictionary<int, CultureInfo> _cultureMap = new()
        {
            {1, new CultureInfo("zh-CN") },
            {2, new CultureInfo("en-US") }
        };
        public static CultureInfo GetCulture(this IChannelClient client)
        {
            return _cultureMap.GetValueOrDefault(client.Language) ?? _cultureMap[1];
        }
    }
}
