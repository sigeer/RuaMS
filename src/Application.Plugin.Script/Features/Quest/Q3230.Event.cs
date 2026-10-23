using Application.Core.Game.Players;
using Application.Core.scripting.Events.Instances;
using Application.Plugin.Script.Events;

namespace Application.Plugin.Script.Features.Quest
{
    internal sealed class Q3230Event : SoloQuestEventTemplate
    {
        const int ENTRY_MAP = 922000010;
        const int EXIT_MAP = 221024400;
        /// <summary>副本内唯一允许持有的道具（玩偶钥匙）。</summary>
        const int DOLL_KEY_ITEM = 4031094;

        public Q3230Event() : base(Q3230.QuestIdValue)
        {
            EventTime = 10 * 60;

            MaxLobbys = 1;

            EntryMap = ENTRY_MAP;
            ExitMap = EXIT_MAP;

            MinMap = ENTRY_MAP;
            MaxMap = ENTRY_MAP;

        }

        public override async Task OnPlayerEntry(AbstractEventInstanceManager eim, Player chr)
        {
            var map = await eim.getInstanceMap(ENTRY_MAP);
            if (map == null)
                return;

            // JS 在 playerEntry 里打乱箱子并设置独占道具
            map.shuffleReactors();
            eim.SetExclusiveItems([DOLL_KEY_ITEM]);

            await chr.changeMap(map, EntryPortal);
            await eim.startEventTimer(EventTime * 1000L);
        }
    }
}
