using Application.Core.Channel;
using Application.Core.Scripting.Events;

namespace Application.Plugin.Script.Events
{
    internal class q21739 : SoloEventManager
    {
        public q21739(WorldChannel cserv) : base(cserv, nameof(q21739))
        {
            EventTime = 10 * 60;
            EntryMap = 920030000;
            EntryPortal = 2;
            ExitMap = 200060000;
            MinMap = 920030000;
            MaxMap = 920030001;
        }
    }
}
