using Application.Core.Channel;
using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.scripting.Events.Instances;
using Application.Core.Scripting.Events;
using Application.Shared.Constants.Mob;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Plugin.Script.Events
{
    internal class PQ_Ellin : PartyQuestEventManager
    {
        public PQ_Ellin(WorldChannel cserv) : base(cserv, nameof(PQ_Ellin))
        {
            MinCount = 4;
            MaxCount = 6;
            MinLevel = 44;
            MaxLevel = 55;
            EntryMap = 930000000;
            ExitMap = 930000800;
            RecruitMap = 300030100;
            ClearMap = 930000800;
            MinMap = 930000000;
            MaxMap = 930000800;

            EventTime = 30 * 60;
        }

        int bossId = 9300182;

        public override void OnMobKilled(AbstractEventInstanceManager eim, Monster mob, ICombatantObject? killer)
        {
            if (mob.getId() == bossId)
            {
                eim.showClearEffect(mob.getMap().getId());
                eim.clearPQ();
            }
        }

        public override void OnMobClear(AbstractEventInstanceManager eim, IMap map)
        {
            if (map.Id == 930000100)
            {
                eim.showClearEffect(map.getId());
            }
        }
    }
}
