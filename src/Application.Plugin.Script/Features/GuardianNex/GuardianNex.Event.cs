using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;

namespace Application.Plugin.Script.Features.GuardianNex
{
    internal sealed class Event : AbstractSoloEventTemplate
    {
        static readonly int[] GuardianBossIds = [7120100, 7120101, 7120102, 8120100, 8120101, 8140510];

        public Event() : base(GuardianNexFeature.EventName)
        {
            MinCount = 1;
            MaxCount = 1;
            MinLevel = 1;
            MaxLevel = 255;

            EntryMap = 240070000;
            ExitMap = 240070000;
            ClearMap = 240070000;

            MinMap = 240070000;
            MaxMap = 240070000;

            EventTime = 15 * 60;
            Type = Shared.Events.EventInstanceType.Regular;
        }

        public override async Task OnSetup(AbstractEventInstanceManager eim, int level, int lobbyId)
        {
            eim.Level = level;
            eim.setProperty("level", level);
            eim.setIntProperty("nex", 0);
        }

        public override async Task OnMobKilled(AbstractEventInstanceManager eim, Monster mob, ICombatantObject? killer)
        {
            var nex = eim.getIntProperty("nex");
            if (mob.getId() != GuardianBossIds[nex])
                return;

            await eim.showClearEffect(mob.getMap().getId());
            await eim.clearPQ();
        }


    }
}
