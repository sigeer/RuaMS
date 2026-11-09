using Application.Core.scripting.Events.Instances;
using Application.Plugin.Script.Events;

namespace Application.Plugin.Script.Features.Pirate4thJobFeature
{
    internal sealed class Pirate4thJobFeatureEvent6400 : SoloQuestEventTemplate
    {
        internal const int QuestIdValue = 6400;

        internal const int ENTRY_MAP = 912020000;
        internal const int EXIT_MAP = 120000102;

        public Pirate4thJobFeatureEvent6400() : base(QuestIdValue)
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
    }
}
