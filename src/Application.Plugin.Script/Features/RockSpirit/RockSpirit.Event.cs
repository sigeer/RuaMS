using Application.Core.scripting.Events.Templates;

namespace Application.Plugin.Script.Features.RockSpirit
{
    internal sealed class Event : AbstractSoloEventTemplate
    {
        public Event() : base(RockSpiritFeature.EventName)
        {
            EventTime = 60 * 60;
            EntryMap = 103040410;
            ExitMap = 103040400;
            MinMap = 103040410;
            MaxMap = 103040410;
        }

    }
}
