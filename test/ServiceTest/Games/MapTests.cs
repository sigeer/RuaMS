using Application.Core.Game.Life;
using Application.Core.Game.Maps.Specials;
using server.life;
using server.maps;
using System.Diagnostics;
using tools;

namespace ServiceTest.Games
{
    public class MapTests : TestBase
    {
        [Test]
        [TestCase(10000, ExpectedResult = 3)]
        public int SpawnNpc_Test(int mapId)
        {
            var map = MapFactory.loadMapFromWz(mapId, 0, 0, null);
            var objects = map.getMapObjects();
            foreach (var obj in objects)
            {
                if (obj is NPC npc)
                    Console.WriteLine(PacketCreator.spawnNPC(npc).ToString());
            }
            return objects.Count;
        }

        [Test]
        public void NPC_Test()
        {
            var npcId = 2000;
            var npc = LifeFactory.getNPC(npcId);
            Assert.Pass();
        }

        [Test]
        [TestCase(107000300)]
        [TestCase(10000)]
        public void LoadMapFromWZ_Test(int mapId)
        {
            Assert.DoesNotThrow(() => MapFactory.loadMapFromWz(mapId, 0, 0, null));
        }

        [Test]
        public void MonsterCarnivalMap_Test()
        {
            List<int> cpqMaps =  [980000101,
                           980000201,
                           980000301,
                           980000401,
                           980000501,
                           980000601,
                           980031100,
                           980032100,
                           980033100];

            foreach (var mapId in cpqMaps)
            {
                Assert.That(MapFactory.loadMapFromWz(mapId, 0, 0, null) is ICPQMap);
            }
        }

        [Test]
        public void ChangeMap_Test()
        {
            var curMapId = 103000201;
            var nextMapId = 103000202;

            var mapManager = MockClient.OnlinedCharacter.getChannelServer().getMapFactory();
            var curMap = mapManager.getMap(curMapId);
            var nextMap = mapManager.getMap(nextMapId);
            Stopwatch sw = new();

            sw.Start();
            MockClient.OnlinedCharacter.changeMap(curMap, curMap.getPortal(0));
            sw.Stop();
            Console.WriteLine($"changeMap1. {sw.Elapsed.TotalSeconds}");

            sw.Restart();
            MockClient.OnlinedCharacter.changeMap(nextMap, nextMap.getPortal(0));
            sw.Stop();
            Console.WriteLine($"changeMap2. {sw.Elapsed.TotalSeconds}");

            sw.Restart();
            MockClient.OnlinedCharacter.changeMap(curMap, curMap.getPortal(0));
            sw.Stop();
            Console.WriteLine($"changeMap3. {sw.Elapsed.TotalSeconds}");

            Assert.Pass();
        }
    }
}
