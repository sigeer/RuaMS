using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.Game.Players;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;
using Application.Core.Server.life;
using Application.Shared.Battle;

namespace Application.Plugin.Script.Features.Exped_PinkBean
{
    internal sealed class Event : AbstractExpeditionEventTemplate
    {
        const int ENTRY_MAP = 270050100;
        const int MID_MAP = 270050200;
        const int CLEAR_MAP = 270050300;
        const int RECRUIT_MAP = 270050000;

        const int PINKBEAN_MOB = 8820000;
        const int PINKBEAN_REAL = 8820001;
        const int JR_BOSS_MIN = 8820002;
        const int JR_BOSS_MAX = 8820006;

        public Event()
            : base(PINKBEAN_MOB, Exped_PinkBeanFeature.EventName, ExpeditionEntryType.PINKBEAN.Name)
        {
            MinCount = 6;
            MaxCount = 30;

            MinLevel = 120;
            MaxLevel = 255;

            RecruitMap = RECRUIT_MAP;
            EntryMap = ENTRY_MAP;
            ExitMap = CLEAR_MAP;
            ClearMap = CLEAR_MAP;

            MinMap = ENTRY_MAP;
            MaxMap = CLEAR_MAP;

            EventTime = 140 * 60;
            RegistrationTime = 5 * 60;
        }

        public override async Task OnSetup(AbstractEventInstanceManager eim, int level, int lobbyId)
        {
            eim.Level = level;
            eim.setProperty("level", level);
            eim.setIntProperty("canJoin", 1);
            eim.setIntProperty("defeatedBoss", 0);
            eim.setIntProperty("fallenPlayers", 0);
            eim.setIntProperty("stage", 1);
            eim.setIntProperty("channel", eim.ChannelServer.getId());

            var entry = await eim.getInstanceMap(ENTRY_MAP);
            if (entry != null)
            {
                await entry.killAllMonsters();
                var mob = LifeFactory.Instance.GetMonsterTrust(PINKBEAN_MOB);
                await entry.spawnMonsterOnGroundBelow(mob, new System.Drawing.Point(0, -42),
                    m => m.disableDrops());
            }
        }

        public override async Task OnBattleStarted(AbstractEventInstanceManager eim)
        {
            await base.OnBattleStarted(eim);

            // 第一波在 15 秒后开始
            await eim.dropMessage(5, "第一波攻击将在15秒后开始，请做好准备。");
            eim.Schedule(StartWave, 15_000);
        }

        public override async Task OnMobKilled(AbstractEventInstanceManager eim, Monster mob, ICombatantObject? killer)
        {
            var mobId = mob.getId();

            if (mobId == PINKBEAN_REAL)
            {
                eim.setIntProperty("defeatedBoss", 1);
                await eim.showClearEffect(mob.getMap().getId());
                await mob.getMap().killAllMonsters();
                await eim.clearPQ();
                return;
            }

            if (mobId < JR_BOSS_MIN || mobId > JR_BOSS_MAX)
                return;

            if (mob.getMap().countMonster(JR_BOSS_MIN, JR_BOSS_MAX) != 0)
                return;

            var stage = eim.getIntProperty("stage");
            if (stage >= 5)
            {
                // 最后一波守卫倒下：在反应堆 2708000 处掉落通关道具，粉豆失去无敌
                var bossMap = await eim.getInstanceMap(ENTRY_MAP);
                var reactor = bossMap?.getReactorById(2708000);
                var dropper = eim.getPlayers().FirstOrDefault();
                if (bossMap != null && reactor != null && dropper != null)
                {
                    var item = Application.Core.Channel.DataProviders.ItemInformationProvider.getInstance()
                        .GenerateVirtualItemById(4001193, 1);
                    await bossMap.spawnItemDrop(dropper, dropper, item, reactor.getPosition(), true, true);
                }

                await eim.dropMessage(6, "随着最后的守护者倒下，品克缤失去了无敌状态。真正的战斗现在开始！");
                return;
            }

            eim.setIntProperty("stage", stage + 1);
            await eim.Pink("下一波守卫将在 15 秒后出现，做好准备。");
            eim.Schedule(StartWave, 15_000);
        }

        /// <summary>
        /// 召唤下一波小 Boss。JS 的做法是杀掉地图上的占位体 8820019~8820023，
        /// 再在同一位置刷出真正的 8820002~8820006（id - 17）。
        /// </summary>
        public async Task StartWave(AbstractEventInstanceManager eim)
        {
            var map = await eim.getInstanceMap(ENTRY_MAP);
            if (map == null)
                return;

            var stage = eim.getIntProperty("stage");
            for (var i = 1; i <= stage; i++)
            {
                var placeholderId = 8820019 + (i % 5);
                var placeholder = map.getMonsterById(placeholderId);
                if (placeholder == null)
                    continue;

                var pos = placeholder.getPosition();
                await map.killMonster(placeholderId);

                var real = LifeFactory.Instance.GetMonsterTrust(placeholderId - 17);
                await map.spawnMonsterOnGroundBelow(real, pos);
            }
        }

        public override async Task OnPlayerDied(AbstractEventInstanceManager eim, Player chr)
        {
            // 团灭保护：累计阵亡 3/4/5 人分别提示，第 5 人结束远征
            var count = eim.getIntProperty("fallenPlayers") + 1;
            eim.setIntProperty("fallenPlayers", count);

            if (count == 5)
            {
                await eim.dropMessage(5, "[远征队] 太多队员阵亡，品克缤现在被视为不可战胜，远征结束。");
                await End(eim, TerminationReason.Failure);
            }
            else if (count == 4)
            {
                await eim.dropMessage(5, "[远征队] 品克缤变得比以往更强大，大家进入背水一战模式！");
            }
            else if (count == 3)
            {
                await eim.dropMessage(5, "[远征队] 伤亡人数开始失控，请小心战斗。");
            }

            await base.OnPlayerDied(eim, chr);
        }

        public override async Task OnMobRevive(AbstractEventInstanceManager eim, Monster mob)
        {
            // 假品克缤 8820000 被禁掉落，复活成真身 8820001 时要恢复，否则真品克缤永久不掉落
            if (mob.getId() == PINKBEAN_REAL)
                mob.enableDrops();

            await base.OnMobRevive(eim, mob);
        }

    }
}
