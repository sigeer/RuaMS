using Application.Core.Channel;
using Application.Core.Game.Maps;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.Scripting.Events;
using Application.Utility;
using Application.Utility.Extensions;

namespace Application.Plugin.Script.Events
{
    internal class PQ_Ludi : PartyQuestEventManager
    {
        public PQ_Ludi(WorldChannel cserv) : base(cserv, nameof(PQ_Ludi))
        {
            MinCount = 5;
            MaxCount = 6;
            MinLevel = 35;
            MaxLevel = 50;

            EntryMap = 922010100;
            ExitMap = 922010000;
            RecruitMap = 221024500;
            ClearMap = 922011000;
            MinMap = 922010100;
            MaxMap = 922011100;

            EventTime = 45 * 60;
        }

        protected override void setEventRewards(AbstractEventInstanceManager eim)
        {
            List<object> itemSet = [2040602, 2040802, 2040002, 2040402, 2040505, 2040502, 2040601, 2044501, 2044701, 2044601, 2041019, 2041016, 2041022, 2041013, 2041007, 2043301, 2040301, 2040801, 2040001, 2040004, 2040504, 2040501, 2040513, 2043101, 2044201, 2044401, 2040701, 2044301, 2043801, 2040401, 2043701, 2040803, 2000003, 2000002, 2000004, 2000006, 2000005, 2022000, 2001001, 2001002, 2022003, 2001000, 2020014, 2020015, 4003000, 1102003, 1102004, 1102000, 1102002, 1102001, 1102011, 1102012, 1102013, 1102014, 1032011, 1032012, 1032013, 1032002, 1032008, 1032011, 2070011, 4010003, 4010000, 4010006, 4010002, 4010005, 4010004, 4010001, 4020001, 4020002, 4020008, 4020007, 4020003, 4020000, 4020004, 4020005, 4020006];
            List<object> itemQty = [1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 85, 85, 10, 60, 2, 20, 15, 15, 20, 15, 10, 5, 35, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 10, 10, 6, 10, 10, 10, 10, 10, 10, 4, 4, 10, 10, 10, 10, 10];
            eim.setEventRewards(1, itemSet, itemQty);

            List<object> expStages = [210, 2520, 2940, 3360, 3770, 0, 4620, 5040, 5950];    //bonus exp given on CLEAR stage signal
            eim.setEventClearStageExp(expStages);
        }

        public override void ClearPQ(AbstractEventInstanceManager eim)
        {
            base.ClearPQ(eim);

            // 奖励关
            eim.startEventTimer(1 * 60_000);
            eim.warpEventTeam(922011000);
        }

        public List<int> GetStage(AbstractEventInstanceManager eim, IMap map)
        {
            return eim.Properties.GetOrAdd($"stg{map.Id}Property", () => string.Join(',', Randomizer.Take(5, map.getAreas().Count))).Split(',').Select(int.Parse).OrderBy(x => x).ToList();
        }

        public override void OnTimeOut(AbstractEventInstanceManager eim)
        {
            if (eim.ClearedMaps.GetValueOrDefault(922011000) == StageStatus.Completed)
            {
                eim.warpEventTeam(922011000, 922011100);
            }
            else
            {
                base.OnTimeOut(eim);
            }
        }
    }
}
