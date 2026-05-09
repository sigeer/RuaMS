using Application.Core.Channel;
using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.Game.Players;
using Application.Core.scripting.Events.Instances;
using Application.Core.Scripting.Events;
using Application.Shared.Constants.Mob;
using Application.Shared.Quest;
using System.Numerics;

namespace Application.Plugin.Script.Events
{
    internal class Battle_Zakum : ExpeditionEventManager
    {
        public Battle_Zakum(WorldChannel cserv) : base(cserv, nameof(Battle_Zakum), MobId.ZAKUM_3)
        {
            MinCount = 6;
            MaxCount = 30;

            MinLevel = 50;
            MaxLevel = 255;

            RecruitMap = 211042400;
            EntryMap = 280030000;
            ExitMap = 211042400;
            ClearMap = 211042400;

            MinMap = 280030000;
            MaxMap = 280030000;

            EventTime = 120 * 60;
            RegistrationTime = 5 * 60;
        }

        public override void OnMobKilled(AbstractEventInstanceManager eim, Monster mob, ICombatantObject? killer)
        {
            if (mob.getId() == BossId)
            {
                eim.setIntProperty("defeatedBoss", 1);
                eim.showClearEffect(mob.getMap().getId());
                eim.clearPQ();

                ChannelServer.NodeActor.Send(s =>
                {
                    s.SendDropMessage(6,
                        "[Victory] At last, the tree of evil that for so long overwhelmed Ossyria has fallen. To the crew that managed to finally conquer Zakum, after numerous attempts, victory! You are the true heroes of Ossyria!!", false);
                });
            }
        }

        public override void OnPlayerUnregister(AbstractEventInstanceManager eim, Player chr)
        {
            base.OnPlayerUnregister(eim, chr);

            if (eim.isEventCleared())
            {
                completeQuest(chr, QuestId.ZakumBattle, 2030010);
            }
        }
    }
}
