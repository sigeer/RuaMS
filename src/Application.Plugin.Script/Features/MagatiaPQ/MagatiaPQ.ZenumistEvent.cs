using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;

namespace Application.Plugin.Script.Features.MagatiaPQ
{
    /// <summary>
    /// 泽尼姆斯特（Zenumist）分支的事件模板。
    /// 结构与 <see cref="AlcadnoEvent"/> 相同，只有地图号基数、护送怪与出口反应堆不同。
    /// </summary>
    internal sealed class ZenumistEvent : AbstractPartyQuestEventTemplate
    {
        public ZenumistEvent() : base(MagatiaPQFeature.ZenumistEventName)
        {
            MinCount = 4;
            MaxCount = 4;
            MinLevel = 71;
            MaxLevel = 85;

            RecruitMap = 261000011;
            EntryMap = MagatiaPQFeature.ZenumistMapBase;
            ExitMap = MagatiaPQFeature.ZenumistMapBase + 700;
            ClearMap = MagatiaPQFeature.ZenumistMapBase + 700;

            MinMap = MagatiaPQFeature.ZenumistMapBase;
            MaxMap = MagatiaPQFeature.ZenumistMapBase + 600;

            EventTime = 45 * 60;
            Type = Shared.Events.EventInstanceType.PartyQuest;
        }

        public override async Task OnMobKilled(AbstractEventInstanceManager eim, Monster mob, ICombatantObject? killer)
        {
            var map = mob.getMap();
            var mapId = map.getId();
            var mapBase = MagatiaPQFeature.ZenumistMapBase;

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

                    var reactor = map.getReactorByName("rnj6_out");
                    if (reactor != null)
                    {
                        await reactor.forceHitReactor(1);
                    }
                }
            }
            else if (mob.getId() == 9300139 || mob.getId() == 9300140)
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
