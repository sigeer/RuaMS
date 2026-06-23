using Application.Core.Game.Maps;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;
using Application.Utility;
using Application.Utility.Extensions;

namespace Application.Plugin.Script.Events
{
    internal class PQ_Ludi : AbstractPartyQuestEventTemplate
    {
        public PQ_Ludi()
            : base(nameof(PQ_Ludi))
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

            AllClearRewards = new()
            {
                {
                    1,
                    new Core.model.RewardPools(
                        [
                            new(2040602, 1),
                            new(2040802, 1),
                            new(2040002, 1),
                            new(2040402, 1),
                            new(2040505, 1),
                            new(2040502, 1),
                            new(2040601, 1),
                            new(2044501, 1),
                            new(2044701, 1),
                            new(2044601, 1),
                            new(2041019, 1),
                            new(2041016, 1),
                            new(2041022, 1),
                            new(2041013, 1),
                            new(2041007, 1),
                            new(2043301, 1),
                            new(2040301, 1),
                            new(2040801, 1),
                            new(2040001, 1),
                            new(2040004, 1),
                            new(2040504, 1),
                            new(2040501, 1),
                            new(2040513, 1),
                            new(2043101, 1),
                            new(2044201, 1),
                            new(2044401, 1),
                            new(2040701, 1),
                            new(2044301, 1),
                            new(2043801, 1),
                            new(2040401, 1),
                            new(2043701, 1),
                            new(2040803, 1),
                            new(2000003, 85),
                            new(2000002, 85),
                            new(2000004, 10),
                            new(2000006, 60),
                            new(2000005, 2),
                            new(2022000, 20),
                            new(2001001, 15),
                            new(2001002, 15),
                            new(2022003, 20),
                            new(2001000, 15),
                            new(2020014, 10),
                            new(2020015, 5),
                            new(4003000, 35),
                            new(1102003, 1),
                            new(1102004, 1),
                            new(1102000, 1),
                            new(1102002, 1),
                            new(1102001, 1),
                            new(1102011, 1),
                            new(1102012, 1),
                            new(1102013, 1),
                            new(1102014, 1),
                            new(1032011, 1),
                            new(1032012, 1),
                            new(1032013, 1),
                            new(1032002, 1),
                            new(1032008, 1),
                            new(1032011, 1),
                            new(2070011, 1),
                            new(4010003, 10),
                            new(4010000, 10),
                            new(4010006, 6),
                            new(4010002, 10),
                            new(4010005, 10),
                            new(4010004, 10),
                            new(4010001, 10),
                            new(4020001, 10),
                            new(4020002, 10),
                            new(4020008, 4),
                            new(4020007, 4),
                            new(4020003, 10),
                            new(4020000, 10),
                            new(4020004, 10),
                            new(4020005, 10),
                            new(4020006, 10),
                        ],
                        [],
                        []
                    )
                },
            };

            StageClearRewards = new()
            {
                { 922010100, new(210, 0) },
                { 922010200, new(2520, 0) },
                { 922010300, new(2940, 0) },
                { 922010400, new(3360, 0) },
                { 922010500, new(3770, 0) },
                { 922010600, new(0, 0) },
                { 922010700, new(4620, 0) },
                { 922010800, new(5950, 0) },
            };
        }

        public override async Task ClearPQ(AbstractEventInstanceManager eim)
        {
            await base.ClearPQ(eim);

            // 奖励关
            await eim.startEventTimer(1 * 60_000);
            await eim.warpEventTeam(922011000);
        }

        public HashSet<int> GetStage(AbstractEventInstanceManager eim, IMap map)
        {
            return eim
                .Properties.GetOrAdd(
                    $"stg{map.Id}Property",
                    () => string.Join(',', Randomizer.Take(5, map.getAreas().Count))
                )
                .Split(',')
                .Select(int.Parse)
                .ToHashSet();
        }

        public override async Task OnTimeOut(AbstractEventInstanceManager eim)
        {
            if (eim.ClearedMaps.GetValueOrDefault(922011000) == StageStatus.Completed)
            {
                await eim.warpEventTeam(922011000, 922011100);
            }
            else
            {
                await base.OnTimeOut(eim);
            }
        }
    }
}
