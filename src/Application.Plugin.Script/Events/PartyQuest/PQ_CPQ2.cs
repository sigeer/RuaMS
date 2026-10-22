using Application.Core.Game.Players;
using Application.Core.model;
using Application.Core.scripting.Events.Templates;
using Application.Shared.Quest;

namespace Application.Plugin.Script.Events
{
    /// <summary>
    /// 怪物嘉年华 2（Monster Carnival 2）。
    /// 地图组为 980031xxx / 980032xxx / 980033xxx，每代各 3 个房间。
    /// </summary>
    internal sealed class PQ_CPQ2 : AbstractMonsterCarnivalEventTemplate
    {
        public PQ_CPQ2(string name, int recruitMap) : base(name, recruitMap + 1)
        {
            QuestId = (short)ExQuestId.PQ_MC1;

            RecruitMap = recruitMap;
            EntryMap = recruitMap + 1;

            ExitMap = 980030010;

            MinMap = recruitMap;
            MaxMap = recruitMap + 99;

            MinLevel = 51;
            MaxLevel = 70;

            AllClearRewards = new()
            {
                { 1, new([new (4001129, 1),new (4001129, 2)],[],[50000, 25500, 21000, 19505, 17500, 12000, 5000, 2500] ) }
            };
        }

        public override RewardOptions GetAllClearRewardOptions(Player chr, int point = 1)
        {
            return new RewardOptions(ExpPoolIndex: point);
        }
    }
}
