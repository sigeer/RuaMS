using Application.Core.Game.Maps;
using Application.Core.model;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;
using Application.Utility;
using Application.Utility.Extensions;

namespace Application.Plugin.Script.Features.KerningPQ
{
    internal sealed class Event : AbstractPartyQuestEventTemplate
    {
        public Event() : base(KerningPQFeature.EventName)
        {
            QuestId = 1201;
            MinCount = 3;
            MaxCount = 6;
            MinLevel = 21;
            MaxLevel = 30;
            EntryMap = KerningPQFeature.Stage1Map;
            ExitMap = KerningPQFeature.ExitMapId;
            RecruitMap = 103000000;
            ClearMap = KerningPQFeature.BonusMap;
            MinMap = KerningPQFeature.Stage1Map;
            MaxMap = KerningPQFeature.BonusMap;
            EventTime = 30 * 60;
            Type = Shared.Events.EventInstanceType.PartyQuest;

            AllClearRewards = new()
            {
                {
                    1,
                    new RewardPools(
                        [
                            new(2040505, 1), new(2040514, 1), new(2040502, 1), new(2040002, 1),
                            new(2040602, 1), new(2040402, 1), new(2040802, 1), new(1032009, 1),
                            new(1032004, 1), new(1032005, 1), new(1032006, 1), new(1032007, 1),
                            new(1032010, 1), new(1032002, 1), new(1002026, 1), new(1002089, 1),
                            new(1002090, 1), new(2000003, 80), new(2000001, 80), new(2000002, 80),
                            new(2000006, 50), new(2022003, 5), new(2022000, 15), new(2000004, 15),
                            new(4003000, 30), new(4010000, 15), new(4010001, 15), new(4010002, 15),
                            new(4010003, 15), new(4010004, 8), new(4010005, 8), new(4010006, 8),
                            new(4010007, 8), new(4020000, 8), new(4020001, 8), new(4020002, 8),
                            new(4020003, 8), new(4020004, 8), new(4020005, 8), new(4020006, 8),
                            new(4020007, 3), new(4020008, 3),
                        ],
                        [],
                        []
                    )
                },
            };
            StageClearRewards = new()
            {
                { KerningPQFeature.Stage1Map, new(100, 0) },
                { KerningPQFeature.Stage2Map, new(200, 0) },
                { KerningPQFeature.Stage3Map, new(400, 0) },
                { KerningPQFeature.Stage4Map, new(800, 0) },
                { KerningPQFeature.Stage5Map, new(1500, 0) },
            };
        }

        public (string[] Quest, int[] Answer) GetStage1()
        {
            string[] questions =
            [
                "收集与#b战士#n首次转职所需最低等级相同数量的#b通行证#n。",
                "收集与#b战士#n首次转职所需最低力量（STR）相同数量的#b通行证#n。",
                "收集与#b魔法师#n首次转职所需最低智力（INT）相同数量的#b通行证#n。",
                "收集与#b弓箭手#n首次转职所需最低敏捷（DEX）相同数量的#b通行证#n。",
                "收集与#b飞侠#n首次转职所需最低敏捷（DEX）相同数量的#b通行证#n。",
                "收集与二次转职所需最低等级相同数量的#b通行证#n。",
                "收集与#b魔法师#n首次转职所需最低等级相同数量的#b通行证#n。",
            ];
            int[] answers = [10, 35, 20, 25, 25, 30, 8];
            return (questions, answers);
        }

        /// <summary>
        /// 第 2~4 阶段的正确答案：从该地图的区域中随机取 3 个。
        /// </summary>
        public HashSet<int> GetStage(AbstractEventInstanceManager eim, IMap map)
        {
            return eim
                .Props.GetOrAdd(
                    $"stg{map.Id}Property",
                    () => string.Join(',', Randomizer.Take(3, map.getAreas().Count))
                ).ToString()!
                .Split(',')
                .Select(int.Parse)
                .ToHashSet();
        }
    }
}
