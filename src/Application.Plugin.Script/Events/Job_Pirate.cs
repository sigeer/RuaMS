using Application.Core.Channel;
using Application.Core.Scripting.Events;

namespace Application.Plugin.Script.Events
{

    internal class q6330 : SoloEventManager
    {
        public q6330(WorldChannel cserv) : base(cserv, nameof(q6330))
        {
            EventTime = 2 * 60 + 1;

            EntryMap = 912010000;
            ExitMap = 120000101;
            MinMap = 912010000;
            MaxMap = 912010200;
        }

        public override void OnTimeOut(AbstractEventInstanceManager eim)
        {
            var chr = eim.getLeader();
            if (chr?.isAlive() ?? false)
            {
                chr.setQuestProgress(6330, 6331, "2");
                chr.changeMap(912010200);
            }

        }
    }

    internal class q6370 : SoloEventManager
    {
        public q6370(WorldChannel cserv) : base(cserv, nameof(q6370))
        {
            EventTime = 2 * 60 + 1;

            EntryMap = 912010100;
            ExitMap = 120000101;
            MinMap = 912010100;
            MaxMap = 912010200;
        }

        public override void OnTimeOut(AbstractEventInstanceManager eim)
        {
            var chr = eim.getLeader();
            if (chr?.isAlive() ?? false)
            {
                chr.setQuestProgress(6370, 6371, "2");
                chr.changeMap(912010200);
            }
        }
    }
}
