using Application.Shared.Constants.Job;
using Application.Templates.Reader;
using System.Globalization;

namespace Application.Shared.MapObjects.Players
{
    public interface IClientCulture
    {
        CultureInfo CultureInfo { get; }
        string GetItemMessage(int itemId);
        string? GetItemName(int itemId);
        string GetJobName(Job job);
        string GetMapName(int mapId);
        string GetMapStreetName(int mapId);
        string GetMessageByKey(string key, params object[] paramsValue);
        string GetMobName(int mobId);
        string GetNpcDefaultTalk(int npcId, int status = 0);
        string GetNpcName(int npcId);
        string? GetNullableMessageByKey(string key, params string[] paramsValue);
        string GetQuestName(int questId);
        string GetScriptTalkByKey(string key, params object[] paramsValue);
        string? GetSkillName(int skillId);
        string Number(int i);
        string Ordinal(int i);

        WzFindResult<WzFindMapResultItem> FindMapIdByName(string name);
        WzFindResult<WzFindResultItem> FindItemIdByName(string name);
        WzFindResult<WzFindResultItem> FindMobIdByName(string name);
        IStringProvider StringProvider { get; }
    }
}