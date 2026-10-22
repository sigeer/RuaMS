using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;

namespace Application.Plugin.Script.Events
{
    /// <summary>
    /// 玛加提亚组队任务（Alcadno）。
    /// 对应 JS scripts/event/MagatiaPQ_A.js。
    /// </summary>
    internal class MagatiaPQ_A : AbstractPartyQuestEventTemplate
    {
        public MagatiaPQ_A() : base(nameof(MagatiaPQ_A))
        {
            MinCount = 4;
            MaxCount = 4;
            MinLevel = 71;
            MaxLevel = 85;

            RecruitMap = 261000021;
            EntryMap = 926110000;
            ExitMap = 926110700;
            ClearMap = 926110700;

            MinMap = 926110000;
            MaxMap = 926110600;

            EventTime = 45 * 60;
            Type = Shared.Events.EventInstanceType.PartyQuest;
        }

        public override async Task OnMobKilled(AbstractEventInstanceManager eim, Monster mob, ICombatantObject? killer)
        {
            var map = mob.getMap();
            var mapId = map.getId();

            if (mapId == 926110001 && eim.getIntProperty("statusStg1") == 1)
            {
                if (map.countMonsters() == 0)
                {
                    await eim.showClearEffect(mapId);
                    await eim.GiveStageClearRewardAll(926110001);
                    eim.setIntProperty("statusStg2", 1);
                }
            }
            else if (mapId == 926110203 && eim.getIntProperty("statusStg1") == 1)
            {
                if (map.countMonsters() == 0)
                {
                    await eim.showClearEffect(mapId);
                    await eim.GiveStageClearRewardAll(926110203);

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
