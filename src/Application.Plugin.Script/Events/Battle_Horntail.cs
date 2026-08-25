using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;
using Application.Shared.Battle;
using Application.Shared.Constants.Mob;

namespace Application.Plugin.Script.Events
{
    internal class PQ_Horntail : AbstractPartyQuestEventTemplate
    {
        public PQ_Horntail() : base(nameof(PQ_Horntail))
        {
            MinCount = 6;
            MaxCount = 6;

            MinLevel = 120;
            MaxLevel = 255;

            EntryMap = 240050100;
            ExitMap = 240050000;
            RecruitMap = 240050000;
            ClearMap = 240050400;

            MinMap = 240050100;
            MaxMap = 240050310;

            EventTime = 25 * 60;
        }

        public override async Task OnSetup(AbstractEventInstanceManager eim, int level, int lobbyId)
        {
            (await eim.getInstanceMap(240050101))?.getReactorByName("passKey1")?.setEventState(0);
            (await eim.getInstanceMap(240050102))?.getReactorByName("passKey2")?.setEventState(1);
            (await eim.getInstanceMap(240050103))?.getReactorByName("passKey3")?.setEventState(2);
            (await eim.getInstanceMap(240050104))?.getReactorByName("passKey4")?.setEventState(3);

            await base.OnSetup(eim, level, lobbyId);
        }
    }

    internal class Battle_Horntail : AbstractExpeditionEventTemplate
    {
        public Battle_Horntail() : base(MobId.HORNTAIL, nameof(Battle_Horntail), ExpeditionEntryType.HORNTAIL)
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
