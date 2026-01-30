using Application.Resources.Messages;
using Application.Shared.Languages;
using Application.Templates.Exceptions;
using Application.Templates.Providers;
using Application.Templates.String;
using Application.Templates.XmlWzReader.Provider;
using Humanizer;
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
            StringProvider = ProviderSource.Instance.GetProviderByKey<StringProvider>(CultureInfo.Name)
                ?? throw new ProviderNotFoundException(nameof(StringProvider), $"没有找到{CultureInfo.Name}相应的wz资源");
        }

        public ClientCulture() : this(SupportedCultureManager.GetDefaultSupportedCulture())
        {
        }

        public string GetMessageByKey(string key, params object[] paramsValue)
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

        public string GetScriptTalkByKey(string key, params object[] paramsValue)
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

        /// <summary>
        /// 获取NPC对话文本
        /// </summary>
        /// <param name="npcId"></param>
        /// <param name="status">0: d0, 1: d1, 其他: 随机</param>
        /// <returns></returns>
        public string GetNpcDefaultTalk(int npcId, int status = 0)
        {
            var template = StringProvider.GetSubProvider(StringCategory.Npc)?.GetRequiredItem<StringNpcTemplate>(npcId);
            if (template == null)
            {
                return "(...)";
            }
            if (status == 0)
                return template.DefaultTalk0;
            else if (status == 1)
                return template.DefaultTalk1;
            else
                return (Randomizer.nextInt(100) % 2 == 0) ? template.DefaultTalk0 : template.DefaultTalk1;
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

        public string GetJobName(Job job)
        {
            return ClientMessage.ResourceManager.GetString($"Job_{job.Id}", CultureInfo) ?? job.ToString();
        }

        public string Ordinal(int i)
        {
            return i.Ordinalize(CultureInfo);
        }

        public string Number(int i)
        {
            return i.ToString("N", CultureInfo);
        }
        public static ClientCulture SystemCulture = new ClientCulture();
    }

}
