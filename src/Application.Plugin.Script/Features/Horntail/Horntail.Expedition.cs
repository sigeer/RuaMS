using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.Server.life;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;
using Application.Shared.Battle;
using Application.Shared.Constants.Mob;

namespace Application.Plugin.Script.Features.Horntail
{
    internal sealed class Expedition : AbstractExpeditionEventTemplate
    {
        public Expedition() : base(MobId.HORNTAIL, HorntailFeature.ExpeditionEventName, ExpeditionEntryType.HORNTAIL.Name)
        {
            MinCount = 6;
            MaxCount = 30;

            MinLevel = 100;
            MaxLevel = 255;

            RecruitMap = 240050400;
            EntryMap = 240060000;
            ExitMap = 240050600;
            ClearMap = 240050600;

            MinMap = 240060000;
            MaxMap = 240060200;

            EventTime = 120 * 60;
            RegistrationTime = 5 * 60;
        }

        public override async Task OnSetup(AbstractEventInstanceManager eim, int level, int lobbyId)
        {
            eim.setIntProperty("canJoin", 1);
            eim.setIntProperty("defeatedBoss", 0);
            eim.setIntProperty("defeatedHead", 0);

            // 左右头在两张地图上，WZ 的 life 里没有，必须由事件刷出；
            // 否则 defeatedHead 恒为 0，hontale_BR 封印门永远打不开
            var leftMap = await eim.getInstanceMap(240060000);
            await leftMap.spawnMonsterOnGroundBelow(LifeFactory.Instance.GetMonsterTrust(8810000), new System.Drawing.Point(960, 120));

            var rightMap = await eim.getInstanceMap(240060100);
            await rightMap.spawnMonsterOnGroundBelow(LifeFactory.Instance.GetMonsterTrust(8810001), new System.Drawing.Point(-420, 120));

            await base.OnSetup(eim, level, lobbyId);
        }

        public override async Task OnMobKilled(AbstractEventInstanceManager eim, Monster mob, ICombatantObject? killer)
        {
            if (mob.getId() == BossId)
            {
                eim.setIntProperty("defeatedBoss", 1);
                await eim.showClearEffect(mob.getMap().getId());
                await eim.clearPQ();

                await eim.dispatchRaiseQuestMobCount(8810018, 240060200);
                await eim.EventManager.ChannelServer.NodeActor.Send(s =>
                {
                    s.SendDropMessage(6,
                        "[Victory] To the crew that have finally conquered Horned Tail after numerous attempts, I salute thee! You are the true heroes of Leafre!!", false);
                });
            }
            else if ((mob.getId() == MobId.HORNTAIL_PREHEAD_LEFT || mob.getId() == MobId.HORNTAIL_PREHEAD_RIGHT))
            {
                var killed = eim.getIntProperty("defeatedHead");
                eim.setIntProperty("defeatedHead", killed + 1);
                await eim.showClearEffect(mob.getMap().getId());
            }
        }
    }
}
