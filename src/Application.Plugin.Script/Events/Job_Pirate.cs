using Application.Core.Channel;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;
using Application.Core.Scripting.Events;

namespace Application.Plugin.Script.Events
{

    internal class q6330 : AbstractSoloEventTemplate
    {
        public q6330() : base(nameof(q6330))
        {
            EventTime = 2 * 60 + 1;
            EntryMap = 912010000;
            ExitMap = 120000101;
            MinMap = 912010000;
            MaxMap = 912010200;
        }

        public override async Task OnTimeOut(AbstractEventInstanceManager eim)
        {
            var chr = eim.getLeader();
            if (chr?.isAlive() ?? false)
            {
                await chr.setQuestProgress(6330, 6331, "2");
                await chr.changeMap(912010200);
            }

        }
    }

    internal class q6370 : AbstractSoloEventTemplate
    {
        public q6370() : base(nameof(q6370))
        {
            EventTime = 2 * 60 + 1;
            EntryMap = 912010100;
            ExitMap = 120000101;
            MinMap = 912010100;
            MaxMap = 912010200;
        }

        public override async Task OnTimeOut(AbstractEventInstanceManager eim)
        {
            var chr = eim.getLeader();
            if (chr?.isAlive() ?? false)
            {
                await chr.setQuestProgress(6370, 6371, "2");
                await chr.changeMap(912010200);
            }
        }
    }
}
