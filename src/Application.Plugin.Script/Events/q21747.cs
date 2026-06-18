using Application.Core.Channel;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;
using Application.Core.Scripting.Events;
using Application.Shared.Constants.Npc;
using System.Drawing;

namespace Application.Plugin.Script.Events
{
    internal class q21747 : AbstractSoloEventTemplate
    {
        public q21747() : base(nameof(q21747))
        {
            EventTime = 10 * 60;
            EntryMap = 925040100;
            ExitMap = 250020300;
            MinMap = 925040100;
            MaxMap = 925040100;
        }

        public override async Task OnSetup(AbstractEventInstanceManager eim, int level, int lobbyId)
        {
            await base.OnSetup(eim, level, lobbyId);
            await (await eim.getMapInstance(925040100)).SpawnNpc(1204020, new Point(850, 0));
        }
    }
}
