using Application.Core.Client;
using Application.Core.Game.Players;
using Application.Core.model;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.scripting.Events.Templates;
using Application.Resources.Messages;
using Application.Shared.Quest;

namespace Application.Plugin.Script.Features.MonsterCarnival
{
    /// <summary>
    /// 怪物嘉年华房间模板基类：招募地图由子类指定，招募地图 +1 即入场地图。
    /// 两个世代（CPQ1 / CPQ2）只有等级段与地图组不同。
    /// </summary>
    internal abstract class MonsterCarnivalRoomTemplate : AbstractMonsterCarnivalEventTemplate
    {
        protected MonsterCarnivalRoomTemplate(string name, int recruitMap, int exitMap, int minLevel, int maxLevel)
            : base(name, recruitMap + 1)
        {
            QuestId = (short)ExQuestId.PQ_MC1;

            RecruitMap = recruitMap;
            EntryMap = recruitMap + 1;

            ExitMap = exitMap;

            MinMap = recruitMap;
            MaxMap = recruitMap + 99;

            MinLevel = minLevel;
            MaxLevel = maxLevel;

            AllClearRewards = new()
            {
                { 1, new([new(4001129, 1), new(4001129, 2)], [], [50000, 25500, 21000, 19505, 17500, 12000, 5000, 2500]) }
            };
        }

        public override RewardOptions GetAllClearRewardOptions(Player chr, int point = 1)
        {
            return new RewardOptions(ExpPoolIndex: point);
        }

        public override string? HandleCreateInstanceResult(CreateInstanceResult r, IChannelClient c)
        {
            switch (r)
            {
                case CreateInstanceResult.Success:
                    return c.CurrentCulture.GetTalkMessage(ScriptMessage.CPQ_EntryLobby);
                case CreateInstanceResult.RequiredParty:
                    return c.CurrentCulture.GetTalkMessage(ScriptMessage.MC_NeedCreate);
                case CreateInstanceResult.RequiredLeader:
                    return c.CurrentCulture.GetTalkMessage(ScriptMessage.MC_WantStart);
                case CreateInstanceResult.Requirement:
                    return c.CurrentCulture.GetTalkMessage(ScriptMessage.MC_CreateRequirement);
                case CreateInstanceResult.LobbyLimited:
                case CreateInstanceResult.Disposed:
                case CreateInstanceResult.Unknown:
                default:
                    return c.CurrentCulture.GetTalkMessage(ScriptMessage.CPQ_Error);
            }
        }

        public override string? HandleJoinRequestResult(MCJoinInstanceResult r, IChannelClient c)
        {
            switch (r)
            {
                case MCJoinInstanceResult.Success:
                    return c.CurrentCulture.GetTalkMessage(ScriptMessage.CPQ_ChallengeRoomSent);
                case MCJoinInstanceResult.RequiredParty:
                    return c.CurrentCulture.GetTalkMessage(ScriptMessage.MC_NeedCreate);
                case MCJoinInstanceResult.RequiredLeader:
                    return c.CurrentCulture.GetTalkMessage(ScriptMessage.MC_WantStart);
                case MCJoinInstanceResult.Requirement:
                    return c.CurrentCulture.GetTalkMessage(ScriptMessage.MC_JoinRequirement);
                case MCJoinInstanceResult.NotInWaiting:
                    return c.CurrentCulture.GetTalkMessage(ScriptMessage.CPQ_FindError);
                case MCJoinInstanceResult.AnthorRequest:
                    return c.CurrentCulture.GetTalkMessage(ScriptMessage.CPQ_ChallengeRoomAnswer);
                case MCJoinInstanceResult.Unknown:
                default:
                    return c.CurrentCulture.GetTalkMessage(ScriptMessage.CPQ_Error);
            }
        }
    }
}
