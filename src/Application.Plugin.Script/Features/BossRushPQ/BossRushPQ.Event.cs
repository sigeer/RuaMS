using Application.Core.Game.Players;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;

namespace Application.Plugin.Script.Features.BossRushPQ
{
    internal sealed class Event : AbstractPartyQuestEventTemplate
    {
        const int ENTRY_MAP = 970030100;

        public Event() : base(BossRushPQFeature.EventName)
        {
            MinCount = 1;
            MaxCount = 6;
            MinLevel = 1;
            MaxLevel = 255;

            RecruitMap = 970030000;
            EntryMap = ENTRY_MAP;
            ExitMap = 970030000;
            ClearMap = 970030000;

            MinMap = 970030001;
            MaxMap = 970042711;

            EventTime = 5 * 60;
            Type = Shared.Events.EventInstanceType.PartyQuest;
        }

        public override async Task OnSetup(AbstractEventInstanceManager eim, int level, int lobbyId)
        {
            eim.Level = level;
            eim.setProperty("level", level);
            eim.setIntProperty("lobby", lobbyId);
        }

        public override async Task OnPlayerEntry(AbstractEventInstanceManager eim, Player chr)
        {
            // JS: eim.getMapInstance(entryMap + eim.getIntProperty("lobby"))
            var map = await eim.getInstanceMap(ENTRY_MAP + eim.getIntProperty("lobby"));
            if (map == null)
                return;

            await chr.changeMap(map, 0);
        }

    }
}
