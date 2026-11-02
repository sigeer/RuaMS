using Application.Core.Client;
using Application.Core.scripting.Events.Instances;
using Application.Core.Server.maps;
using scripting.portal;

namespace Application.Plugin.Script.Features.MagatiaPQ
{
    internal sealed class PortalHandler : PortalPlayerInteraction
    {
        public PortalHandler(IChannelClient c, IPortal portal) : base(c, portal)
        {
        }

        /// <summary>
        /// 当前副本所属分支的地图号基数；事件不在场或不属于本任务时返回 null。
        /// </summary>
        int? MapBase => getEventInstance()?.EventName switch
        {
            MagatiaPQFeature.AlcadnoEventName => MagatiaPQFeature.AlcadnoMapBase,
            MagatiaPQFeature.ZenumistEventName => MagatiaPQFeature.ZenumistMapBase,
            _ => null
        };

        async Task<bool> PassIf(Func<AbstractEventInstanceManager, bool> condition, Func<int, int> mapSelector)
        {
            var eim = getEventInstance();
            var baseMap = MapBase;
            if (eim == null || baseMap == null || !condition(eim))
            {
                await Pink("The portal is not opened yet.");
                return false;
            }

            await playPortalSound();
            await warp(mapSelector(baseMap.Value), 0);
            return true;
        }

        async Task<bool> PassIfReactor(string reactorName, Func<int, int> mapSelector, int portalId = 0)
        {
            if (getMap().getReactorByName(reactorName)?.getState() != 1)
            {
                await Pink("The door is not opened yet.");
                return false;
            }

            var baseMap = MapBase;
            if (baseMap == null)
            {
                return false;
            }

            await playPortalSound();
            await warp(mapSelector(baseMap.Value), portalId);
            return true;
        }

        /// <summary>
        /// 4 号房间用位掩码记录已进入的小房间，重复进入会被拒绝。
        /// </summary>
        async Task<bool> EnterRoom(int slot)
        {
            var eim = getEventInstance();
            var baseMap = MapBase;
            if (eim == null || baseMap == null)
            {
                return false;
            }

            var area = eim.getIntProperty("statusStg5");
            if ((area >> slot) % 2 != 0)
            {
                await Pink("This room is already being explored.");
                return false;
            }

            eim.setIntProperty("statusStg5", area | (1 << slot));
            await playPortalSound();
            await warp(baseMap.Value + 301 + slot, 0);
            return true;
        }

        /// <summary>
        /// 爬塔：按 10 层随机组合判断当前柱位是否正确，正确上移一层，错误退回。
        /// </summary>
        async Task<bool> ClimbCombination()
        {
            var eim = getEventInstance();
            if (eim == null || MapBase == null)
            {
                return false;
            }

            var mapLayer = "stage6_comb" + (getMapId() % 10);
            var comb = eim.getProperty(mapLayer);
            if (string.IsNullOrEmpty(comb))
            {
                var builder = new System.Text.StringBuilder(10);
                for (var i = 0; i < 10; i++)
                {
                    builder.Append(Random.Shared.Next(4));
                }

                comb = builder.ToString();
                eim.setProperty(mapLayer, comb);
            }

            var portalId = int.Parse(getPortal().getName().Substring(2, 4));
            var row = (int)Math.Floor(portalId / 10.0);
            var col = portalId % 10;

            if (col == int.Parse(comb.Substring(row, 1)))
            {
                if (row < 9)
                {
                    await playPortalSound();
                    await TeleportPortal(getPortal().getId() + 4);
                }
                else
                {
                    if (eim.getIntProperty("statusStg6") == 0)
                    {
                        eim.setIntProperty("statusStg6", 1);
                        await eim.GiveStageClearRewardAll(6);
                    }

                    await playPortalSound();
                    await TeleportPortal(1);
                }
            }
            else
            {
                await playPortalSound();
                await TeleportPortal(2);
            }

            return true;
        }

        /// <summary>
        /// 训练塔入口：按当前分支基数 +401。
        /// </summary>
        async Task<bool> EnterTraining()
        {
            var baseMap = MapBase;
            if (baseMap == null)
            {
                return false;
            }

            await playPortalSound();
            await warp(baseMap.Value + 401, 0);
            return true;
        }

        /// <summary>出口传送：Alcadno 回 261000021，Zenumist 回 261000011。</summary>
        async Task<bool> Exit(int targetMap)
        {
            await playPortalSound();
            await warp(targetMap, 0);
            return true;
        }

        #region Alcadno（jnr 前缀）

        // Map: 926110001 | pt: 7
        public Task<bool> jnr1_out() => PassIf(eim => eim.getIntProperty("statusStg2") == 1, b => b + 100);

        // Map: 926110000 | pt: 8
        public Task<bool> jnr1_pt00() => PassIf(eim => eim.getIntProperty("statusStg1") == 1, b => b + 1);

        // Map: 926110100 | pt: 8
        public Task<bool> jnr2_out() => PassIf(eim => eim.getIntProperty("statusStg3") == 3, b => b + 200);

        // Map: 926110200 | pt: 8
        public Task<bool> jnr3_in0() => PassIfReactor("jnr3_out1", b => b + 201);

        // Map: 926110200 | pt: 8
        public Task<bool> jnr3_in1() => PassIfReactor("jnr3_out2", b => b + 202);

        // Map: 926110200 | pt: 8
        public Task<bool> jnr3_out() => PassIfReactor("jnr3_out3", b => b + 203);

        // Map: 926110300 | pt: 8
        public Task<bool> jnr4_r1() => EnterRoom(0);

        // Map: 926110300 | pt: 8
        public Task<bool> jnr4_r2() => EnterRoom(1);

        // Map: 926110300 | pt: 8
        public Task<bool> jnr4_r3() => EnterRoom(2);

        // Map: 926110300 | pt: 8
        public Task<bool> jnr4_r4() => EnterRoom(3);

        // Map: 926110301, 926110302, 926110303, 926110304 | pt: 8
        public Task<bool> jnr5_rp() => ClimbCombination();

        // Map: 926110203 | pt: 8
        public Task<bool> jnr6_out() => PassIfReactor("jnr6_out", b => b + 300);

        // Map: 926110201 | pt: 8
        public Task<bool> jnr_201_0() => PassIfReactor("jnr31_out", b => b + 200, 1);

        // Map: 926110202 | pt: 8
        public Task<bool> jnr_202() => PassIfReactor("jnr32_out", b => b + 200, 2);

        // Map: 926110700 | pt: 7
        public Task<bool> jnr_exit() => Exit(261000021);

        // Map: 926110400 | pt: 8
        public Task<bool> jnr12_in() => EnterTraining();

        #endregion

        #region Zenumist（rnj 前缀）

        // Map: 926100001 | pt: 7
        public Task<bool> rnj1_out() => PassIf(eim => eim.getIntProperty("statusStg2") == 1, b => b + 100);

        // Map: 926100000 | pt: 8
        public Task<bool> rnj1_pt00() => PassIf(eim => eim.getIntProperty("statusStg1") == 1, b => b + 1);

        // Map: 926100100 | pt: 8
        public Task<bool> rnj2_out() => PassIf(eim => eim.getIntProperty("statusStg3") == 3, b => b + 200);

        // Map: 926100200 | pt: 8
        public Task<bool> rnj3_in0() => PassIfReactor("rnj3_out1", b => b + 201);

        // Map: 926100200 | pt: 8
        public Task<bool> rnj3_in1() => PassIfReactor("rnj3_out2", b => b + 202);

        // Map: 926100200 | pt: 8
        public Task<bool> rnj3_out() => PassIfReactor("rnj3_out3", b => b + 203);

        // Map: 926100300 | pt: 8
        public Task<bool> rnj4_r1() => EnterRoom(0);

        // Map: 926100300 | pt: 8
        public Task<bool> rnj4_r2() => EnterRoom(1);

        // Map: 926100300 | pt: 8
        public Task<bool> rnj4_r3() => EnterRoom(2);

        // Map: 926100300 | pt: 8
        public Task<bool> rnj4_r4() => EnterRoom(3);

        // Map: 926100301, 926100302, 926100303, 926100304 | pt: 8
        public Task<bool> rnj5_rp() => ClimbCombination();

        // Map: 926100203 | pt: 8
        public Task<bool> rnj6_out() => PassIfReactor("rnj6_out", b => b + 300);

        // Map: 926100201 | pt: 8
        public Task<bool> rnj_201_0() => PassIfReactor("rnj31_out", b => b + 200, 1);

        // Map: 926100202 | pt: 8
        public Task<bool> rnj_202() => PassIfReactor("rnj32_out", b => b + 200, 2);

        // Map: 926100700 | pt: 7
        public Task<bool> rnj_exit() => Exit(261000011);

        // Map: 926100400 | pt: 8
        public Task<bool> rnj12_in() => EnterTraining();

        #endregion
    }
}
