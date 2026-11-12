using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.Game.Players;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;
using Application.Core.Server.maps;
using Application.Resources.Messages;
using Application.Shared.Constants.Mob;

namespace Application.Plugin.Script.Features.PQ_Henesys
{
    internal sealed class Event : AbstractPartyQuestEventTemplate
    {
        public Event() : base(PQ_HenesysFeature.EventName)
        {
            QuestId = 1200;

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
                await eventMap.killAllMonsters();
                eventMap.RespawnInterval = 15_000;
            }

            var eventMap2 = await eim.getInstanceMap(910010200);
            if (eventMap2 != null)
                eventMap2.RespawnInterval = 15_000;
        }

        public override async Task respawnStages(AbstractEventInstanceManager eim)
        {
            if (eim.GetStageStatus(EntryMap) == StageStatus.NotStarted)
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
                // JS：累计超过 5 次才提示并清零，否则继续累加
                var bunnyDamage = eim.getIntProperty("bunnyDamaged") + 1;
                if (bunnyDamage > 5)
                {
                    await eim.dropMessage(5, "玉兔月妙受到了伤害，请保护好它，这样它才能继续做出美味的年糕！");
                    eim.setIntProperty("bunnyDamaged", 0);
                }
                else
                {
                    eim.setIntProperty("bunnyDamaged", bunnyDamage);
                }
            }
        }

        public override async Task OnFriendlyMobDrop(AbstractEventInstanceManager eim, Monster mob)
        {
            if (mob.getId() == MobId.MOON_BUNNY)
            {
                var cakes = eim.getIntProperty("bunnyCake") + 1;
                eim.setIntProperty("bunnyCake", cakes);
                await eim.dropMessage(6, $"玉兔月妙成功捣出了第 {cakes} 份年糕！");
            }
        }

        public override async Task OnFriendlyMobKilled(AbstractEventInstanceManager eim, Monster mob, ICombatantObject? killer)
        {
            if (mob.getId() == MobId.MOON_BUNNY)
            {
                await eim.dropMessage(5, "因未能保护好月妙导致其重伤，你已被传送至流放之地！");
                // JS 延迟 5 秒再失败传送，给玩家一个缓冲
                eim.Schedule(async instance =>
                {
                    await End(instance, TerminationReason.Failure);
                }, 5_000);
            }

            await base.OnFriendlyMobKilled(eim, mob, killer);
        }

        public override async Task ClearPQ(AbstractEventInstanceManager eim)
        {
            await base.ClearPQ(eim);

            // 通关后全队进入奖励图
            await eim.warpEventTeam(910010100);
        }

        public override async Task OnTimeOut(AbstractEventInstanceManager eim)
        {
            // 第一阶段已过（存在 "1stageclear"）时只把队伍送到 910010400，不结束实例
            if (eim.getProperty("1stageclear") != null)
            {
                await eim.warpEventTeam(910010200, 910010400);
                return;
            }

            await base.OnTimeOut(eim);
        }

        /// <summary>
        /// 流放图 910010300 也视为场外（JS 的 changedMap 显式判定），离开时正常退队。
        /// </summary>
        public override async Task OnPlayerMapChanging(AbstractEventInstanceManager eim, Player player, int mapid)
        {
            if (mapid == ExitMap)
            {
                await eim.unregisterPlayer(player);
                return;
            }

            await base.OnPlayerMapChanging(eim, player, mapid);
        }

        public override string? HandleCreateInstanceResult(CreateInstanceResult r, IChannelClient c)
        {
            switch (r)
            {
                case CreateInstanceResult.Success:
                    break;
                case CreateInstanceResult.RequiredParty:
                    c.CurrentCulture.GetMessageByCategory(ScriptMessageCategories.ScriptMessage, nameof(ScriptMessage.HenesysPQ_EnterTalk1));
                    break;
                case CreateInstanceResult.RequiredLeader:
                    c.CurrentCulture.GetMessageByKey(nameof(ClientMessage.PartyQuest_NeedLeaderTalk));
                    break;
                case CreateInstanceResult.Requirement:
                    c.CurrentCulture.GetMessageByKey(nameof(ClientMessage.PartyQuest_CannotStart_Req));
                    break;
                case CreateInstanceResult.LobbyLimited:
                    c.CurrentCulture.GetMessageByKey(nameof(ClientMessage.PartyQuest_CannotStart_ChannelFull));
                    break;
                case CreateInstanceResult.Disposed:
                case CreateInstanceResult.Unknown:
                    c.CurrentCulture.GetMessageByCategory(ScriptMessageCategories.ScriptMessage, "未知错误");
                    break;
                default:
                    break;
            }
            return null;
        }

    }
}
