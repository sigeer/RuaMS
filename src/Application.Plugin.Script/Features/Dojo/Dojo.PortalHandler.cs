using Application.Core.Client;
using Application.Core.Game.Maps;
using Application.Core.Server.maps;
using Application.Shared.Constants.Map;
using scripting.portal;

namespace Application.Plugin.Script.Features.Dojo
{
    /// <summary>
    /// 武陵道场：dojang_exit、dojang_next、dojang_tuto、dojang_up。
    /// </summary>
    internal sealed class DojoPortalHandler : PortalPlayerInteraction
    {
        public DojoPortalHandler(IChannelClient c, IPortal portal) : base(c, portal)
        {
        }

        // Map: 925020000 | pt: 7
        public async Task<bool> dojang_exit()
        {
            var map = getPlayer().GetSavedLocation(Shared.MapObjects.SavedLocationType.MIRROR);
            if (map == -1)
            {
                map = 100000000;
            }

            await playPortalSound();
            await warp(map);
            return true;
        }
        // Map: 925020100, 925020200, 925020300, 925020400, 925020500, 925020600 … (共 39 张) | pt: 8
        public async Task<bool> dojang_next()
        {
            if (!getPlayer().canClickNPC(2000))
            {
                return false;
            }
            getPlayer().setClickedNPC();

            var mapId = getPlayer().getMapId();
            var eim = GetEventInstanceTrust();

            if (!eim.ClearedMaps.ContainsKey(mapId))
            {
                await Pink("门还没有打开。");
                return false;
            }

            var stage = DojoFeature.GetStage(mapId);

            if (stage >= DojoFeature.MaxStage)
            {
                // 通关：结算经验
                await playPortalSound();
                await warp(DojoFeature.ClearMap, 0);
                await getPlayer().gainExp(2000 * getPlayer().getDojoPoints(), true, true, true);
                // 通关了，清空
                getPlayer().setDojoStage(0);
                return true;
            }

            var nextMap = mapId + 100;
            // 进入休息关前，把当前层及此前各层的训练点数一次性补发给地图上的所有人
            if (DojoFeature.IsRestingSpot(nextMap) && eim.EventName == DojoFeature.PartyEventName)
            {
                await FinishSection(mapId);
                return true;
            }

            await Pink("你获得了 " + getPlayer().addDojoPointsByMap(mapId) + " 修炼点数。当前总修炼点数为：" + getPlayer().getDojoPoints() + "。");
            await playPortalSound();
            await warp(nextMap, 0);
            return true;
        }

        /// <summary>
        /// 组队道馆的休息关前结算：把全队送上休息层，并补发这一节（最多 5 层）的训练点数。
        ///
        /// 点数按「从自己所在的层一直到当前层」累计：掉队在低层的队员也能补到
        /// 他从那一层起的份，已经在本层的人只算当前这一层。
        /// </summary>
        private async Task FinishSection(int mapId)
        {
            var eim = GetEventInstanceTrust();
            var restMapId = mapId + 100;

            foreach (var chr in eim.getPlayers())
            {
                // 本节的地图层从 mapId 往下数 5 层，越界的层不参与结算
                var chrMapId = chr.getMapId();
                var from = Math.Max(0, (mapId - chrMapId) / 100);

                for (var j = from; j >= 0; j--)
                {
                    await chr.Pink("你获得了 " + chr.addDojoPointsByMap(mapId - 100 * j) + " 修炼点数。当前总修炼点数为： " + chr.getDojoPoints() + "。");
                }

                await chr.changeMap(restMapId, 0);
            }
        }
        // Map: 925020010 | pt: 7
        public async Task<bool> dojang_tuto()
        {
            var eim = GetEventInstanceTrust();
            if (!eim.isEventCleared())
            {
                await Pink("萧公: 哈哈！你要像懦夫一样逃跑？我可不会让你轻易逃脱！");
                return false;
            }

            await playPortalSound();
            await warp(DojoFeature.RecruitMap, 0);
            return true;
        }
        /// <summary>
        /// 上行传送门：本层 BOSS 被击破后开门（把门 reactor 推到开启状态），本身不传送。
        /// </summary>
        // Map: 925020100, 925020200, 925020300, 925020400, 925020500, 925020610 … (共 33 张) | pt: 7
        public async Task<bool> dojang_up()
        {
            var eim = GetEventInstanceTrust();
            if (!eim.ClearedMaps.ContainsKey(getMapId()))
            {
                await Pink("这里还有怪物没有清理。");
                return false;
            }

            await goDojoUp();
            await getPlayer().getMap().setReactorState();

            // 勋章挑战：单人模式下，击破对应层数的 BOSS 后计数
            var mapId = getPlayer().getMapId();
            var stage = DojoFeature.GetStage(mapId);
            if (DojoFeature.GetMobStage(stage) == getPlayer().getVanquisherStage() && !MapId.isPartyDojo(mapId))
            {
                getPlayer().setVanquisherKills(getPlayer().getVanquisherKills() + 1);
            }

            return true;
        }
    }
}
