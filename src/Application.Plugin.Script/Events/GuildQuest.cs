using Application.Core.Game.Players;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;

namespace Application.Plugin.Script.Events
{
    /// <summary>
    /// 公会任务（Guild Quest）。
    /// 对应 JS scripts/event/GuildQuest.js。
    /// 通关由阶段脚本调用 clearPQ 触发；额外依赖两段计时器：集合等待 → 正式挑战 → 结束奖励段。
    /// </summary>
    internal class GuildQuest : AbstractPartyQuestEventTemplate
    {
        const int ENTRY_MAP = 990000000;
        const int EXIT_MAP = 990001100;
        const int CLEAR_MAP = 990001000;
        const int SPEAKER_NPC = 9040000;

        /// <summary>公会任务涉及的全部地图（JS 逐张 resetPQ）。</summary>
        static readonly int[] QuestMaps =
        [
            990000000, 990000100, 990000200, 990000300, 990000301, 990000400,
            990000401, 990000410, 990000420, 990000430, 990000431, 990000440,
            990000500, 990000501, 990000502, 990000600, 990000610, 990000611,
            990000620, 990000630, 990000631, 990000640, 990000641, 990000700,
            990000800, 990000900, 990001000, 990001100, 990001101,
        ];

        /// <summary>集合等待时间（秒）。</summary>
        const int WAIT_TIME = 3;
        /// <summary>正式挑战时间（秒）。</summary>
        const int QUEST_TIME = 90;
        /// <summary>结算奖励时间（秒）。</summary>
        const int BONUS_TIME = 30;

        public GuildQuest() : base(nameof(GuildQuest))
        {
            MinCount = 6;
            MaxCount = 30;
            MinLevel = 1;
            MaxLevel = 255;

            RecruitMap = 101030104;
            EntryMap = ENTRY_MAP;
            ExitMap = EXIT_MAP;
            ClearMap = CLEAR_MAP;

            MinMap = 990000000;
            MaxMap = 990001101;

            EventTime = WAIT_TIME;
            Type = Shared.Events.EventInstanceType.PartyQuest;
        }

        public override async Task OnSetup(AbstractEventInstanceManager eim, int level, int lobbyId)
        {
            eim.Level = level;
            eim.setProperty("level", level);

            eim.setIntProperty("guild", 0);
            eim.setIntProperty("canJoin", 1);
            eim.setIntProperty("canRevive", 0);

            eim.setProperty("entryTimestamp", eim.EventManager.ChannelServer.Node.getCurrentTime().ToString());
        }

        public override async Task AfterSeup(AbstractEventInstanceManager eim)
        {
            await base.AfterSeup(eim);

            var leader = eim.getLeader();
            if (leader != null)
            {
                eim.setIntProperty("guild", leader.getGuildId());
            }
        }

        public override async Task OnPlayerEntry(AbstractEventInstanceManager eim, Player chr)
        {
            var map = await eim.getInstanceMap(ENTRY_MAP);
            if (map == null)
                return;

            await chr.changeMap(map, 0);
        }

        public override async Task OnPlayerMapChanged(AbstractEventInstanceManager eim, Player player, int mapid)
        {
            // 进入要塞外围时给出护身符提示（JS 的 afterChangedMap）
            if (mapid == 990000100)
            {
                await player.Dialog("所以，简报是这样的。你们要小心，一旦到了要塞外围，任何没有装备#b#t1032033##k的人都会因为周围恶劣的空气而立刻死亡。"
                    + "话虽如此，队伍出发后，一定要#b敲击那片区域里发光的岩石#k，并在进入下一阶段前#b装备掉落的物品#k。"
                    + "这样就能完全保护你免受空气病的影响。祝你好运！", SPEAKER_NPC);
            }

            await base.OnPlayerMapChanged(eim, player, mapid);
        }

        public override async Task OnTimeOut(AbstractEventInstanceManager eim)
        {
            if (eim.isEventCleared())
            {
                await eim.warpEventTeam(EXIT_MAP);
                return;
            }

            // 第一次超时是“集合等待”结束，转入正式挑战
            if (eim.getIntProperty("canJoin") == 1)
            {
                eim.setIntProperty("canJoin", 0);

                if (eim.checkEventTeamLacking(true, MinCount))
                {
                    await End(eim, TerminationReason.MemberCount);
                    return;
                }

                await eim.startEventTimer(QUEST_TIME * 60_000L);

                // 全职业队伍额外获得一个随机增益道具
                if (IsTeamAllJobs(eim))
                {
                    await eim.applyEventPlayersItemBuff(2023000 + Random.Shared.Next(4));
                }
            }
            else
            {
                await End(eim, TerminationReason.Timeout);
            }
        }

        public override async Task ClearPQ(AbstractEventInstanceManager eim)
        {
            await eim.stopEventTimer();
            await eim.setEventCleared();

            await eim.warpEventTeam(CLEAR_MAP);

            // 结算奖励段：通关后仍有 30 秒可以出场
            await eim.startEventTimer(BONUS_TIME * 60_000L);
        }

        public override async Task OnPlayerUnregister(AbstractEventInstanceManager eim, Player chr)
        {
            chr.CancelBuffFromSourceId(2023000);
            chr.CancelBuffFromSourceId(2023001);
            chr.CancelBuffFromSourceId(2023002);
            chr.CancelBuffFromSourceId(2023003);

            await base.OnPlayerUnregister(eim, chr);
        }

        public override async Task OnPlayerDied(AbstractEventInstanceManager eim, Player chr)
        {
            // 在最终房间团灭且仍有怪物时直接结束
            if (chr.getMapId() == 990000900
                && chr.getMap() is { } map
                && map.countAlivePlayers() == 0
                && map.countMonsters() > 0)
            {
                await End(eim, TerminationReason.Failure);
            }
        }

        public override async Task<bool> OnPlayerRevive(AbstractEventInstanceManager eim, Player player)
        {
            if (eim.getIntProperty("canRevive") == 0)
            {
                await player.respawn(EXIT_MAP);

                if (eim.getIntProperty("canJoin") == 0 && IsEventTeamLackingNow(eim, true, player))
                {
                    await End(eim, TerminationReason.MemberCount);
                }

                return false;
            }

            return await base.OnPlayerRevive(eim, player);
        }

        /// <summary>队伍是否凑齐 5 个基础职业（战士/法师/弓箭手/飞侠/海盗）。</summary>
        static bool IsTeamAllJobs(AbstractEventInstanceManager eim)
        {
            var rangeJobs = Convert.ToInt32("111110", 2);
            return (eim.getEventPlayersJobs() & rangeJobs) == rangeJobs;
        }
    }
}
