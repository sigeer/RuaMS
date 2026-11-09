using Application.Core.Client;
using Application.Core.Game.Maps;
using Application.Core.scripting.Events.Instances;
using Application.Core.Server.life;
using scripting.map;

namespace Application.Plugin.Script.Features.Exped_Balrog
{
    /// <summary>
    /// 地图脚本：Map.wz 的 <c>onUserEnter</c> 与 <c>onFirstUserEnter</c> 共用同一实现，
    /// 两者只是触发时机不同，因此不再区分种类。
    /// </summary>
    internal sealed class Exped_BalrogMapHandler : MapScriptMethods
    {
        public Exped_BalrogMapHandler(IChannelClient c, IMap map) : base(c, map)
        {
        }

        // Map: 105100300, 105100400
        public async Task balog_buff()
        {
            // TODO

        }

        // Map: 105100301, 105100401
        public async Task balog_dateSet()
        {
            // TODO

        }

        // Map: 105100100
        public async Task outCase()
        {
            // TODO

        }

        // Map: 105100300
        public async Task balog_summon()
        {
            var eim = getMap().EventInstanceManager;
            if (eim == null || eim.EventName != Exped_BalrogFeature.EasyEventName)
            {
                return;
            }

            await getMap().spawnFakeMonsterOnGroundBelow(LifeFactory.Instance.GetMonsterTrust(Exped_BalrogFeature.SealedBossId), Exped_BalrogFeature.SpawnPoint);
            await getMap().spawnMonsterOnGroundBelow(LifeFactory.Instance.GetMonsterTrust(Exped_BalrogFeature.LeftClawMobId), Exped_BalrogFeature.SpawnPoint);
            await getMap().spawnMonsterOnGroundBelow(LifeFactory.Instance.GetMonsterTrust(Exped_BalrogFeature.Sealed0RightClawMobId), Exped_BalrogFeature.SpawnPoint);

            eim.Schedule(async eim => await releaseLeftClaw(eim, Exped_BalrogFeature.Sealed0RightClawMobId), 1 * 60000);
        }

        async Task releaseLeftClaw(AbstractEventInstanceManager eim, int toKill)
        {
            await getMap().killMonster(toKill);
        }

        // Map: 105100400
        public async Task easy_balog_summon()
        {
            var eim = getMap().EventInstanceManager;
            if (eim == null || eim.EventName != Exped_BalrogFeature.EasyEventName)
            {
                return;
            }

            /// <summary>
            /// 简单难度：召唤封印形态 8830007（假）、左爪 8830009、右爪 8830013。
            /// 本体 8830010 不在此处刷出，要等封印形态被打破后由 spawnSealedBalrog 召唤。
            /// </summary>
            await getMap().spawnFakeMonsterOnGroundBelow(LifeFactory.Instance.GetMonsterTrust(Exped_BalrogFeature.EasySealedBossId), Exped_BalrogFeature.SpawnPoint);
            await getMap().spawnMonsterOnGroundBelow(LifeFactory.Instance.GetMonsterTrust(Exped_BalrogFeature.EasyLeftClawMobId), Exped_BalrogFeature.SpawnPoint);
            await getMap().spawnMonsterOnGroundBelow(LifeFactory.Instance.GetMonsterTrust(Exped_BalrogFeature.EasySealed0RightClawMobId), Exped_BalrogFeature.SpawnPoint);

            eim.Schedule(async eim => await releaseLeftClaw(eim, Exped_BalrogFeature.EasySealed0RightClawMobId), 1 * 60000);
        }

        // Map: 105100301, 105100401
        public async Task balog_bonusSetting()
        {
            // TODO

        }

    }
}
