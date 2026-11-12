using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;
using Application.Core.Server.life;
using Application.Shared.Battle;
using System.Drawing;

namespace Application.Plugin.Script.Features.Exped_Balrog
{
    internal class Exped_Balrog : AbstractExpeditionEventTemplate
    {
        public Exped_Balrog() : base(Exped_BalrogFeature.SealedBossId, Exped_BalrogFeature.EventName, ExpeditionEntryType.BALROG_NORMAL.Name)
        {
            MinCount = 6;
            MaxCount = 30;

            MinLevel = 50;
            MaxLevel = 255;

            EntryMap = 105100300;
            ExitMap = 105100100;
            ClearMap = 105100301;
            RecruitMap = 105100100;

            MinMap = 105100300;
            MaxMap = 105100301;

            EventTime = 60 * 60;
            RegistrationTime = 5 * 60;
        }

        public override async Task OnMobKilled(AbstractEventInstanceManager eim, Monster mob, ICombatantObject? killer)
        {
            // 未封印的巴洛古：8830000 / 8830001 / 8830002
            var balrogId = mob.getId() - Exped_BalrogFeature.SealedBossId;
            if (balrogId < 0 || balrogId > 2)
                return;

            var count = eim.getIntProperty("boss");
            if (count == 2)
            {
                await eim.showClearEffect();
                await eim.clearPQ();

                await eim.dispatchRaiseQuestMobCount(BossId, EntryMap);
                await eim.dispatchRaiseQuestMobCount(9101003, EntryMap); // thanks Atoot for noticing quest not getting updated after boss kill

                var leaderName = eim.getLeader()?.getName() ?? string.Empty;
                var map = mob.getMap();
                await eim.EventManager.ChannelServer.NodeActor.Send(s =>
                {
                    s.SendDropMessage(6, "[Victory] " + leaderName + "'s party has successfully defeated the Balrog! Praise to them, they finished with " + map.countAlivePlayers() + " players alive.", false);
                });
            }
            else
            {
                if (count == 1)
                {
                    var mapObj = await eim.getInstanceMap(EntryMap);
                    if (mapObj != null)
                        await mapObj.makeMonsterReal(mapObj.getMonsterById(Exped_BalrogFeature.SealedBossId));
                }
                eim.setIntProperty("boss", count + 1);
            }

            // 本体死了以后 10 秒才重刷封印形态
            if (mob.getId() == BossId)
            {
                eim.Schedule(async instance =>
                {
                    var mapObj = await instance.getInstanceMap(EntryMap);
                    if (mapObj != null)
                        await mapObj.spawnMonsterOnGroundBelow(LifeFactory.Instance.GetMonsterTrust(Exped_BalrogFeature.BossMobId), new Point(412, 258));
                }, 10_000);
            }
        }


    }
}
