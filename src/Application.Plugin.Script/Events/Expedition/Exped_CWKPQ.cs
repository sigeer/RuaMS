using Application.Core.Game.Maps;
using Application.Core.Game.Players;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;
using Application.Core.Server.life;
using Application.Resources.Messages;
using Application.Shared.Battle;

namespace Application.Plugin.Script.Events
{
    /// <summary>
    /// 红树林要塞组队任务（CWKPQ）。对应 JS scripts/event/CWKPQ.js。
    /// 通关由各阶段守卫者脚本调用 clearPQ 触发（JS 的 monsterKilled 为空函数），
    /// 阶段推进依赖 changedMap 重启计时器。
    /// </summary>
    internal class Exped_CWKPQ : AbstractExpeditionEventTemplate
    {
        const int ENTRY_MAP = 610030100;
        const int EXIT_MAP = 610030020;
        const int GUARDIAN_MOB = 9400594;

        static readonly int[] InstanceMaps =
        [
            610030100, 610030200, 610030300, 610030400, 610030500,
            610030510, 610030520, 610030521, 610030522, 610030530,
            610030540, 610030550, 610030600, 610030700, 610030800,
        ];

        /// <summary>
        /// 进入各阶段地图时的计时器（毫秒）与对应的 instance 序号。
        /// Key: 地图 Id，Value: (要求的 current_instance, 新的 current_instance, 毫秒)
        /// </summary>
        static readonly Dictionary<int, (int Required, int Next, long Time)> StageTimers = new()
        {
            [610030200] = (0, 1, 600_000),   // 10 分钟
            [610030300] = (1, 2, 600_000),
            [610030400] = (2, 3, 600_000),
            [610030500] = (3, 4, 1_200_000), // 20 分钟
            [610030600] = (4, 5, 3_600_000), // 1 小时
            [610030800] = (5, 6, 60_000),    // 1 分钟
        };

        /// <summary>
        /// 4 技能石的位置布局。每列对应一个位置可选的候选反应堆名（-1 表示空位）。
        /// </summary>
        static readonly int[][] JobReactorSlots =
        [
            [0, 0, -1, -1, 0],
            [-1, 4, 3, 3, 3],
            [1, 3, 4, 2, 2],
            [2, -1, 0, 1, -1],
            [3, 2, 1, 0, -1],
            [4, 1, -1, 4, 1],
            [-1, 2, 4],
            [-1, -1],
        ];

        static readonly string[][] ReactorNames =
        [
            ["4skill0a", "4skill0b", "4fake1c", "4fake1d", "4skill0e"],
            ["4fake0a", "4skill4b", "4skill3c", "4skill3d", "4skill3e"],
            ["4skill1a", "4skill3b", "4skill4c", "4skill2d", "4skill2e"],
            ["4skill2a", "4fake1b", "4skill0c", "4skill1d", "4fake1e"],
            ["4skill3a", "4skill2b", "4skill1c", "4skill0d", "4fake0e"],
            ["4skill4a", "4skill1b", "4fake0c", "4skill4d", "4skill1e"],
            ["4fake1a", "4skill2c", "4skill4e"],
            ["4fake0b", "4fake0d"],
        ];

        public Exped_CWKPQ()
            : base(GUARDIAN_MOB, nameof(Exped_CWKPQ), ExpeditionEntryType.CWKPQ.Name)
        {
            MinCount = 6;
            MaxCount = 30;

            MinLevel = 90;
            MaxLevel = 255;

            RecruitMap = EXIT_MAP;
            EntryMap = ENTRY_MAP;
            ExitMap = EXIT_MAP;
            ClearMap = EXIT_MAP;

            MinMap = ENTRY_MAP;
            MaxMap = 610030800;

            EventTime = 2 * 60;
            RegistrationTime = 5 * 60;
        }

        public override async Task OnSetup(AbstractEventInstanceManager eim, int level, int lobbyId)
        {
            eim.Level = level;
            eim.setProperty("level", level);
            eim.setIntProperty("canJoin", 1);
            eim.setIntProperty("defeatedBoss", 0);
            eim.setIntProperty("current_instance", 0);

            for (var i = 1; i <= 6; i++)
            {
                eim.setIntProperty($"glpq{i}", 0);
            }
            eim.setIntProperty("glpq3_p", 0);
            eim.setIntProperty("glpq5_room", 0);
            for (var i = 0; i <= 7; i++)
            {
                eim.setIntProperty($"glpq_f{i}", 0);
            }
            eim.setIntProperty("glpq_s", 0);


            var map400 = await eim.getInstanceMap(610030400);
            if (map400 != null)
            {
                GenerateMapReactors(map400);
                HideWrongEnvironments(map400);
            }

            (await eim.getInstanceMap(610030550))?.shuffleReactors();

            // 第一阶段的守卫者
            var mapEntry = await eim.getInstanceMap(ENTRY_MAP);
            if (mapEntry != null)
            {
                var mob = LifeFactory.Instance.GetMonsterTrust(GUARDIAN_MOB);
                await mapEntry.spawnMonsterOnGroundBelow(mob, new System.Drawing.Point(1000, 336));
            }

            // 60 秒后一大波守卫者
            eim.Schedule(SpawnGuardians, 60_000);
        }

        /// <summary>
        /// 随机摆放 4 技能石，保证 6 个职业各有一个可用技能石。
        /// </summary>
        static void GenerateMapReactors(IMap map)
        {
            int[] indexes;
            while (true)
            {
                indexes = new int[JobReactorSlots.Length];
                var found = new HashSet<int>();

                for (var i = 0; i < JobReactorSlots.Length; i++)
                {
                    var slot = JobReactorSlots[i];
                    var idx = Random.Shared.Next(slot.Length);
                    indexes[i] = idx;

                    var job = slot[idx];
                    if (job >= 0)
                    {
                        found.Add(job);
                    }
                }

                if (found.Count == 6)
                {
                    break;
                }
            }

            var toRandomize = new List<object>();
            for (var i = 0; i < indexes.Length; i++)
            {
                var name = ReactorNames[i][indexes[i]];
                var reactor = map.getReactorByName(name);
                if (reactor == null)
                    continue;

                reactor.setState(1);
                toRandomize.Add(reactor);
            }

            map.shuffleReactors(toRandomize);
        }

        /// <summary>
        /// 隐藏 4 技能地图中不属于正确路径的环境对象（JS 的 moveEnvironment 初始状态）。
        /// </summary>
        static void HideWrongEnvironments(IMap map)
        {
            string[] rows = ["a", "b", "c", "d", "e", "f", "g", "h", "i"];
            for (var x = 0; x < rows.Length; x++)
            {
                for (var y = 1; y <= 7; y++)
                {
                    if (x == 1 || x == 3 || x == 4 || x == 6 || x == 8)
                    {
                        if (y != 2 && y != 4 && y != 5 && y != 7)
                        {
                            _ = map.moveEnvironment(rows[x] + y, 1);
                        }
                    }
                    else
                    {
                        _ = map.moveEnvironment(rows[x] + y, 1);
                    }
                }
            }
        }

        static async Task SpawnGuardians(AbstractEventInstanceManager eim)
        {
            var map = await eim.getInstanceMap(ENTRY_MAP);
            if (map == null || map.countPlayers() <= 0)
                return;

            await eim.Pink(nameof(ClientMessage.Event_CWKPQ_DetectedByGuardian));

            for (var i = 0; i < 20; i++)
            {
                var mob = LifeFactory.Instance.GetMonsterTrust(GUARDIAN_MOB);
                await map.spawnMonsterOnGroundBelow(mob, new System.Drawing.Point(1000, 336));
            }
        }

        public override async Task OnPlayerEntry(AbstractEventInstanceManager eim, Player chr)
        {
            await eim.Pink(nameof(ClientMessage.Expedition_EnterMap), chr.Name);

            var map = await eim.getInstanceMap(ENTRY_MAP + eim.getIntProperty("current_instance") * 100);
            if (map == null)
                return;

            await chr.changeMap(map, 0);
        }

        public override async Task OnPlayerMapChanged(AbstractEventInstanceManager eim, Player player, int mapid)
        {
            if (StageTimers.TryGetValue(mapid, out var stage)
                && eim.getIntProperty("current_instance") == stage.Required)
            {
                eim.setIntProperty("current_instance", stage.Next);
                await eim.restartEventTimer(stage.Time);
            }

            await base.OnPlayerMapChanged(eim, player, mapid);
        }
    }
}
