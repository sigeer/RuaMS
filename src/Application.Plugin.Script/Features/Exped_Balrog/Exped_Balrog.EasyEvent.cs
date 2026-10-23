using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;
using Application.Core.Server.life;
using Application.Shared.Battle;

namespace Application.Plugin.Script.Features.Exped_Balrog
{
    internal sealed class EasyEvent : AbstractExpeditionEventTemplate
    {
        public EasyEvent() : base(Exped_BalrogFeature.EasySealedBossId, Exped_BalrogFeature.EasyEventName, ExpeditionEntryType.BALROG_EASY.Name)
        {
            MinCount = 3;
            MaxCount = 30;
            MinLevel = 50;
            MaxLevel = 255;

            RecruitMap = 105100100;
            EntryMap = 105100400;
            ExitMap = 105100100;
            ClearMap = 105100401;

            MinMap = 105100400;
            MaxMap = 105100401;

            EventTime = 60 * 60;
        }

        public override async Task OnMobKilled(AbstractEventInstanceManager eim, Monster mob, ICombatantObject? killer)
        {
            // 未封印的巴洛古：8830007 / 8830008 / 8830009
            var balrogId = mob.getId() - Exped_BalrogFeature.EasySealedBossId;
            if (balrogId < 0 || balrogId > 2)
                return;

            var count = eim.getIntProperty("boss");
            if (count >= 2)
            {
                await eim.showClearEffect(mob.getMap().getId());
                await eim.clearPQ();

                await eim.dispatchRaiseQuestMobCount(Exped_BalrogFeature.EasyBossMobId, EntryMap);
            }
            else
            {
                if (count == 1)
                {
                    var map = await eim.getInstanceMap(EntryMap);
                    var body = map?.getMonsterById(Exped_BalrogFeature.EasySealedBossId);
                    if (map != null && body != null)
                    {
                        await map.makeMonsterReal(body);
                    }
                }

                eim.setIntProperty("boss", count + 1);
            }

            // 巴洛古本体被击杀后，10 秒后重新召唤封印形态
            if (mob.getId() == Exped_BalrogFeature.EasySealedBossId)
            {
                eim.Schedule(SpawnSealedBalrog, 10_000);
            }
        }

        async Task SpawnSealedBalrog(AbstractEventInstanceManager eim)
        {
            // 本体死亡后重新召唤的是另一种封印形态
            var map = await eim.getInstanceMap(EntryMap);
            if (map == null)
                return;

            var mob = LifeFactory.Instance.GetMonsterTrust(Exped_BalrogFeature.EasyBossMobId);
            await map.spawnMonsterOnGroundBelow(mob, Exped_BalrogFeature.EasySpawnPoint);
        }
    }
}
