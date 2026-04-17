using Application.Core.Channel;
using Application.Core.Scripting.Events;

namespace Application.Plugin.Script.Events
{
    internal class DollHouse : SoloEventManager
    {
        public DollHouse(WorldChannel cserv) : base(cserv, nameof(DollHouse))
        {
            EntryMap = 922000010;
            ExitMap = 221024400;

            EventTime = 600;
        }
    }
}
