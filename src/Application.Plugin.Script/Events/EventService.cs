using Application.Core.Channel;
using Application.Core.Game.Maps;
using Application.Core.scripting.Events.Templates;
using Application.Plugin.Script.Events;
using Application.Shared.Constants;
using Application.Shared.Constants.Job;
using Application.Shared.Constants.Map;
using Application.Shared.Quest;
using Serilog;

namespace Application.Plugin.Events
{
    internal record AreaBossOption(string Name, int MapId, int BossId, int Period, string Message, List<RandomPoint> Point);

    internal class EventService : IPluginMapService
    {
        Dictionary<int, AreaBossOption> _mapBoss = [];
        HashSet<AbstractEventTemplate> _events;
        WorldChannelServer? _node;
        public EventService()
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

            _events = [
                new PQ_Henesys(),
                new PQ_Kerning(),
                new PQ_Ellin(),
                new PQ_Ludi(),
                new PQ_WuGong(),

                new PQ_CPQ1( nameof(PQ_CPQ1) + "1", 980000100),
                new PQ_CPQ1( nameof(PQ_CPQ1) + "2", 980000200),
                new PQ_CPQ1( nameof(PQ_CPQ1) + "3", 980000300),

                new PQ_Ariant( nameof(PQ_Ariant) + "1", 980010100),
                new PQ_Ariant( nameof(PQ_Ariant) + "2", 980010200),
                new PQ_Ariant( nameof(PQ_Ariant) + "3", 980010300),

                new PQ_Zakum(),
                new Battle_Zakum(),

                new Battle_Balrog(),

                new PrivateContiMove( "KerningTrain", [103000100, 103000310], [103000301, 103000302], 50),
                // 天空之城 - 圣地
                new PrivateContiMove( "ShipOribs", [MapId.ORBIS_STATION, MapId.SKY_FERRY],[200090020, 200090021], 8 * 60),
                // 魔法密林 - 圣地
                new PrivateContiMove( "ShipEllin", [MapId.ELLINIA_SKY_FERRY, MapId.SKY_FERRY],[MapId.FROM_ELLINIA_TO_EREVE, MapId.FROM_EREVE_TO_ELLINIA], 2 * 60),
                // 里恩 - 明珠港
                new PrivateContiMove( "Whale", [MapId.DANGEROUS_FOREST, MapId.LITH_HARBOUR],[MapId.FROM_RIEN_TO_LITH, MapId.FROM_LITH_TO_RIEN], 60) { ArrivePortals = [0, 3]},
                // 天空之城 - 武陵
                new PrivateContiMove( "Crane", [200000141, 250000100],[200090300, 200090310], 60),

                new SoloQuestEventTemplate(QuestId.Get3rdJobQuest(Job.WARRIOR), 20 * 60, 108010300, 105070001, 108010300, 108010301),
                new SoloQuestEventTemplate(QuestId.Get3rdJobQuest(Job.MAGICIAN), 20 * 60, 108010200, 100040106, 108010200, 108010201),
                new SoloQuestEventTemplate(QuestId.Get3rdJobQuest(Job.BOWMAN), 20 * 60, 108010100, 105040305, 108010100, 108010101),
                new SoloQuestEventTemplate(QuestId.Get3rdJobQuest(Job.THIEF), 20 * 60, 108010400, 107000402, 108010400, 108010401),
                new SoloQuestEventTemplate(QuestId.Get3rdJobQuest(Job.PIRATE), 20 * 60, 108010500, 105070200, 108010500, 108010501),

                new SoloQuestEventTemplate(QuestId.Get2ndJobQuest(Job.WARRIOR), 20 * 60, 108000300, 102020300, 108000300, 108000300) { MaxLobbys = 3, ExitPortal = 9 },
                new SoloQuestEventTemplate(QuestId.Get2ndJobQuest(Job.MAGICIAN), 20 * 60, 108000200, 101020000, 108000200, 108000200) { MaxLobbys = 3, ExitPortal = 9 },
                new SoloQuestEventTemplate(QuestId.Get2ndJobQuest(Job.BOWMAN), 20 * 60, 108000100, 106010000, 108000100, 108000100) { MaxLobbys = 3, ExitPortal = 9 },
                new SoloQuestEventTemplate(QuestId.Get2ndJobQuest(Job.THIEF), 20 * 60, 108000400, 102040000, 108000400, 108000400) { MaxLobbys = 3, ExitPortal = 9 },
                new SoloQuestEventTemplate(2191, 20 * 60, 108000502, 120000101, 108000502, 108000502) { MaxLobbys = 2 },
                new SoloQuestEventTemplate(2192, 20 * 60, 108000501, 120000101, 108000501, 108000501) { MaxLobbys = 2 },
                new SoloQuestEventTemplate(3230, 10 * 60, 922000010,221024400,922000010,922000010 ),
                new SoloQuestEventTemplate(21301, 10 * 60, 108010700,140020200,108010700,108010700 ){EntryPortal = 1},

                new SoloQuestEventTemplate(6108, 30 * 60, 910500000,105090200,910500000,910500000 ){EntryPortal = 1},
                new SoloQuestEventTemplate(20718, 10 * 60, 910110000,101000000,910110000,910110000){EntryPortal = 1, ExitPortal = 26},
                new q21401(),
                new q21610(),
                new q21613(),
                new SoloQuestEventTemplate(21733, 10 * 60, 910400000,104000004,910400000,910400000),
                new SoloQuestEventTemplate(21739, 10 * 60, 920030000,200060000,920030000,920030001) { EntryPortal = 2 },
                new q21747(),
                new q2245(),
                new SoloQuestEventTemplate(2291, 30 * 60, 103040440,103040400,103040410,103040460),
                new SoloQuestEventTemplate(3239, 20 * 60, 922000000,922000009,922000000,922000000),
                new q6002(),
                new q6330(),
                new q6370(),

                new RockSpirit(),
                new Puppeteer(),
                new MK_PrimeMinister(),
                ];

        }
        public async ValueTask DisposeAsync()
        {
            if (_node != null)
            {
                foreach (var channel in _node.Servers.Values.OfType<WorldChannel>().ToArray())
                {
                    Log.Logger.Information("[{ServerName}] 卸载中...", channel.InstanceName);
                    await channel.EventScriptManager.ClearEvents(_events.Select(x => x.Name).ToHashSet());

                    var mapManager = channel.getMapFactory();
                    var mapBoss = _mapBoss.Values.Select(x => (x.MapId, x.Name)).ToArray();
                    foreach (var (mapId, name) in mapBoss)
                    {
                        if (mapManager.TryGetMap(mapId, out var map))
                        {
                            map?.ClearAreaBoss(name);
                        }
                    }
                    Log.Logger.Information("[{ServerName}] 卸载事件...完成", channel.InstanceName);
                }
                _events.Clear();
                _mapBoss.Clear();
            }
        }

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

        public Task OnMounted(WorldChannelServer node)
        {
            _node = node;
            foreach (var channel in node.Servers.Values.OfType<WorldChannel>().ToArray())
            {
                var oldCount = channel.EventScriptManager.EventCount;
                Log.Logger.Information("[{ServerName}] 加载事件...", channel.InstanceName);
                var count = channel.EventScriptManager.ReloadEventScript(_events);
                Log.Logger.Information("[{ServerName}] 加载事件...完成（{EventCount}项）", channel.InstanceName, count - oldCount);
            }

            return Task.CompletedTask;
        }
    }
}
