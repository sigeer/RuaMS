using Application.Core.scripting.Events.Templates;

namespace Application.Plugin.Script.Features.DavyJohn
{
    internal sealed class Event : AbstractPartyQuestEventTemplate
    {
        public Event() : base(DavyJohnFeature.EventName)
        {
            QuestId = 1204;

            MinCount = 3;
            MaxCount = 6;
            MinLevel = 55;
            MaxLevel = 100;

            EntryMap = DavyJohnFeature.Stage1Map;
            ExitMap = DavyJohnFeature.ExitMapId;
            RecruitMap = DavyJohnFeature.RecruitMapId;
            ClearMap = DavyJohnFeature.ClearMapId;
            MinMap = DavyJohnFeature.Stage1Map;
            MaxMap = DavyJohnFeature.Stage1Map + 500;

            EventTime = 4 * 60;
            Type = Shared.Events.EventInstanceType.PartyQuest;
        }
    }
}
