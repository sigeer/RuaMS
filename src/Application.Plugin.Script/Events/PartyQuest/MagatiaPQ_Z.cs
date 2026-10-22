using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;

namespace Application.Plugin.Script.Events
{
    /// <summary>
    /// 玛加提亚组队任务（Zenumist）。
    /// 对应 JS scripts/event/MagatiaPQ_Z.js。
    /// </summary>
    internal class MagatiaPQ_Z : AbstractPartyQuestEventTemplate
    {
        public MagatiaPQ_Z() : base(nameof(MagatiaPQ_Z))
        {
            MinCount = 4;
            MaxCount = 4;
            MinLevel = 71;
            MaxLevel = 85;

            RecruitMap = 261000011;
            EntryMap = 926100000;
            ExitMap = 926100700;
            ClearMap = 926100700;

            MinMap = 926100000;
            MaxMap = 926100600;

            EventTime = 45 * 60;
            Type = Shared.Events.EventInstanceType.PartyQuest;
        }

        public override async Task OnMobKilled(AbstractEventInstanceManager eim, Monster mob, ICombatantObject? killer)
        {
            var map = mob.getMap();
            var mapId = map.getId();

            if (mapId == 926100001 && eim.getIntProperty("statusStg1") == 1)
            {
                if (map.countMonsters() == 0)
                {
                    await eim.showClearEffect(mapId);
                    await eim.GiveStageClearRewardAll(926100001);
                    eim.setIntProperty("statusStg2", 1);
                }
            }
            else if (mapId == 926100203 && eim.getIntProperty("statusStg1") == 1)
            {
                if (map.countMonsters() == 0)
                {
                    await eim.showClearEffect(mapId);
                    await eim.GiveStageClearRewardAll(926100203);

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
