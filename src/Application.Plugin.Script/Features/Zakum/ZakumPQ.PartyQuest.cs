using Application.Core.scripting.Events.Templates;

namespace Application.Plugin.Script.Features.Zakum
{
    internal sealed class ZakumPartyQuest : AbstractPartyQuestEventTemplate
    {
        public ZakumPartyQuest() : base(ZakumPQ.PartyQuestEventName)
        {
            MinCount = 1;
            MaxCount = 6;

            MinLevel = 50;
            MaxLevel = 255;

            EntryMap = 280010000;
            ExitMap = ZakumPQ.ExitMap;
            RecruitMap = ZakumPQ.ExitMap;
            ClearMap = ZakumPQ.ExitMap;

            MinMap = 280010000;
            MaxMap = 280011006;

            EventTime = 30 * 60;
        }
    }
}
