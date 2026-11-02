using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;

namespace Application.Plugin.Script.Features.MagatiaPQ
{
    /// <summary>
    /// 阿尔卡德诺（Alcadno）分支的事件模板。
    /// 前三阶段靠地图清怪推进，最后以护送怪 9300151 / 9300152 击杀结束。
    /// </summary>
    internal sealed class AlcadnoEvent : AbstractPartyQuestEventTemplate
    {
        public AlcadnoEvent() : base(MagatiaPQFeature.AlcadnoEventName)
        {
            MinCount = 4;
            MaxCount = 4;
            MinLevel = 71;
            MaxLevel = 85;

            RecruitMap = 261000021;
            EntryMap = MagatiaPQFeature.AlcadnoMapBase;
            ExitMap = MagatiaPQFeature.AlcadnoMapBase + 700;
            ClearMap = MagatiaPQFeature.AlcadnoMapBase + 700;

            MinMap = MagatiaPQFeature.AlcadnoMapBase;
            MaxMap = MagatiaPQFeature.AlcadnoMapBase + 600;

            EventTime = 45 * 60;
            Type = Shared.Events.EventInstanceType.PartyQuest;
        }

        public override async Task OnMobKilled(AbstractEventInstanceManager eim, Monster mob, ICombatantObject? killer)
        {
            var map = mob.getMap();
            var mapId = map.getId();
            var mapBase = MagatiaPQFeature.AlcadnoMapBase;

            if (mapId == mapBase + 1 && eim.getIntProperty("statusStg1") == 1)
            {
                if (map.countMonsters() == 0)
                {
                    await eim.showClearEffect(mapId);
                    await eim.GiveStageClearRewardAll(mapId);
                    eim.setIntProperty("statusStg2", 1);
                }
            }
            else if (mapId == mapBase + 203 && eim.getIntProperty("statusStg1") == 1)
            {
                if (map.countMonsters() == 0)
                {
                    await eim.showClearEffect(mapId);
                    await eim.GiveStageClearRewardAll(mapId);

                    var reactor = map.getReactorByName("jnr6_out");
                    if (reactor != null)
                    {
                        await reactor.forceHitReactor(1);
                    }
                }
            }
            else if (mob.getId() == 9300151 || mob.getId() == 9300152)
            {
                await eim.showClearEffect(mapId);
                await eim.GiveStageClearRewardAll(mapId);

                var gain = eim.getIntProperty("escortFail") == 1
                    ? 90000
                    : (mob.getId() == 9300139 ? 105000 : 140000);
                await eim.giveEventPlayersExp(gain);

                await map.killAllMonstersNotFriendly();

                if (mob.getId() == 9300139)
                {
                    eim.setIntProperty("normalClear", 1);
                }

                await eim.clearPQ();
            }
        }
    }
}
