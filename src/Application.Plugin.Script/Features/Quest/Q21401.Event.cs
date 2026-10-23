using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.scripting.Events.Instances;
using Application.Plugin.Script.Events;

namespace Application.Plugin.Script.Features.Quest
{
    internal sealed class Q21401Event : SoloQuestEventTemplate
    {
        public Q21401Event() : base(Q21401Feature.QuestIdValue)
        {
            MaxLobbys = 7;
            EventTime = 10 * 60;
            EntryMap = 914020000;
            ExitMap = 140000000;
            MinMap = 914020000;
            MaxMap = 914020000;
        }

        public override async Task OnMobKilled(AbstractEventInstanceManager eim, Monster mob, ICombatantObject? killer)
        {
            if (mob.getId() == 9001014)
            {
                await eim.clearPQ();
            }
        }

    }
}
