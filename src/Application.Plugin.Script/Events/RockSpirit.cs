using Application.Core.scripting.Events.Templates;

namespace Application.Plugin.Script.Events
{
    internal class RockSpirit : AbstractSoloEventTemplate
    {
        public RockSpirit() : base(nameof(RockSpirit))
        {
            EventTime = 60 * 60;
            EntryMap = 103040410;
            ExitMap = 103040400;
            MinMap = 103040410;
            MaxMap = 103040410;
        }
    }
}
