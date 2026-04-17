using Application.Core.Channel;
using Application.Core.Game.Maps;
using Application.Core.Scripting.Events;
using Application.Utility;
using Application.Utility.Extensions;

namespace Application.Plugin.Script.Events
{
    internal class PQ_Kerning : PartyQuestEventManager
    {
        public PQ_Kerning(WorldChannel cserv) : base(cserv, nameof(PQ_Kerning))
        {
            MinCount = 3;
            MaxCount = 6;
            MinLevel = 21;
            MaxLevel = 30;
            EntryMap = 103000800;
            ExitMap = 103000890;
            RecruitMap = 103000000;
            ClearMap = 103000805;
            MinMap = 103000800;
            MaxMap = 103000805;
            EventTime = 30 * 60;
        }


        protected override void setEventRewards(AbstractEventInstanceManager eim)
        {
            int evLevel = 1;    //Rewards at clear PQ
            List<object> itemSet = [2040505, 2040514, 2040502, 2040002, 2040602, 2040402, 2040802, 1032009, 1032004, 1032005, 1032006, 1032007, 1032010, 1032002, 1002026, 1002089, 1002090, 2000003, 2000001, 2000002, 2000006, 2022003, 2022000, 2000004, 4003000, 4010000, 4010001, 4010002, 4010003, 4010004, 4010005, 4010006, 4010007, 4020000, 4020001, 4020002, 4020003, 4020004, 4020005, 4020006, 4020007, 4020008];
            List<object> itemQty = [1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 80, 80, 80, 50, 5, 15, 15, 30, 15, 15, 15, 15, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 3, 3];
            eim.setEventRewards(evLevel, itemSet, itemQty);

            List<object> expStages = [100, 200, 400, 800, 1500];    //bonus exp given on CLEAR stage signal
            eim.setEventClearStageExp(expStages);
        }

        public (string[] Quest, int[] Answer) GetStage1()
        {
            string[] stage1Questions = [
                     "收集与#b战士#n首次转职所需最低等级相同数量的#b通行证#n。",
                    "收集与#b战士#n首次转职所需最低力量（STR）相同数量的#b通行证#n。",
                    "收集与#b魔法师#n首次转职所需最低智力（INT）相同数量的#b通行证#n。",
                    "收集与#b弓箭手#n首次转职所需最低敏捷（DEX）相同数量的#b通行证#n。",
                    "收集与#b飞侠#n首次转职所需最低敏捷（DEX）相同数量的#b通行证#n。",
                    "收集与二次转职所需最低等级相同数量的#b通行证#n。",
                    "收集与#b魔法师#n首次转职所需最低等级相同数量的#b通行证#n。"
            ];
            int[] stage1Answers = [10, 35, 20, 25, 25, 30, 8];
            return (stage1Questions, stage1Answers);
        }

        public List<int> GetStage(AbstractEventInstanceManager eim, IMap map)
        {
            return eim.Properties.GetOrAdd($"stg{map.Id}Property", () => string.Join(',', Randomizer.Take(3, map.getAreas().Count))).Split(',').Select(int.Parse).OrderBy(x => x).ToList();
        }
    }
}
