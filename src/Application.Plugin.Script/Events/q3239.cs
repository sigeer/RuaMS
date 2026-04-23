using Application.Core.Channel;
using Application.Core.Scripting.Events;

namespace Application.Plugin.Script.Events
{
    internal class q3239 : SoloEventManager
    {
        public q3239(WorldChannel cserv) : base(cserv, nameof(q3239))
        {
            EventTime = 20 * 60;
            EntryMap = 922000000;
            ExitMap = 922000009;

            MinMap = 922000000;
            MaxMap = 922000000;
        }

    }
}
