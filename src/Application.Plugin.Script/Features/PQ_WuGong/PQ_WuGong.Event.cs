using Application.Core.Channel.DataProviders;
using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.Server.life;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;
using Application.Shared.Items;
using System.Drawing;

namespace Application.Plugin.Script.Features.PQ_WuGong
{
    /// <summary>
    /// 武公组队任务。对应 JS <c>scripts/event/WuGongPQ.js</c>。
    /// 进入后清空地图原有怪物，按难度倍数召唤 9600009，击杀后掉落 0~5 个 2000005 并通关。
    /// </summary>
    internal sealed class Event : AbstractPartyQuestEventTemplate
    {
        /// <summary>待召唤的 Boss。</summary>
        internal const int BossId = 9600009;

        /// <summary>Boss 召唤坐标。</summary>
        internal const int SpawnX = 2261;
        internal const int SpawnY = 823;

        /// <summary>Boss 掉落：物品、最大数量、掉率。</summary>
        internal const int DropItemId = 2000005;
        internal const int DropMaxCount = 5;
        internal const double DropChance = 0.4;

        public Event() : base(PQ_WuGongFeature.EventName)
        {
            MinCount = 1;
            MaxCount = 6;
            MinLevel = 25;
            MaxLevel = 90;
            EntryMap = 701010323;
            ExitMap = 701010320;
            RecruitMap = 701010322;
            ClearMap = 701010323;
            MinMap = 701010323;
            MaxMap = 701010323;
            EventTime = 10 * 60;
            Type = Shared.Events.EventInstanceType.PartyQuest;
        }

        public override async Task OnSetup(AbstractEventInstanceManager eim, int level, int lobbyId)
        {
            var map = await eim.getMapInstance(EntryMap);
            await map.killAllMonsters();

            // 按难度缩放后召唤，坐标与 JS 一致
            var mob = LifeFactory.Instance.GetMonsterTrust(BossId);
            await map.spawnMonsterOnGroundBelow(mob, new Point(SpawnX, SpawnY),
                m => m.changeDifficulty(level <= 0 ? 1 : level, true));

            await base.OnSetup(eim, level, lobbyId);
        }

        public override async Task OnMobKilled(AbstractEventInstanceManager eim, Monster mob, ICombatantObject? killer)
        {
            if (!eim.isEventCleared() && mob.getId() == BossId)
            {
                // 预掷掉落：0~5 个，各 40%
                var map = mob.getMap();
                var dropper = eim.getPlayers().FirstOrDefault();
                if (dropper != null)
                {
                    var dropPos = mob.getPosition();
                    for (var i = 0; i < DropMaxCount; i++)
                    {
                        if (Random.Shared.NextDouble() >= DropChance)
                            continue;

                        var item = ItemInformationProvider.getInstance().GenerateVirtualItemById(DropItemId, 1);
                        await map.spawnItemDrop(mob, dropper, item, dropPos, false, false);
                    }
                }

                await eim.clearPQ();
            }

            await base.OnMobKilled(eim, mob, killer);
        }

        public override async Task ClearPQ(AbstractEventInstanceManager eim)
        {
            await base.ClearPQ(eim);

            // 通关后再给 2 分钟清场时间
            await eim.startEventTimer(2 * 60_000);
        }

        public override string? HandleCreateInstanceResult(CreateInstanceResult r, IChannelClient c)
        {
            switch (r)
            {
                case CreateInstanceResult.Success:
                    return null;
                case CreateInstanceResult.RequiredParty:
                    return "该项#b秘密任务#k十分危险，你需要进行一个组队才能进入执行。";

                case CreateInstanceResult.RequiredLeader:
                    return "请让你的队长来开始这个任务。";

                case CreateInstanceResult.Requirement:
                    return "你目前无法执行这个#b秘密任务#k，因你不符合要求";

                case CreateInstanceResult.LobbyLimited:
                    return "已经有其他人员在执行#b秘密任务#k了，请稍后再试。";

                case CreateInstanceResult.Disposed:
                case CreateInstanceResult.Unknown:
                default:
                    return "未知错误";
            }
        }

    }
}
