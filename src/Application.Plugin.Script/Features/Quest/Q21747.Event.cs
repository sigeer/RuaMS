using Application.Core.scripting.Events.Instances;
using Application.Plugin.Script.Events;
using System.Drawing;

namespace Application.Plugin.Script.Features.Quest
{
    internal sealed class Q21747Event : SoloQuestEventTemplate
    {
        public Q21747Event() : base(Q21747Feature.QuestIdValue)
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
