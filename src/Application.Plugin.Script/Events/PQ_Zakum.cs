using Application.Core.Channel;
using Application.Core.scripting.Events.Templates;
using Application.Core.Scripting.Events;

namespace Application.Plugin.Script.Events
{

    internal class PQ_Zakum : AbstractPartyQuestEventTemplate
    {
        public PQ_Zakum() : base(nameof(PQ_Zakum))
        {
            MinCount = 3;
            MaxCount = 6;

            MinLevel = 50;
            MaxLevel = 255;

            EntryMap = 280010000;
            ExitMap = 211042300;
            RecruitMap = 211042300;
            ClearMap = 211042300;

            MinMap = 280010000;
            MaxMap = 280011006;

            EventTime = 30 * 60;
        }
    }
}
