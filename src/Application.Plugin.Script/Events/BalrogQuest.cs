using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;

namespace Application.Plugin.Script.Events
{
    /// <summary>
    /// 巴洛古单人剧情任务。
    /// 对应 JS scripts/event/BalrogQuest.js。
    /// </summary>
    internal class BalrogQuest : AbstractSoloEventTemplate
    {
        const int BalrogMobId = 9300326;

        public BalrogQuest() : base(nameof(BalrogQuest))
        {
            MinCount = 1;
            MaxCount = 1;
            MinLevel = 1;
            MaxLevel = 255;

            EntryMap = 910520000;
            ExitMap = 105100100;
            ClearMap = 105100100;

            MinMap = 910520000;
            MaxMap = 910520000;

            EventTime = 10 * 60;
            Type = Shared.Events.EventInstanceType.Regular;
        }

        public override async Task OnMobKilled(AbstractEventInstanceManager eim, Monster mob, ICombatantObject? killer)
        {
            if (mob.getId() != BalrogMobId)
                return;

            await mob.MapModel.SpawnNpc(1061015, new System.Drawing.Point(0, 115));
        }
    }
}
