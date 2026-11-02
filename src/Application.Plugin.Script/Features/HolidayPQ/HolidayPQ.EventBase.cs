using Application.Core.Channel.DataProviders;
using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;
using Application.Core.Server.life;
using System.Drawing;

namespace Application.Plugin.Script.Features.HolidayPQ
{
    /// <summary>
    /// 假日组队任务（Maplemas PQ）三个难度共用的基类。
    /// 对应 JS scripts/event/HolidayPQ_{1..3}.js —— 规则一致，只是地图组与等级段随难度变化。
    /// 玩法：雪人靠「雪之力量」长大，假道具会让它缩小；雪人成长到顶级后由玩家击败斯克鲁奇通关。
    /// </summary>
    internal abstract class HolidayPQEventBase : AbstractPartyQuestEventTemplate
    {
        /// <summary>真实的雪之力量。</summary>
        internal const int SnowVigor = 4032094;

        /// <summary>假的雪之力量，会让雪人缩小。</summary>
        internal const int FakeSnowVigor = 4032095;

        /// <summary>难度：1~3，决定地图基数与等级段。</summary>
        internal int Difficulty { get; }

        protected HolidayPQEventBase(int difficulty) : base(HolidayPQFeature.EventNameOf(difficulty))
        {
            Difficulty = difficulty;

            MinCount = 3;
            MaxCount = 6;

            var (minLevel, maxLevel, eventMinutes) = difficulty switch
            {
                1 => (21, 30, 15),
                2 => (31, 40, 20),
                _ => (41, 50, 25),
            };

            MinLevel = minLevel;
            MaxLevel = maxLevel;

            var baseMap = 889100000 + (difficulty - 1) * 10;
            RecruitMap = baseMap;
            EntryMap = baseMap + 1;
            ExitMap = baseMap + 2;
            ClearMap = baseMap + 2;

            MinMap = baseMap + 1;
            MaxMap = baseMap + 1;

            EventTime = eventMinutes * 60;
            Type = Shared.Events.EventInstanceType.PartyQuest;
        }

        /// <summary>雪人本体（9400317 + 5*难度 + 等级-1）。</summary>
        internal int SnowmanId(int level) => 9400317 + 5 * Difficulty + (level - 1);

        /// <summary>雪人「血量条」对应的隐藏怪物（9400316 + 5*难度 + 等级）。</summary>
        internal int SnowmanShellId(int level) => 9400316 + 5 * Difficulty + level;

        /// <summary>斯克鲁奇本体。</summary>
        internal int ScroogeId => 9400318 + Difficulty;

        public override async Task OnSetup(AbstractEventInstanceManager eim, int level, int lobbyId)
        {
            eim.Level = level;
            eim.setProperty("level", level);
            eim.setIntProperty("stage", 0);
            eim.setIntProperty("statusStg1", -1);
            eim.setIntProperty("missingDrops", 0);
            eim.setIntProperty("snowmanLevel", 0);
            eim.setIntProperty("snowmanStep", 0);
            eim.setIntProperty("spawnedBoss", 0);

            var map = await eim.getInstanceMap(EntryMap);
            if (map != null)
            {
                map.allowSummonState(false);
            }
        }

        // 三个难度对应的入口地图（889100001 / 889100011 / 889100021）中，
        // life 节点的 mob 全部为 mobTime >= 0，原生 OnTick -> respawn() 已能驱动刷新，
        // 无需 respawnStages。

        public override async Task OnMobKilled(AbstractEventInstanceManager eim, Monster mob, ICombatantObject? killer)
        {
            if (eim.isEventCleared())
                return;

            var mobId = mob.getId();

            // 击败斯克鲁奇 → 通关
            if (mobId >= 9400319 && mobId <= 9400321)
            {
                await eim.showClearEffect();
                await eim.GiveStageClearRewardAll(2 * eim.getIntProperty("level"));
                await eim.clearPQ();
                return;
            }

            // 普通小怪按概率掉落雪之力量（连续 5 次不掉则保底）
            var forceDrop = false;
            if (Random.Shared.NextDouble() >= 0.42)
            {
                var miss = eim.getIntProperty("missingDrops");
                if (miss < 5)
                {
                    eim.setIntProperty("missingDrops", miss + 1);
                    return;
                }

                forceDrop = true;
            }

            var dropper = eim.getPlayers().FirstOrDefault();
            if (dropper == null)
                return;

            var itemId = forceDrop || Random.Shared.NextDouble() < 0.77 ? SnowVigor : FakeSnowVigor;
            var item = ItemInformationProvider.getInstance().GenerateVirtualItemById(itemId, 1);
            await mob.getMap().spawnItemDrop(mob, dropper, item, mob.getPosition(), true, false);
            eim.setIntProperty("missingDrops", 0);
        }

        /// <summary>
        /// 雪人被攻击：等级降到 1 以下则失败，否则降一级。
        /// </summary>
        public override async Task OnFriendlyMobKilled(AbstractEventInstanceManager eim, Monster mob, ICombatantObject? killer)
        {
            eim.setIntProperty("snowmanStep", 0);

            var snowmanLevel = eim.getIntProperty("snowmanLevel");
            if (snowmanLevel <= 1)
            {
                await End(eim, TerminationReason.Failure);
            }
            else
            {
                eim.setIntProperty("snowmanLevel", snowmanLevel - 1);
            }
        }

        /// <summary>
        /// 雪人吃到一个雪之力量：累计到阈值后进化一级。
        /// </summary>
        public static async Task FeedSnowman(AbstractEventInstanceManager eim)
        {
            if (eim.getIntProperty("snowmanLevel") >= 5)
                return;

            var step = eim.getIntProperty("snowmanStep");
            var level = eim.getIntProperty("level");
            var snowmanLevel = eim.getIntProperty("snowmanLevel");

            if (step >= 2 + level * snowmanLevel)
            {
                eim.setIntProperty("snowmanStep", 0);
                await EvolveSnowman(eim, level, snowmanLevel);
                return;
            }

            var map = await eim.getInstanceMap(eim.EventManager.EntryMap);
            var shell = map?.getMonsterById(9400316 + 5 * level + snowmanLevel);
            shell?.heal(200 + 200 * snowmanLevel, 0);

            eim.setIntProperty("snowmanStep", step + 1);
        }

        /// <summary>
        /// 雪人吃到假道具：后退一步。
        /// </summary>
        public static async Task FeedFakeSnowman(AbstractEventInstanceManager eim)
        {
            if (eim.getIntProperty("snowmanLevel") >= 5)
                return;

            var step = eim.getIntProperty("snowmanStep");
            if (step > 0)
            {
                eim.setIntProperty("snowmanStep", step - 1);
            }

            await eim.dropMessage(5, "雪人吸收了假的雪之力量！");
        }

        /// <summary>
        /// 雪人进化一级；到顶级时清场并发放阶段奖励。
        /// </summary>
        static async Task EvolveSnowman(AbstractEventInstanceManager eim, int difficulty, int curLevel)
        {
            var map = await eim.getInstanceMap(eim.EventManager.EntryMap);
            if (map == null)
                return;

            // 先移除当前形态
            await map.killMonster(9400317 + 5 * difficulty + (curLevel - 1));

            // +2 以便被 friendlyKilled 扣回 1
            eim.setIntProperty("snowmanLevel", curLevel + 2);

            var next = LifeFactory.Instance.GetMonsterTrust(9400317 + 5 * difficulty + curLevel);
            await map.spawnMonsterOnGroundBelow(next, new Point(-180, 15));

            if (curLevel >= 4)
            {
                map.allowSummonState(false);
                await map.killAllMonstersNotFriendly();
                map.setReactorState();

                await eim.GiveStageClearRewardAll(2 * difficulty - 1);
                await eim.showClearEffect();
            }
        }

        public override async Task ClearPQ(AbstractEventInstanceManager eim)
        {
            await base.ClearPQ(eim);

            // 通关后给全员加一个庆祝增益
            await eim.applyEventPlayersItemBuff(2022436);
        }
    }
}
