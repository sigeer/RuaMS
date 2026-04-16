using Application.Core.Channel;
using Application.Core.Scripting.Events;

namespace Application.Plugin.Script.Events
{
    internal class S3rdJob : SoloEventManager
    {
        public S3rdJob(WorldChannel cserv, string name, int entryMap, int exitMap, int minMap, int maxMap) : base(cserv, $"{nameof(S3rdJob)}{name}")
        {
            EventTime = 1200;

            MaxLobbys = 7;
        }
    }
}
