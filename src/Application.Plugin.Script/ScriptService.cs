using Application.Core.Channel;
using Application.Core.Game.Maps;
using Application.Plugin.Events;
using Application.Plugin.Script.Npc;

namespace Application.Plugin.Script
{
    /// <summary>
    /// 默认脚本插件。
    ///
    /// 它只回答「本插件声明了哪些脚本」——每个 <c>Load*Scripts()</c> 内部做类型扫描并返回描述符集合。
    /// 跨插件的合并、优先级裁决、重名日志全部由 <see cref="PluginManager"/> 负责；
    /// 因此这里不持有任何注册表，插件热更新时也不会留下需要清理的缓存。
    ///
    /// 脚本来源只有一处：整个程序集里声明了脚本的类型——
    /// 既包括按种类划分的旧式 partial 类（<see cref="NpcScript"/> 等），
    /// 也包括 <c>Features/</c> 下按功能目录组织的处理器。
    /// </summary>
    internal class ScriptService : PluginServiceBase,
        IScriptNpcService,
        IScriptQuestService,
        IScriptItemService,
        IScriptPortalService,
        IScriptMapService,
        IScriptReactorService,
        IScriptEventService,
        IPluginMapService
    {
        Dictionary<int, AreaBossOption> _mapBoss = [];

        public ScriptService(PluginContainer<PluginServiceBase> pluginContainer) : base(pluginContainer)
        {
            // 如果一张地图有多个BOSS还需要改造
            List<AreaBossOption> options = [
                new ("Deo",260010201, 3220001, 10800, "Deo slowly appeared out of the sand dust.", [new(645,645,275)]),
                new ("Bamboo",800020120, 6090002, 10800, "From amo…ouded by the mists, Bamboo Warrior appears.", [new(560, 560, 50)]),
                new ("Centipede",251010102, 5220004, 10800, "From …n, the gargantuous Giant Centipede appears.", [new(560, 560, 50)]),
                new ("Kimera",261030000, 8220002, 10800, "吉米拉从地下的黑暗中出现，眼中闪烁着微光。", [new(-900, 0,180)]),
                new ("KingClang",110040000, 5220001, 10800, "一个奇怪的海螺出现在了沙滩上。", [new(-1600,800, 140)]),
                new ("Faust1",100040105, 5220002, 10800, "浮士德出现在蓝色迷雾中。", [new(456, 456, 278)]),
                new ("Faust2",100040106, 5220002, 10800, "浮士德出现在蓝色迷雾中。", [new(474, 474, 278)]),
                new ("Eliza",200010300, 8220000, 10800, "Eliza has appeared with a black whirlwind.", [new(208, 208, 83)]),
                new ("Dyle",107000300, 6220000, 10800, "The huge crocodile Dyle has come out from the swamp.", [new(90, 90, 119)]),
                new ("Mano",104000400, 2220000, 10800, "A cool breeze was felt when Mano appeared.", [new(279, 279, -496)]),
                new ("Zeno",221040301, 6220001, 10800, "Zeno has appeared with a heavy sound of machinery.", [new(-4224, -4224, 776)]),
                new ("TaeRoon",250010304, 7220000, 10800, "Tae Roon has appeared with a soft whistling sound.", [new(-800, -100,390)]),
                new ("Stumpy",101030404, 3220000, 10800, "Stumpy h…umping sound that rings the Stone Mountain.", [new(400, 1200,1280)]),
                new ("KingSageCat",250010504, 7220002, 10800, "周围的鬼气更加浓重了。传来一阵令人不快的猫叫声。", [new(-500, 800,540)]),
                new ("NineTailedFox",222010310, 7220001, 10800, "A…and the presence of the old fox can be felt", [new(-800, 500,33)]),
                new ("Seruf",230020100, 4220001, 10800, "A strange shell has appeared from a grove of seaweed", [new(-1500, 800,520)]),
                new ("Leviathan",240040401, 8220003, 10800, "Levia…rom the canyon and the cold icy wind blows.", [new(-300, 300,1125)]),
                new ("SnackBar",105090310, 8220008, 10800, "一个可疑的小吃摊在一个奇怪的偏僻地方慢慢开张了。", [new(-626, -626, -604), new (735, 735,-600)]),
                new("Timer1", 220050100, 5220003, 10800, "Tick-Toc…Tick-Tock! Timer makes it's presence known.", [new(-770, 0,1030)]),
                new("Timer2", 220050000, 5220003, 10800, "Tick-Toc…Tick-Tock! Timer makes it's presence known.", [new(-1000, 400,1030)]),
                new("Timer3", 220050200, 5220003, 10800, "Tick-Toc…Tick-Tock! Timer makes it's presence known.", [new(-700, 700, 1030)])];
            _mapBoss = options.ToDictionary(x => x.MapId);
        }

        /// <summary>
        /// 默认脚本插件使用最低优先级，使第三方插件以默认优先级即可覆盖同名脚本
        /// </summary>
        public override PluginPriority Priority => PluginPriority.Low;

        public async Task OnMapLoad(IMap map)
        {
            if (_mapBoss.TryGetValue(map.Id, out var item))
            {
                await map.SetupAreaBoss(item.Name, item.BossId, item.Period, item.Point, item.Message);
            }
        }

        public Task OnMapUnload(IMap map)
        {
            return Task.CompletedTask;
        }

        public override Task OnMounted()
        {
            foreach (var channel in _node.Servers.Values.OfType<WorldChannel>().ToArray())
            {
                LogInfo("{ServerName} 事件模板共 {EventCount} 项", channel.InstanceName, channel.EventScriptManager.EventCount);
            }

            return base.OnMounted();
        }

        public override async ValueTask DisposeAsync()
        {
            foreach (var channel in _node.Servers.Values.OfType<WorldChannel>().ToArray())
            {
                LogInfo("{ServerName} 卸载中...", channel.InstanceName);

                var mapManager = channel.getMapFactory();
                var mapBoss = _mapBoss.Values.Select(x => (x.MapId, x.Name)).ToArray();
                foreach (var (mapId, name) in mapBoss)
                {
                    if (mapManager.TryGetMap(mapId, out var map))
                    {
                        map.ClearAreaBoss(name);
                    }
                }
                LogInfo("{ServerName} 卸载事件...完成", channel.InstanceName);
            }
            _mapBoss.Clear();
        }


    }
}
