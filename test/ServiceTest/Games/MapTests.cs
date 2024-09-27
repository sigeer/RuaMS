using Application.Core.Game.Life;
using Application.Core.Game.Maps.Specials;
using server.life;
using server.maps;
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
        public void MapClone_Test()
        {
            List<int> cpqMaps = [980000101,
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
                var mapModel = MapFactory.loadMapFromWz(mapId, 0, 0, null);
                var cloned = mapModel.Clone();
                Assert.That(mapModel.getStreetName() == cloned.getStreetName());
            }
        }
    }
}
