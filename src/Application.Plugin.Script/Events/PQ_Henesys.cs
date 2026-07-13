using Application.Core.Channel;
using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;
using Application.Core.Scripting.Events;
using Application.Resources.Messages;
using Application.Shared.Constants.Mob;
using scripting.npc;
using server.maps;

namespace Application.Plugin.Script.Events
{
    internal class PQ_Henesys : AbstractPartyQuestEventTemplate
    {
        public PQ_Henesys() : base(nameof(PQ_Henesys))
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
            Type = Shared.Events.EventInstanceType.PartyQuest;

            StageClearRewards = new()
            {
                { 910010000, new(1600, 0) },
            };
        }

        public override async Task OnSetup(AbstractEventInstanceManager eim, int level, int lobbyId)
        {
            eim.Level = level;
            eim.setProperty("level", level);
            eim.setProperty("stage", "0");
            eim.setIntProperty("bunnyCake", 0);
            eim.setIntProperty("bunnyDamaged", 0);

            var eventMap = await eim.getInstanceMap(EntryMap);
            if (eventMap != null)
            {
                await eventMap.clearMapObjects();
                eventMap.allowSummonState(false);
                eventMap.RespawnInterval = 15_000;
            }

            var eventMap2 = await eim.getInstanceMap(910010200);
            if (eventMap2 != null)
                eventMap2.RespawnInterval = 15_000;
        }

        public override async Task respawnStages(AbstractEventInstanceManager eim)
        {
            var status = eim.ClearedMaps.GetValueOrDefault(EntryMap, StageStatus.NotStarted);
            if (status == StageStatus.NotStarted)
            {
                var map = await eim.getInstanceMap(EntryMap)!;

                var flowers = map.GetRequiredMapObjects<Reactor>(Shared.MapObjects.MapObjectType.REACTOR,
                    r => r.getName().StartsWith("moonflower") && r.getState() == 1);

                var rabbitReactor = map.getReactorByName("fullmoon")!;
                await rabbitReactor.forceHitReactor((sbyte)flowers.Count);

                eim.Schedule(respawnStages, 5_000);
            }

        }


        public override async Task OnFriendlyMobDamaged(AbstractEventInstanceManager eim, Monster mob, ICombatantObject? attacker, int damage)
        {
            if (mob.getId() == MobId.MOON_BUNNY)
            {
                var bunnyDamage = eim.getIntProperty("bunnyDamaged") + 1;
                if (bunnyDamage % 5 == 0)
                {
                    await eim.LightBlue(nameof(ClientMessage.Event_HenesysPQ_Message2));
                }
                eim.setIntProperty("bunnyDamaged", bunnyDamage);
            }
        }

        public override async Task OnFriendlyMobDrop(AbstractEventInstanceManager eim, Monster mob)
        {
            if (mob.getId() == MobId.MOON_BUNNY)
            {
                var cakes = eim.getIntProperty("bunnyCake") + 1;
                eim.setIntProperty("bunnyCake", cakes);
               await  eim.LightBlue(nameof(ClientMessage.Event_HenesysPQ_Message1), cakes.ToString());
            }
        }

        public override async Task OnFriendlyMobKilled(AbstractEventInstanceManager eim, Monster mob, ICombatantObject? killer)
        {
            if (mob.getId() == MobId.MOON_BUNNY)
            {
                await eim.Pink(nameof(ClientMessage.Event_HenesysPQ_Fail));
                await End(eim, TerminationReason.Failure);
            }

        }

        public override string? HandleCreateInstanceResult(CreateInstanceResult r, IChannelClient c)
        {
            switch (r)
            {
                case CreateInstanceResult.Success:
                    break;
                case CreateInstanceResult.RequiredParty:
                    c.CurrentCulture.GetScriptTalkByKey(nameof(ScriptTalk.HenesysPQ_EnterTalk1));
                    break;
                case CreateInstanceResult.RequiredLeader:
                    c.CurrentCulture.GetScriptTalkByKey(nameof(ScriptTalk.PartyQuest_NeedLeaderTalk));
                    break;
                case CreateInstanceResult.Requirement:
                    c.CurrentCulture.GetScriptTalkByKey(nameof(ScriptTalk.PartyQuest_CannotStart_Req));
                    break;
                case CreateInstanceResult.LobbyLimited:
                    c.CurrentCulture.GetScriptTalkByKey(nameof(ScriptTalk.PartyQuest_CannotStart_ChannelFull));
                    break;
                case CreateInstanceResult.Disposed:
                case CreateInstanceResult.Unknown:
                    c.CurrentCulture.GetScriptTalkByKey("未知错误");
                    break;
                default:
                    break;
            }
            return null;
        }
    }
}
