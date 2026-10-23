using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.scripting.Events.Instances;
using Application.Plugin.Script.Events;
using System.Drawing;

namespace Application.Plugin.Script.Features.Quest
{
    internal sealed class Q2245Event : SoloQuestEventTemplate
    {
        public Q2245Event() : base(Q2245Feature.QuestIdValue)
        {
            MaxLobbys = 7;
            EventTime = 10 * 60;
            EntryMap = 910520000;
            ExitMap = 105100100;
            MinMap = 910520000;
            MaxMap = 910520000;
        }

        public override async Task OnMobKilled(AbstractEventInstanceManager eim, Monster mob, ICombatantObject? killer)
        {
            if (mob.getId() == 9300326)
            {
                await eim.spawnNpc(1061015, new Point(0, 115), mob.getMap());
            }
        }

    }
}
