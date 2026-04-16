using Application.Core.Channel;
using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.Scripting.Events;
using Application.Resources.Messages;
using Application.Shared.Constants.Mob;
using scripting.npc;

namespace Application.Plugin.Script.Events
{
    internal class PQ_Henesys : PartyQuestEventManager
    {
        public PQ_Henesys(WorldChannel cserv) : base(cserv, nameof(PQ_Henesys))
        {
            MinCount = 3;
            MaxCount = 6;

            MinLevel = 10;
            MaxLevel = 255;

            EntryMap = 910010000;
            ExitMap = 910010300;
            RecruitMap = 100000200;
            ClearMap = 100000200;
            MinMap = 910010000;
            MaxMap = 910010400;

            EventTime = 10 * 60;
        }

        protected override void OnSetup(AbstractEventInstanceManager eim, int level, int lobbyId)
        {
            eim.setProperty("level", level);
            eim.setProperty("stage", "0");
            eim.setIntProperty("bunnyCake", 0);
            eim.setIntProperty("bunnyDamaged", 0);

            eim.getInstanceMap(910010000)?.resetPQ(level);
            eim.getInstanceMap(910010000)?.allowSummonState(false);

            eim.getInstanceMap(910010200)?.resetPQ(level);
        }

        protected override void respawnStages(AbstractEventInstanceManager eim)
        {
            eim.getMapInstance(910010000).instanceMapRespawn();
            eim.getMapInstance(910010200).instanceMapRespawn();

            eim.Schedule(e => respawnStages(e), 15 * 1000);
        }

        protected override void setEventRewards(AbstractEventInstanceManager eim)
        {
            List<object> expStages = [1600];    //bonus exp given on CLEAR stage signal
            eim.setEventClearStageExp(expStages);
        }


        public override void OnFriendlyMobDamaged(AbstractEventInstanceManager eim, Monster mob, ICombatantObject? attacker)
        {
            if (mob.getId() == MobId.MOON_BUNNY)
            {
                var bunnyDamage = eim.getIntProperty("bunnyDamaged") + 1;
                if (bunnyDamage % 5 == 0)
                {
                    eim.LightBlue(nameof(ClientMessage.Event_HenesysPQ_Message2));
                }
                eim.setIntProperty("bunnyDamaged", bunnyDamage);
            }
        }

        public override void OnFriendlyMobDrop(AbstractEventInstanceManager eim, Monster mob)
        {
            if (mob.getId() == MobId.MOON_BUNNY)
            {
                var cakes = eim.getIntProperty("bunnyCake") + 1;
                eim.setIntProperty("bunnyCake", cakes);
                eim.LightBlue(nameof(ClientMessage.Event_HenesysPQ_Message1), cakes.ToString());
            }
        }

        public override void OnFriendlyMobKilled(AbstractEventInstanceManager eim, Monster mob, ICombatantObject? killer)
        {
            if (mob.getId() == MobId.MOON_BUNNY)
            {
                eim.Pink(nameof(ClientMessage.Event_HenesysPQ_Fail));
                End(eim);
            }

        }

        public async Task HandleCreateInstanceResult(CreateInstanceResult r, NPCConversationManager cm)
        {
            switch (r)
            {
                case CreateInstanceResult.Success:
                    break;
                case CreateInstanceResult.RequiredParty:
                    await cm.SayOK(cm.GetTalkMessage(nameof(ScriptTalk.HenesysPQ_EnterTalk1)));
                    break;
                case CreateInstanceResult.RequiredLeader:
                    await cm.SayOK(cm.GetTalkMessage(nameof(ScriptTalk.PartyQuest_NeedLeaderTalk)));
                    break;
                case CreateInstanceResult.Requirement:
                    await cm.SayOK(cm.GetTalkMessage(nameof(ScriptTalk.PartyQuest_CannotStart_Req)));
                    break;
                case CreateInstanceResult.LobbyLimited:
                    await cm.SayOK(cm.GetTalkMessage(nameof(ScriptTalk.PartyQuest_CannotStart_ChannelFull)));
                    break;
                case CreateInstanceResult.Disposed:
                case CreateInstanceResult.Unknown:
                    await cm.SayOK("未知错误");
                    break;
                default:
                    break;
            }
        }
    }
}
