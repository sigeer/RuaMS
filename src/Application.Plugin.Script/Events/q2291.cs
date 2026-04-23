using Application.Core.Channel;
using Application.Core.Scripting.Events;

namespace Application.Plugin.Script.Events
{
    internal class q2291 : SoloEventManager
    {
        public q2291(WorldChannel cserv) : base(cserv, nameof(q2291))
        {
            EventTime = 30 * 60;
            EntryMap = 103040440;
            ExitMap = 103040400;

            MinMap = 103040410;
            MaxMap = 103040460;
        }
    }
}
