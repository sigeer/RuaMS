using Application.Core.Client;
using Application.Core.Server.maps;
using scripting.reactor;

namespace Application.Plugin.Script.Features.GuildQuest
{
    internal sealed class GuildQuestReactorActHandler : ReactorActScriptBase
    {
        public GuildQuestReactorActHandler(IChannelClient c, Reactor reactor) : base(c, reactor)
        {
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
    }
}
