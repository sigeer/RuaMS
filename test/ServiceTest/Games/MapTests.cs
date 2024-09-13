using Application.Core.Game.Life;
using server.life;
using server.maps;
using tools;

namespace ServiceTest.Games
{
    public class MapTests
    {
        public MapTests()
        {
            Environment.SetEnvironmentVariable("wz-path", "D:\\Cosmic\\wz");
        }
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
    }
}
