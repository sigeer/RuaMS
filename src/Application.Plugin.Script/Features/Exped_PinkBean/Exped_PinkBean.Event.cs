using Application.Core.Game.Life;
using Application.Core.Game.Maps;
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
                var mob = LifeFactory.Instance.GetMonsterTrust(PINKBEAN_MOB);
                await entry.spawnMonsterOnGroundBelow(mob, new System.Drawing.Point(0, -42),
                    m => m.disableDrops());
            }
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
                // 最后一波守卫倒下，粉豆失去无敌
                await eim.dropMessage(6, "最后的守卫倒下后，粉豆失去了无敌状态。真正的战斗现在开始！");
                return;
            }

            eim.setIntProperty("stage", stage + 1);
            await eim.dropMessage(5, "下一波守卫将在 15 秒后出现，做好准备。");
            await eim.EventManager.ChannelServer.TimerManager.schedule(
                new Application.Utility.Tasks.NamedRunnable("PinkBean_StartWave", () => _ = StartWave(eim)),
                TimeSpan.FromSeconds(15));
        }

        /// <summary>
        /// 召唤下一波小 Boss。由 OnMobKilled 通过 eim.schedule 触发。
        /// </summary>
        public async Task StartWave(AbstractEventInstanceManager eim)
        {
            var map = await eim.getInstanceMap(ENTRY_MAP);
            if (map == null)
                return;

            var stage = eim.getIntProperty("stage");
            for (var i = 1; i <= stage; i++)
            {
                var mobId = 8820019 + (i % 5);
                var mob = LifeFactory.Instance.GetMonsterTrust(mobId);
                await map.spawnMonsterOnGroundBelow(mob, new System.Drawing.Point(0, -42));
            }
        }

    }
}
