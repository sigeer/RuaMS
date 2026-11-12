using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;
using Application.Shared.Battle;
using Application.Shared.Constants.Mob;

namespace Application.Plugin.Script.Features.Zakum
{
    internal sealed class Expedition : AbstractExpeditionEventTemplate
    {
        public Expedition() : base(MobId.ZAKUM_3, ZakumFeature.ExpeditionEventName, ExpeditionEntryType.ZAKUM.Name)
        {
            MinCount = 6;
            MaxCount = 30;

            MinLevel = 50;
            MaxLevel = 255;

            RecruitMap = 211042400;
            EntryMap = 280030000;
            ExitMap = 211042400;
            ClearMap = 211042400;

            MinMap = 280030000;
            MaxMap = 280030000;

            EventTime = 120 * 60;
            RegistrationTime = 5 * 60;
        }

        /// <summary>
        /// 扎昆入口闸门：211042300 上的反应堆 2118002。开战时打开，通关/超时时复位。
        /// </summary>
        const int GateMapId = 211042300;
        const int GateReactorId = 2118002;

        async Task UpdateGateState(AbstractEventInstanceManager eim, sbyte state)
        {
            var map = await eim.EventManager.ChannelServer.getMapFactory().getMap(GateMapId);
            var reactor = map.getReactorById(GateReactorId);
            if (reactor != null)
                await reactor.forceHitReactor(state);
        }

        public override async Task OnBattleStarted(AbstractEventInstanceManager eim)
        {
            await base.OnBattleStarted(eim);
            await UpdateGateState(eim, 1);
        }

        public override async Task OnLobbyDisposed(AbstractEventInstanceManager eim)
        {
            await UpdateGateState(eim, 0);
        }

        public override async Task OnMobKilled(AbstractEventInstanceManager eim, Monster mob, ICombatantObject? killer)
        {
            if (mob.getId() == BossId)
            {
                eim.setIntProperty("defeatedBoss", 1);
                await eim.showClearEffect(mob.getMap().getId());
                await eim.clearPQ();

                await eim.EventManager.ChannelServer.NodeActor.Send(s =>
                {
                    s.SendDropMessage(6,
                        "[Victory] At last, the tree of evil that for so long overwhelmed Ossyria has fallen. To the crew that managed to finally conquer Zakum, after numerous attempts, victory! You are the true heroes of Ossyria!!", false);
                });
            }
        }
    }
}
