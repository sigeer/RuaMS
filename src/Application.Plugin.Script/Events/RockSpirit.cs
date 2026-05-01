using Application.Core.Channel;
using Application.Core.Scripting.Events;

namespace Application.Plugin.Script.Events
{
    internal class RockSpirit : SoloEventManager
    {
        public RockSpirit(WorldChannel cserv) : base(cserv, nameof(RockSpirit))
        {
            EventTime = 60 * 60;
            EntryMap = 103040410;
            ExitMap = 103040400;
            MinMap = 103040410;
            MaxMap = 103040410;
        }
    }
}
