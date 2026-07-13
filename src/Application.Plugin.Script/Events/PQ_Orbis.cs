using Application.Core.Game.Players;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;

namespace Application.Plugin.Script.Events
{
    internal class PQ_Orbis : AbstractPartyQuestEventTemplate
    {
        public PQ_Orbis()
            : base(nameof(PQ_Orbis))
        {
            MinCount = 5;
            MaxCount = 6;
            MinLevel = 51;
            MaxLevel = 70;
            EntryMap = 920010000;
            ExitMap = 920011200;
            RecruitMap = 200080101;
            ClearMap = 920011300;
            MinMap = 920010000;
            MaxMap = 920011300;
            EventTime = 45 * 60;
            Type = Shared.Events.EventInstanceType.PartyQuest;

            AllClearRewards = new()
            {
                {
                    1,
                    new Core.model.RewardPools(
                        [new(2040602, 1), new(2040802, 1), new(2040002, 1), new(2040402, 1), new(2040505, 1), new(2040502, 1), new(2040601, 1), new(2044501, 1), new(2044701, 1), new(2044601, 1), new(2041019, 1), new(2041016, 1), new(2041022, 1), new(2041013, 1), new(2041007, 1), new(2043301, 1), new(2040301, 1), new(2040801, 1), new(2040001, 1), new(2040004, 1), new(2040504, 1), new(2040501, 1), new(2040513, 1), new(2043101, 1), new(2044201, 1), new(2044401, 1), new(2040701, 1), new(2044301, 1), new(2043801, 1), new(2040401, 1), new(2043701, 1), new(2040803, 1), new(2000003, 100), new(2000002, 100), new(2000004, 15), new(2000006, 80), new(2000005, 5), new(2022000, 25), new(2001001, 20), new(2001002, 20), new(2022003, 25), new(2001000, 20), new(2020014, 15), new(2020015, 10), new(4003000, 45), new(1102015, 1), new(1102016, 1), new(1102017, 1), new(1102018, 1), new(1102021, 1), new(1102022, 1), new(1102023, 1), new(1102024, 1), new(1102084, 1), new(1102085, 1), new(1102086, 1), new(1032019, 1), new(1032020, 1), new(1032021, 1), new(1032014, 1), new(2070011, 1), new(4010003, 15), new(4010000, 15), new(4010006, 10), new(4010002, 15), new(4010005, 15), new(4010004, 15), new(4010001, 15), new(4020001, 15), new(4020002, 15), new(4020008, 10), new(4020007, 10), new(4020003, 15), new(4020000, 15), new(4020004, 15), new(4020005, 15), new(4020006, 15), new(2210000, 5), new(2210001, 5), new(2210002, 5), new(2070006, 1), new(2070005, 1), new(2070007, 1), new(2070004, 1), new(2061003, 2000), new(2060003, 2000), new(2060004, 2000), new(2061004, 2000), new(2100000, 1), new(2100001, 1), new(2100002, 1), new(2100003, 1), new(2100004, 1), new(2100005, 1)],
                        [],
                        []
                    )
                },
            };
        }

        public override async Task OnSetup(AbstractEventInstanceManager eim, int level, int lobbyId)
        {
            eim.setProperty("statusStg0", -1);
            eim.setProperty("statusStg1", -1);
            eim.setProperty("statusStg2", -1);
            eim.setProperty("statusStg3", -1);
            eim.setProperty("statusStg4", -1);
            eim.setProperty("statusStg5", -1);
            eim.setProperty("statusStg6", -1);
            eim.setProperty("statusStg7", -1);
            eim.setProperty("statusStg8", -1);
            eim.setProperty("statusStg2_c", 0);
            eim.setProperty("statusStg7_c", 0);
            eim.setProperty("statusStgBonus", 0);

            var d = DateTime.Now.DayOfWeek;
            (await eim.getInstanceMap(920010400))?.getReactorByName("music")?.setEventState((sbyte)d);

            await base.OnSetup(eim, level, lobbyId);
        }

        public override async Task AfterSeup(AbstractEventInstanceManager eim)
        {
            var eventJobs = eim.getEventPlayersJobs();
            var rangeJobs = Convert.ToInt32("111110", 2);

            var isTeamAllJob = ((eventJobs & rangeJobs) == rangeJobs);
            if (isTeamAllJob)
            {
                await eim.applyEventPlayersItemBuff(2022090 + Random.Shared.Next(4));
            }
            await base.AfterSeup(eim);
        }

        public override async Task OnPlayerEntry(AbstractEventInstanceManager eim, Player chr)
        {
            await chr.Dialog("你好，我是女神的内侍官#e#b帮佣易克#n#k。\r\n别紧张，你现在还看不到我。\r\n当女神石化时，我也同时失去了力量，所以现在你只能看到我微弱的闪光。\r\n如果你能收集到魔法云的力量，我就能恢复形体变回原样。\r\n请帮我收集 #b#e20#n#k 朵 #b#e#v4001063##t4001063##k#n 带回来。");
            await base.OnPlayerEntry(eim, chr);
        }

        public override async Task OnTimeOut(AbstractEventInstanceManager eim)
        {
            if (eim.getIntProperty("statusStg8") == 1)
            {
                await eim.warpEventTeam(920011300);
            }
            else
            {
                await End(eim, Core.scripting.Events.Abstraction.TerminationReason.Timeout);
            }
        }
        public override async Task OnPlayerUnregister(AbstractEventInstanceManager eim, Player chr)
        {
            await chr.cancelEffect(2022090);
            await chr.cancelEffect(2022091);
            await chr.cancelEffect(2022092);
            await chr.cancelEffect(2022093);
            await base.OnPlayerUnregister(eim, chr);
        }
    }
}
