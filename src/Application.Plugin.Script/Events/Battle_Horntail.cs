using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.Game.Players;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;
using Application.Shared.Constants.Mob;
using Application.Shared.Quest;

namespace Application.Plugin.Script.Events
{
    internal class Battle_Horntail : AbstractExpeditionEventTemplate
    {
        public Battle_Horntail() : base(nameof(Battle_Horntail), MobId.HORNTAIL)
        {
            MinCount = 6;
            MaxCount = 30;

            MinLevel = 100;
            MaxLevel = 255;

            RecruitMap = 240050400;
            EntryMap = 240060000;
            ExitMap = 240050600;
            ClearMap = 240050600;

            MinMap = 240060000;
            MaxMap = 240060200;

            EventTime = 120 * 60;
            RegistrationTime = 5 * 60;
        }

        public override async Task OnMobKilled(AbstractEventInstanceManager eim, Monster mob, ICombatantObject? killer)
        {
            if (mob.getId() == BossId)
            {
                eim.setIntProperty("defeatedBoss", 1);
                await eim.showClearEffect(mob.getMap().getId());
                await eim.clearPQ();

                await eim.dispatchRaiseQuestMobCount(8810018, 240060200);
                await eim.EventManager.ChannelServer.NodeActor.Send(s =>
                {
                    s.SendDropMessage(6,
                        "[Victory] To the crew that have finally conquered Horned Tail after numerous attempts, I salute thee! You are the true heroes of Leafre!!", false);
                });
            }
            else if ((mob.getId() == MobId.HORNTAIL_PREHEAD_LEFT || mob.getId() == MobId.HORNTAIL_PREHEAD_RIGHT))
            {
                var killed = eim.getIntProperty("defeatedHead");
                eim.setIntProperty("defeatedHead", killed + 1);
                await eim.showClearEffect(mob.getMap().getId());
            }
        }
    }
}
