using Application.Resources.Messages;
using Application.Shared.Languages;
using Application.Templates.Exceptions;
using Application.Templates.Providers;
using Application.Templates.String;
using Application.Templates.XmlWzReader.Provider;
using System.Globalization;
using System.Resources;

namespace Application.Core.Channel
{
    public class ClientCulture
    {
        public CultureInfo CultureInfo { get; }
        public ClientCulture(int language) : this(SupportedCultureManager.GetCulture(language))
        {
        }

        public StringProvider StringProvider { get; }

        public ClientCulture(CultureInfo cultureInfo)
        {
            CultureInfo = cultureInfo;
            StringProvider = (ProviderFactory.GetProviderByKey(CultureInfo.Name) as StringProvider)
                ?? throw new ProviderNotFoundException(nameof(StringProvider), $"没有找到{CultureInfo.Name}相应的wz资源");
        }

        public ClientCulture() : this(Thread.CurrentThread.CurrentCulture)
        {
        }

        public string GetMessageByKey(string key, params string[] paramsValue)
        {
            var message = ClientMessage.ResourceManager.GetString(key, CultureInfo);
            if (string.IsNullOrEmpty(message))
            {
                Log.Logger.Warning("i18n未找到{Key}", key);
                return key;
            }
            return string.Format(message, paramsValue);
        }

        public string? GetNullableMessageByKey(string key, params string[] paramsValue)
        {
            var message = ClientMessage.ResourceManager.GetString(key, CultureInfo);
            if (string.IsNullOrEmpty(message))
            {
                return null;
            }
            return string.Format(message, paramsValue);
        }

        public string GetScriptTalkByKey(string key, params string[] paramsValue)
        {
            var message = ScriptTalk.ResourceManager.GetString(key, CultureInfo);
            if (string.IsNullOrEmpty(message))
            {
                return key;
            }
            return string.Format(message, paramsValue);
        }

        public string GetMobName(int mobId)
        {
            return StringProvider.GetSubProvider(StringCategory.Mob)?.GetRequiredItem<StringTemplate>(mobId)?.Name ?? StringConstants.WZ_MissingNo;
        }

        public string GetNpcName(int npcId)
        {
            return StringProvider.GetSubProvider(StringCategory.Npc)?.GetRequiredItem<StringNpcTemplate>(npcId)?.Name ?? StringConstants.WZ_MissingNo;
        }

        public string GetNpcDefaultTalk(int npcId)
        {
            return StringProvider.GetSubProvider(StringCategory.Npc)?.GetRequiredItem<StringNpcTemplate>(npcId)?.DefaultTalk ?? "(...)";
        }

        public string GetItemMessage(int itemId)
        {
            return StringProvider.GetSubProvider(StringCategory.Item)?.GetRequiredItem<StringTemplate>(itemId)?.Message ?? string.Empty;
        }

        public string? GetItemName(int itemId)
        {
            return StringProvider.GetSubProvider(StringCategory.Item)?.GetRequiredItem<StringTemplate>(itemId)?.Name;
        }

        public string GetMapName(int mapId)
        {
            return StringProvider.GetSubProvider(StringCategory.Map).GetRequiredItem<StringMapTemplate>(mapId)?.MapName ?? StringConstants.WZ_NoName;
        }

        public string GetMapStreetName(int mapId)
        {
            return StringProvider.GetSubProvider(StringCategory.Map).GetRequiredItem<StringMapTemplate>(mapId)?.StreetName ?? StringConstants.WZ_NoName;
        }

        public static ClientCulture SystemCulture = new ClientCulture();
    }

}
