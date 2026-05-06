using Application.Core.Channel;
using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.scripting.Events.Instances;
using Application.Core.Scripting.Events;
using Application.Resources.Messages;
using Application.Shared.Constants.Mob;
using scripting.npc;
using server.maps;

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

            StageClearRewards = new()
            {
                { 910010000, new(1600, 0) },
            };
        }

        protected override void OnSetup(AbstractEventInstanceManager eim, int level, int lobbyId)
        {
            eim.Level = level;
            eim.setProperty("level", level);
            eim.setProperty("stage", "0");
            eim.setIntProperty("bunnyCake", 0);
            eim.setIntProperty("bunnyDamaged", 0);

            eim.getInstanceMap(EntryMap)?.allowSummonState(false);
            eim.getInstanceMap(EntryMap)?.RespawnInterval = 15_000;

            eim.getInstanceMap(910010200)?.RespawnInterval = 15_000;
        }

        protected override void respawnStages(AbstractEventInstanceManager eim)
        {
            var status = eim.ClearedMaps.GetValueOrDefault(EntryMap, StageStatus.NotStarted);
            if (status == StageStatus.NotStarted)
            {
                var map = eim.getInstanceMap(EntryMap)!;

                var flowers = map.GetRequiredMapObjects<Reactor>(Shared.MapObjects.MapObjectType.REACTOR,
                    r => r.getName().StartsWith("moonflower") && r.getState() == 1);

                var rabbitReactor = map.getReactorByName("fullmoon")!;
                rabbitReactor.forceHitReactor((sbyte)flowers.Count);

                eim.Schedule(respawnStages, 5_000);
            }

        }


        public override void OnFriendlyMobDamaged(AbstractEventInstanceManager eim, Monster mob, ICombatantObject? attacker, int damage)
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
