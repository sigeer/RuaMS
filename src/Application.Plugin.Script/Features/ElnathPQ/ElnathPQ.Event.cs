using Application.Core.scripting.Events.Templates;

namespace Application.Plugin.Script.Features.ElnathPQ
{
    internal sealed class Event : AbstractPartyQuestEventTemplate
    {
        const int ENTRY_MAP = 921100300;
        const int EXIT_MAP = 211040100;

        public Event() : base(ElnathPQFeature.EventName)
        {
            MinCount = 1;
            MaxCount = 4;
            MinLevel = 80;
            MaxLevel = 255;

            RecruitMap = 211000001;
            EntryMap = ENTRY_MAP;
            ExitMap = EXIT_MAP;
            ClearMap = EXIT_MAP;

            MinMap = ENTRY_MAP;
            MaxMap = ENTRY_MAP;

            EventTime = 10 * 60;
            Type = Shared.Events.EventInstanceType.PartyQuest;
        }

    }
}
