using Application.Core.Channel;
using Application.Core.Scripting.Events;

namespace Application.Plugin.Script.Events
{
    internal class PQ_Pirate : PartyQuestEventManager
    {
        public PQ_Pirate(WorldChannel cserv) : base(cserv, nameof(PQ_Pirate))
        {
            MinCount = 3;
            MaxCount = 6;
            MinLevel = 55;
            MaxLevel = 100;

            EntryMap = 925100000;
            ExitMap = 925100700;
            RecruitMap = 251010404;
            ClearMap = 925100600;
            MinMap = 925100000;
            MaxMap = 925100500;

            EventTime = 4 * 60;
        }
    }
}
