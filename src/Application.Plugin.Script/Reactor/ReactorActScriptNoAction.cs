using Application.Core.Channel.DataProviders;

namespace Application.Plugin.Script
{
    /// <summary>
    /// Reactor.wz 中**没有 <c>action</c> 字段**、但 scripts/reactor 下仍有 JS 实现的反应堆（act 阶段）。
    ///
    /// 没有action理论上应该不走这里，这里的代码逐步移除
    /// </summary>
    internal partial class ReactorActScript
    {
        // Reactor: 2008006 — 奥比斯 PQ 第三阶段：让张伯伦·伊克刷新宝箱
        public async Task r2008006()
        {
            getEventInstance()?.setProperty("statusStg3", "0");
        }

        // Reactor: 2008007 — 奥比斯 PQ 监狱机关（JS 的 act() 为空，逻辑在 hit()）
        public Task r2008007() => Task.CompletedTask;

        // Reactor: 2408002 — 冰原雪域钥匙反应堆：钥匙被传送到别处，由另一张地图刷新
        public async Task r2408002()
        {
            var eim = GetEventInstanceTrust();
            var vvpMap = getMapId();
            var vvpOrig = 4001088;
            var vvpStage = -1;

            await eim.showClearEffect(false, vvpMap);
            await mapMessage(6, "The key has been teleported somewhere...");

            int vvpKey;
            switch (vvpMap)
            {
                case 240050101:
                    vvpKey = vvpOrig;
                    vvpStage = 1;
                    break;
                case 240050102:
                    vvpKey = vvpOrig + 1;
                    vvpStage = 2;
                    break;
                case 240050103:
                    vvpKey = vvpOrig + 2;
                    vvpStage = 3;
                    break;
                case 240050104:
                    vvpKey = vvpOrig + 3;
                    vvpStage = 4;
                    break;
                default:
                    vvpKey = -1;
                    break;
            }

            eim.setIntProperty(vvpStage + "stageclear", 1);

            var womanfred = await eim.getMapInstance(240050100);
            var theWomanfred = womanfred.getReactorByName("keyDrop1");
            var dropper = eim.getPlayers().FirstOrDefault();
            if (dropper == null || theWomanfred == null)
                return;

            var key = ItemInformationProvider.getInstance().GenerateVirtualItemById(vvpKey, 1);
            await womanfred.spawnItemDrop(theWomanfred, dropper, key, theWomanfred.getPosition(), true, true);
            await eim.dropMessage(6, "A bright flash of light, then a key suddenly appears somewhere in the map.");
        }

        // Reactor: 2408004 — 召唤 NPC 2081008
        public async Task r2408004()
        {
            await spawnNpc(2081008);
        }

        // Reactor: 2508000 — 武陵 妖僧：跳到下一层（38 层不跳）
        public async Task r2508000()
        {
            var mapId = getMapId();
            if (mapId / 100 % 100 != 38)
            {
                await warp(mapId + 100);
            }
        }

        // Reactor: 2618000 — 玛加提亚 PQ 烧瓶（JS 的 act() 为空，逻辑在 hit()）
        public Task r2618000() => Task.CompletedTask;

        // Reactor: 2618001 — 玛加提亚 PQ 门：触发另一张地图的出口反应堆
        public async Task r2618001()
        {
            var eim = GetEventInstanceTrust();
            var isAlcadno = eim.getIntProperty("isAlcadno");
            var reactName = isAlcadno == 0 ? "rnj32_out" : "jnr32_out";
            var reactMap = isAlcadno == 0 ? 926100202 : 926110202;

            var map = await eim.getMapInstance(reactMap);
            await map.getReactorByName(reactName)!.hitReactor(getClient());
        }

        // Reactor: 2618002 — 玛加提亚 PQ 门：触发另一张地图的出口反应堆
        public async Task r2618002()
        {
            var eim = GetEventInstanceTrust();
            var isAlcadno = eim.getIntProperty("isAlcadno");
            var reactName = isAlcadno == 0 ? "rnj31_out" : "jnr31_out";
            var reactMap = isAlcadno == 0 ? 926100201 : 926110201;

            var map = await eim.getMapInstance(reactMap);
            await map.getReactorByName(reactName)!.hitReactor(getClient());
        }

        // Reactor: 3009000 — 毒雾森林 水之泉：第 4 状态时显示通关特效
        public async Task r3009000()
        {
            if (getReactor().getState() == 4)
            {
                await GetEventInstanceTrust().showClearEffect(getMapId());
            }
        }

        // Reactor: 8098000 — 海盗 PQ：按地图生成不同的怪物
        public async Task r8098000()
        {
            var map = getMapId();
            var b = Math.Abs(map - 809050005);

            if (map != 809050000 && map != 809050010 && map != 809050014)
            {
                await spawnMonster(9400217 - b, 2);
                await spawnMonster(9400218 - b, 3);
            }
            else
            {
                await spawnMonster(9400209, 6);
                await spawnMonster(9400210, 9);
            }

            await mapMessage(5, "Some monsters are summoned.");
        }

        // Reactor: 9208000 — 公会任务 门卫谜题
        public Task r9208000() => GatekeeperPuzzle();

        // Reactor: 9208001
        public Task r9208001() => GatekeeperPuzzle();

        // Reactor: 9208002
        public Task r9208002() => GatekeeperPuzzle();

        /// <summary>
        /// 9208000~9208002 共用：展示阶段拼接雕像序号，挑战阶段拼接玩家的猜测。
        /// </summary>
        async Task GatekeeperPuzzle()
        {
            var eim = getEventInstance();
            if (eim == null)
                return;

            var status = eim.getProperty("stage1status");
            if (status == null || status == "waiting")
                return;

            var stage = int.Parse(eim.getProperty("stage1phase") ?? "1");
            var oid = getReactor().getObjectId() % 1000;

            if (status == "display")
            {
                if (getReactor().isRecentHitFromAttack())
                    return;

                var prevCombo = eim.getProperty("stage1combo") ?? "";
                prevCombo += oid.ToString("D3");
                eim.setProperty("stage1combo", prevCombo);
                if (prevCombo.Length == 3 * (stage + 3))
                {
                    eim.setProperty("stage1status", "active");
                    await mapMessage(5, "The combo has been displayed; Proceed with caution.");
                    eim.setProperty("stage1guess", "");
                }
            }
            else
            {
                var prevGuess = eim.getProperty("stage1guess") ?? "";
                if (prevGuess.Length != 3 * (stage + 3))
                {
                    prevGuess += oid.ToString("D3");
                    eim.setProperty("stage1guess", prevGuess);
                }
            }
        }

        // Reactor: 9208004 — 公会任务 第二阶段：获得 20 GP
        public async Task r9208004()
        {
            await GainGuildGP(20);
        }

        // Reactor: 9208007 — 公会任务 长矛：推进 speargate，第四次时全员通关特效 + GP
        public async Task r9208007()
        {
            var eim = GetEventInstanceTrust();
            var map = await eim.getMapInstance(990000400);
            var react = map.getReactorByName("speargate");
            if (react == null)
                return;

            await react.forceHitReactor((sbyte)(react.getState() + 1));
            if (react.getState() != 4)
                return;

            int[] maps = [990000400, 990000410, 990000420, 990000430, 990000431, 990000440];
            foreach (var mapId in maps)
            {
                await eim.showClearEffect(false, mapId);
            }

            await GainGuildGP(20);
        }

        // Reactor: 9208009 — 公会任务 BOSS 房：允许复活
        public async Task r9208009()
        {
            var eim = getEventInstance();
            if (eim == null)
                return;

            eim.setProperty("canRevive", "1");
        }

        // Reactor: 9400300 — 假日 PQ：雪人吸收真实的雪之力量
        public async Task r9400300()
        {
            await Features.HolidayPQ.HolidayPQEventBase.FeedSnowman(GetEventInstanceTrust());
        }

        // Reactor: 9400301 — 假日 PQ：雪人吸收假的雪之力量
        public async Task r9400301()
        {
            await Features.HolidayPQ.HolidayPQEventBase.FeedFakeSnowman(GetEventInstanceTrust());
        }
    }
}
