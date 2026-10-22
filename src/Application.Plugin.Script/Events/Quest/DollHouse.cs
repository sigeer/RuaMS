using Application.Core.Game.Players;
using Application.Core.scripting.Events.Instances;

namespace Application.Plugin.Script.Events
{
    /// <summary>
    /// 玩偶之家单人挑战。
    /// 对应 JS scripts/event/DollHouse.js。
    /// 没有击杀通关判定：限时结束即出场；进入时打乱反应堆并开始计时。
    /// </summary>
    internal class DollHouse : SoloQuestEventTemplate
    {
        const int ENTRY_MAP = 922000010;
        const int EXIT_MAP = 221024400;
        /// <summary>副本内唯一允许持有的道具（玩偶钥匙）。</summary>
        const int DOLL_KEY_ITEM = 4031094;

        public DollHouse() : base(3230, 10 * 60, ENTRY_MAP, EXIT_MAP, ENTRY_MAP, ENTRY_MAP)
        {

        }

        public override async Task OnSetup(AbstractEventInstanceManager eim, int level, int lobbyId)
        {
            eim.Level = level;
            eim.setProperty("level", level);
            eim.setIntProperty("boss", 0);
        }

        public override async Task OnPlayerEntry(AbstractEventInstanceManager eim, Player chr)
        {
            var map = await eim.getInstanceMap(ENTRY_MAP);
            if (map == null)
                return;

            // JS 在 playerEntry 里打乱箱子并设置独占道具
            map.shuffleReactors();
            eim.SetExclusiveItems([DOLL_KEY_ITEM]);

            await chr.changeMap(map, 0);
            await eim.startEventTimer(EventTime * 1000L);
        }
    }
}
