using Application.Core.Client;
using Application.Core.Game.Maps;
using Application.Core.Server.life;
using Application.Utility;
using scripting.map;

namespace Application.Plugin.Script.Features.Dojo
{
    /// <summary>
    /// 地图脚本：Map.wz 的 <c>onUserEnter</c> 与 <c>onFirstUserEnter</c> 共用同一实现，
    /// 两者只是触发时机不同，因此不再区分种类。
    /// </summary>
    internal sealed class DojoMapHandler : MapScriptMethods
    {
        public DojoMapHandler(IChannelClient c, IMap map) : base(c, map)
        {
        }

        // Map: 925020000, 925020010, 925020011, 925020012, 925020013, 925020014
        public async Task dojang_Msg()
        {
            var mapObj = getPlayer().getMap();
            if (mapObj.getId() == 925020000)
            {
                if (mapObj.findClosestPlayerSpawnpoint(getPlayer().getPosition())?.getId() == 0)
                {
                    string[] messages = [
                        "勇闯武陵道场，阁下好胆识！",
                        "想尝尝败北的滋味？尽管放马过来！",
                        "定让你后悔挑战武陵道场！速来受死！"
                    ];
                    await getPlayer().startMapEffect(Randomizer.Select(messages), DojoFeature.MapEffectItem);
                }
            }
            else
            {
                await getPlayer().startMapEffect("哈！让我们看看你有什么！除非你先打败我，否则我不会让你离开的！", DojoFeature.MapEffectItem);
            }
        }

        // Map: 925020002, 925020003, 925040000
        public async Task dojang_QcheckSet()
        {
            // TODO

        }

        // Map: 925020100, 925020101, 925020102, 925020103, 925020104, 925020105 … (共 571 张)
        public async Task dojang_Eff()
        {
            var eim = GetEventInstanceTrust();

            var stage = DojoFeature.GetStage(getMapId());

            // 休息关（stage % 6 == 0）只有入场提示，没有战斗层数动画
            if (stage % 6 > 0)
            {
                var realstage = stage - ((stage / 6) | 0);
                await dojoEnergy();

                await playSound("Dojang/start");
                await mapEffect("dojang/start/stage");
                await mapEffect("dojang/start/number/" + realstage);
            }
        }

        // Map: 925020100, 925020101, 925020102, 925020103, 925020104, 925020105 … (共 571 张)
        public async Task dojang_1st()
        {
            var eim = GetEventInstanceTrust();

            var stage = DojoFeature.GetStage(getMapId());

            if (DojoFeature.IsRestingSpot(getMapId()))
            {
                // 休息关：停止上一关的倒计时，玩家在这里不会超时
                await eim.stopEventTimer();
                return;
            }

            await eim.restartEventTimer(DojoFeature.GetStageTime(stage));
            if (!eim.ClearedMaps.ContainsKey(getMapId()))
            {
                var mobData = LifeFactory.Instance.GetMonsterTrust(DojoFeature.GetStageMobId(stage));
                await getPlayer().getMap().spawnDojoMonster(mobData);
            }
        }

    }
}
