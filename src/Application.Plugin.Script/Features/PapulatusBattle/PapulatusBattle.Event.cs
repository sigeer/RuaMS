using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;

namespace Application.Plugin.Script.Features.PapulatusBattle
{
    internal sealed class Event : AbstractPartyQuestEventTemplate
    {
        public Event() : base(PapulatusBattleFeature.EventName)
        {
            MinCount = 1;
            MaxCount = 6;
            MinLevel = 1;
            MaxLevel = 255;

            RecruitMap = 220080000;
            EntryMap = 220080001;
            ExitMap = 220080000;
            ClearMap = 220080000;

            MinMap = 220080001;
            MaxMap = 220080001;

            EventTime = 45 * 60;
        }


        /// <summary>
        /// 时空裂缝闸门：220080000 上的反应堆 2208001~2208003。开战时打开，通关/超时时关闭。
        /// </summary>
        const int GateMapId = 220080000;
        static readonly int[] GateReactorIds = [2208001, 2208002, 2208003];

        async Task UpdateGateState(AbstractEventInstanceManager eim, sbyte state)
        {
            var map = await eim.EventManager.ChannelServer.getMapFactory().getMap(GateMapId);
            foreach (var id in GateReactorIds)
            {
                var reactor = map.getReactorById(id);
                if (reactor != null)
                    await reactor.forceHitReactor(state);
            }
        }

        public override async Task AfterSeup(AbstractEventInstanceManager eim)
        {
            await base.AfterSeup(eim);
            await UpdateGateState(eim, 1);
        }

        public override async Task OnLobbyDisposed(AbstractEventInstanceManager eim)
        {
            await UpdateGateState(eim, 0);

            await base.OnLobbyDisposed(eim);
        }

        public override async Task OnMobKilled(AbstractEventInstanceManager eim, Monster mob, ICombatantObject? killer)
        {
            if (mob.getId() != 8500002)
                return;

            await eim.showClearEffect(mob.getMap().getId());
            await eim.clearPQ();
        }


    }
}
