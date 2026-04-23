using Application.Core.Channel;
using Application.Core.Scripting.Events;

namespace Application.Plugin.Script.Events
{
    internal class q21733 : SoloEventManager
    {
        public q21733(WorldChannel cserv) : base(cserv, nameof(q21733))
        {
            EventTime = 10 * 60;
            EntryMap = 910400000;
            ExitMap = 104000004;
            MinMap = 910400000;
            MaxMap = 910400000;
        }
    }
}
