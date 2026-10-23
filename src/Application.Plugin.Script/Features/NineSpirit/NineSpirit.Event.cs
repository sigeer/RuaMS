using Application.Core.Game.Players;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;
using System.Drawing;

namespace Application.Plugin.Script.Features.NineSpirit
{
    internal sealed class Event : AbstractSoloEventTemplate
    {
        const int EVENT_MAP = 240040611;
        const int EXIT_MAP = 240040610;
        const int GUIDE_NPC = 2081008;
        const int NEST_REACTOR = 2406000;
        /// <summary>持有该道具时不再生成向导 NPC。</summary>
        const int NEST_KEY_ITEM = 4001094;

        public Event() : base(NineSpiritFeature.EventName)
        {
            MinCount = 1;
            MaxCount = 1;
            MinLevel = 1;
            MaxLevel = 255;

            EntryMap = EVENT_MAP;
            EntryPortal = 1;

            ExitMap = EXIT_MAP;
            ClearMap = EXIT_MAP;

            MinMap = EVENT_MAP;
            MaxMap = EVENT_MAP;

            EventTime = 5 * 60;
            Type = Shared.Events.EventInstanceType.Regular;
        }

        public override async Task OnPlayerEntry(AbstractEventInstanceManager eim, Player chr)
        {
            var map = await eim.getInstanceMap(EVENT_MAP);
            if (map == null)
                return;

            // 手上没有钥匙时，在反应堆位置生成向导
            if (!chr.haveItem(NEST_KEY_ITEM))
            {
                var reactor = map.getReactorById(NEST_REACTOR);
                var pos = reactor?.getPosition() ?? new Point(0, 0);
                await eim.spawnNpc(GUIDE_NPC, pos, map);
            }

            await chr.changeMap(map, 1);
        }

        public override async Task OnPlayerMapChanged(AbstractEventInstanceManager eim, Player player, int mapid)
        {
            // 离开挑战地图即结束副本
            if (!InInstanceMap(mapid))
            {
                await End(eim, Core.scripting.Events.Abstraction.TerminationReason.MemberCount);
            }

            await base.OnPlayerMapChanged(eim, player, mapid);
        }

    }
}
