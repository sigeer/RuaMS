using Application.Core.Channel;
using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;
using Application.Core.Scripting.Events;
using System.Drawing;

namespace Application.Plugin.Script.Events
{
    internal class q2245 : AbstractSoloEventTemplate
    {
        public q2245() : base(nameof(q2245))
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
