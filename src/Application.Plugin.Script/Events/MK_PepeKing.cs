using Application.Core.scripting.Events.Templates;

namespace Application.Plugin.Script.Events
{
    public class MK_PepeKing : AbstractPartyQuestEventTemplate
    {
        public MK_PepeKing() : base(nameof(MK_PepeKing))
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
