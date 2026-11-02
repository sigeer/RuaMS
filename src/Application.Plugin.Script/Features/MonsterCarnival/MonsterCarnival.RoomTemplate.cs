using Application.Core.Game.Players;
using Application.Core.model;
using Application.Core.scripting.Events.Templates;
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
    }
}
