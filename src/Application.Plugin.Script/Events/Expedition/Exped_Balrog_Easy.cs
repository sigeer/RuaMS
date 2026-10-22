using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;
using Application.Core.Server.life;
using Application.Shared.Battle;
using System.Drawing;

namespace Application.Plugin.Script.Events
{
    /// <summary>
    /// 简单巴洛古远征。
    /// 对应 JS scripts/event/BalrogBattle_Easy.js。
    /// </summary>
    internal class Exped_Balrog_Easy : AbstractExpeditionEventTemplate
    {
        const int BossMobId = 8830010;

        public Exped_Balrog_Easy() : base(8830007, nameof(Exped_Balrog_Easy), ExpeditionEntryType.BALROG_EASY.Name)
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
            var balrogId = mob.getId() - BossId;
            if (balrogId < 0 || balrogId > 2)
                return;

            var count = eim.getIntProperty("boss");
            if (count >= 2)
            {
                await eim.showClearEffect(mob.getMap().getId());
                await eim.clearPQ();

                await eim.dispatchRaiseQuestMobCount(BossMobId, EntryMap);
            }
            else
            {
                if (count == 1)
                {
                    var map = await eim.getInstanceMap(EntryMap);
                    var body = map?.getMonsterById(BossId);
                    if (map != null && body != null)
                    {
                        await map.makeMonsterReal(body);
                    }
                }

                eim.setIntProperty("boss", count + 1);
            }

            // 巴洛古本体被击杀后，10 秒后重新召唤封印形态
            if (mob.getId() == BossId)
            {
                eim.Schedule(SpawnSealedBalrog, 10_000);
            }
        }

        async Task SpawnSealedBalrog(AbstractEventInstanceManager eim)
        {
            var map = await eim.getInstanceMap(EntryMap);
            if (map == null)
                return;

            var mob = Application.Core.Server.life.LifeFactory.Instance.GetMonsterTrust(BossMobId);
            await map.spawnMonsterOnGroundBelow(mob, new System.Drawing.Point(412, 258));
        }
    }
}
