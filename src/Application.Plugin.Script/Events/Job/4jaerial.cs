using Application.Core.Game.Players;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;

namespace Application.Plugin.Script.Events
{
    /// <summary>
    /// 四转飞行技能教学（4jaerial）。
    /// 对应 JS scripts/event/4jaerial.js。
    /// 纯教学副本：重置地图、打乱反应堆，限时结束即出场。
    /// </summary>
    internal class Job4Aerial : AbstractSoloEventTemplate
    {
        const int ENTRY_MAP = 912020000;
        const int EXIT_MAP = 120000102;

        public Job4Aerial() : base(nameof(Job4Aerial))
        {
            MinCount = 1;
            MaxCount = 1;
            MinLevel = 1;
            MaxLevel = 255;

            EntryMap = ENTRY_MAP;
            ExitMap = EXIT_MAP;
            ClearMap = EXIT_MAP;

            MinMap = ENTRY_MAP;
            MaxMap = ENTRY_MAP;

            EventTime = 2 * 60;
            MaxLobbys = 7;
            Type = Shared.Events.EventInstanceType.Regular;
        }

        public override async Task OnSetup(AbstractEventInstanceManager eim, int level, int lobbyId)
        {
            eim.Level = level;
            eim.setProperty("level", level);
            eim.setIntProperty("boss", 0);
            eim.setIntProperty("canLeave", 0);

            var map = await eim.getInstanceMap(ENTRY_MAP);
            if (map != null)
            {
                map.shuffleReactors();
            }
        }

        public override async Task OnPlayerEntry(AbstractEventInstanceManager eim, Player chr)
        {
            var map = await eim.getInstanceMap(ENTRY_MAP);
            if (map == null)
                return;

            await chr.changeMap(map, 0);
        }
    }
}
