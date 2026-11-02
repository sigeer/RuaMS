using Application.Core.scripting.Events.Templates;

namespace Application.Plugin.Script.Features.MushroomCastle
{
    internal sealed class PepeKingEvent : AbstractPartyQuestEventTemplate
    {
        public PepeKingEvent() : base(MushroomCastleFeature.PepeKingEventName)
        {
            EventTime = 20 * 60;
            MinCount = 1;
            MaxCount = 6;
            MinLevel = 30;
            MaxLevel = 255;

            EntryMap = 106021500;
            EntryPortal = 1;
            ExitMap = 106021400;
            ExitPortal = 2;
            MinMap = 106021500;
            MaxMap = 106021500;
            RecruitMap = 106021400;
        }
    }
}
