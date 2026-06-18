using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.Game.Maps.Specials;
using server.maps;
using tools;

namespace ServiceTest.Games.Gameplay
{
    public class MapTests
    {
        private async Task<IMap> LoadMap(int mapId)
        {
            var chr = GameTestGlobal.TestServer.GetPlayer()!;
            var channel1 = chr.getChannelServer();
            return await MapFactory.Instance.loadMapFromWz(mapId, channel1, null);
        }

        [Test]
        [TestCase(10000, ExpectedResult = 3)]
        public async Task<int> SpawnNpc_Test(int mapId)
        {
            var map = await LoadMap(mapId);
            var objects = map.getMapObjects();
            foreach (var obj in objects)
            {
                if (obj is NPC npc)
                    Console.WriteLine(PacketCreator.spawnNPC(npc).ToString());
            }
            return objects.Count;
        }

        //[TestCase(103000201, 2)]
        //[Test]
        //public async Task MonsterRate_Test(int mapId, float rate)
        //{
        //    var mapManager = MockClient.OnlinedCharacter.getChannelServer().getMapFactory();
        //    MockClient.OnlinedCharacter.changeMap(mapId);
        //    var map = mapManager.getMap(mapId);

        //    var originalMonsterCount = map.countMonsters();

        //    var monsters = map.getAllMonsters();
        //    foreach (var monster in monsters)
        //    {
        //        map.damageMonster(MockClient.OnlinedCharacter, monster, int.MaxValue);
        //    }

        //    map.MonsterRate *= rate;

        //    await Task.Delay(6000);
        //    Server.getInstance().forceUpdateCurrentTime();
        //    mapManager.updateMaps();

        //    // 1个人在地图时只会生成0.75的怪物（每多一个人+0.05）
        //    Assert.That(map.countMonsters(), Is.EqualTo(originalMonsterCount * rate * 0.75));
        //}

        [Test]
        public async Task NPC_Test()
        {
            var npcId = 2000;

            var map = await LoadMap(20000);
            var mapNPC = map.getNPCById(npcId);
            Assert.That(mapNPC?.getMap() is not null);
        }

        [Test]
        [TestCase(107000300)]
        [TestCase(10000)]
        public void LoadMapFromWZ_Test(int mapId)
        {
            Assert.DoesNotThrow(async () => await LoadMap(mapId));
        }

        [Test]
        public void MonsterCarnivalMap_Test()
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
                Assert.That(LoadMap(mapId) is ICPQMap);
            }
        }

        [Test]
        public async Task ChangeMap_Test()
        {
            var curMapId = 200082300;
            var nextMapId = 230010000;

            var chr = GameTestGlobal.TestServer.GetPlayer();
            var mapManager = chr.getChannelServer().getMapFactory();

            var curMap = await mapManager.getMap(curMapId);
            var nextMap = await mapManager.getMap(nextMapId);

            await chr.changeMap(curMap);
            await chr.changeMap(nextMap);
            await Task.Delay(1000);

            Assert.Pass();
        }

        [Test]
        public async Task MapItemInfo()
        {
            var mapId = 103000201;

            var chr = GameTestGlobal.TestServer.GetPlayer()!;
            var map = await LoadMap(mapId);

            await chr.changeMap(map, map.getPortal(0));
            chr.setMapTransitionComplete();

            await Task.Delay(2000);

            await map.ProcessMonster(async m =>
            {
                await m.DamageBy(chr, int.MaxValue, 0);
            });

            var items = map.getItems();
            Assert.That(items.Count > 0);

            var mapItem = items[0];
            Assert.That(mapItem.MapModel is not null);
        }

        //[Test]
        //public void PickupItemByAnother()
        //{
        //    var mapId = 103000202;
        //    var mapManager = MockClient.OnlinedCharacter.getChannelServer().getMapFactory();
        //    var map = mapManager.getMap(mapId);

        //    MockClient.OnlinedCharacter.changeMap(map, map.getPortal(0));
        //    var monsters = map.getAllMonsters();
        //    foreach (var monster in monsters)
        //    {
        //        // 直接使用killMonster时并没有触发造成伤害，不会掉落
        //        map.damageMonster(MockClient.OnlinedCharacter, monster, int.MaxValue);
        //    }
        //    var items = map.getItems();
        //    Assert.That(items.Count > 0);

        //    var client2 = GetOnlinedTestClient(2);

        //    client2.OnlinedCharacter.changeMap(map, map.getPortal(0));
        //    var anotherPicker = new PlayerPickupProcessor(client2.OnlinedCharacter);
        //    foreach (var item in items)
        //    {
        //        anotherPicker.Handle(item as MapItem);
        //    }

        //    Assert.That(map.getItems().Count == items.Count);
        //    anotherPicker.Flags = PickupCheckFlags.None;

        //    foreach (var item in items)
        //    {
        //        anotherPicker.Handle(item as MapItem);
        //    }
        //    Assert.That(map.getItems().Count == 0);
        //}

        //[Test]
        //public void MobRateChange_Test()
        //{
        //    var map = LoadMap(10000);

        //    Assert.That(map.ActualMonsterRate, Is.EqualTo(map.getWorldServer().MobRate));
        //    map.getWorldServer().MobRate = 2;
        //    Assert.That(map.ActualMonsterRate, Is.EqualTo(map.getWorldServer().MobRate));
        //    map.MonsterRate = 0.5f;
        //    Assert.That(map.ActualMonsterRate, Is.EqualTo(1));
        //}
    }
}
