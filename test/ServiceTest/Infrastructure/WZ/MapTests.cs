using Application.Shared.Constants.Map;
using Application.Templates.Map;
using Application.Templates.Providers;
using Application.Templates.XmlWzReader.Provider;
using Newtonsoft.Json;
using server.maps;
using System.Text;
using XmlWzReader;
using XmlWzReader.wz;

namespace ServiceTest.Infrastructure.WZ
{
    internal class MapTests
    {
        int mapId = 001000000;
        private static string GetMapImg(int mapid)
        {
            string mapName = mapid.ToString().PadLeft(9, '0');
            StringBuilder builder = new StringBuilder("Map/Map");
            int area = mapid / 100000000;
            builder.Append(area);
            builder.Append("/");
            builder.Append(mapName);
            builder.Append(".img");
            mapName = builder.ToString();
            return mapName;
        }


        [Test]
        public void MapTemplateDataCheck()
        {
            var provider = new MapProvider(new Application.Templates.TemplateOptions());
            var cpqMap = provider.GetItem(980000101)!;

            Assert.That(cpqMap.MonsterCarnival.Skills, Does.Contain(1));
            Assert.That(cpqMap.MonsterCarnival.Skills, Does.Contain(7));

            Assert.That(cpqMap.MonsterCarnival.Mobs, Has.Some.Matches<MonsterCarnivalMobData>(p => p.Id == 9300128 && p.SpendCP == 7));
            Assert.That(cpqMap.MonsterCarnival.Guardians, Has.Some.Matches<MonsterCarnivalGuardianData>(p => p.X == -538 && p.Y == -135));

            Assert.That(cpqMap.MonsterCarnival.RewardMapWin, Is.EqualTo(980000103));
            Assert.That(cpqMap.MonsterCarnival.EffectWin, Is.EqualTo("quest/carnival/win"));
            Assert.That(cpqMap.MonsterCarnival.SoundWin, Is.EqualTo("MobCarnival/Win"));
            Assert.That(cpqMap.MonsterCarnival.TimeDefault, Is.EqualTo(610));

        }

        [Test]
        public void ReactorEqualCheck()
        {
            ProviderFactory.Initilaize(o =>
            {
                o.RegisterProvider(new ReactorProvider(new Application.Templates.TemplateOptions()));
            });
            var options = new JsonSerializerSettings
            {
                ContractResolver = new PrivateContractResolver(),
                Formatting = Formatting.Indented
            };

            var specialId = 2001;
            var specialNew = ReactorFactory.getReactor(specialId);
            var specialNewStr = JsonConvert.SerializeObject(specialNew, options);

            var specialOld = OldReactorFactory.getReactor(specialId);
            var specialOldStr = JsonConvert.SerializeObject(specialOld, options);
            Assert.That(specialNewStr, Is.EqualTo(specialOldStr));

            var randomList = ProviderFactory.GetProvider<ReactorProvider>().LoadAll()
                .Select(x => x.TemplateId).OrderBy(x => Guid.NewGuid()).Take(100).ToArray();

            foreach (var reactorId in randomList)
            {
                var newRactor = ReactorFactory.getReactor(reactorId);
                var newRactorStr = JsonConvert.SerializeObject(newRactor, options);

                var oldReactor = OldReactorFactory.getReactor(reactorId);
                var oldactorStr = JsonConvert.SerializeObject(oldReactor, options);
                Assert.That(newRactorStr, Is.EqualTo(oldactorStr), $"ReactorId:{reactorId}");

                var newRactorS = ReactorFactory.getReactor(reactorId);
                var newRactorSStr = JsonConvert.SerializeObject(newRactorS, options);

                var oldReactorS = OldReactorFactory.getReactor(reactorId);
                var oldactorSStr = JsonConvert.SerializeObject(oldReactorS, options);
                Assert.That(newRactorStr, Is.EqualTo(oldactorStr), $"ReactorId:{reactorId}");
            }
        }

        [Test]
        public void MapEqualCheck()
        {
            var oldProvider = DataProviderFactory.getDataProvider(WZFiles.MAP);

            var oldData = oldProvider.getData(GetMapImg(mapId));
            var infoData = oldData.getChildByPath("info")!;
            var newProvider = new MapProvider(new Application.Templates.TemplateOptions());
            var newData = newProvider.GetItem(mapId)!;

            Assert.That(newData.HasClock, Is.EqualTo(oldData.getChildByPath("clock") != null));
            Assert.That(newData.OnUserEnter, Is.EqualTo(DataTool.getString(infoData?.getChildByPath("onUserEnter"))));
            Assert.That(newData.Everlast, Is.EqualTo(DataTool.getIntConvert("everlast", infoData, 0) != 0));
            Assert.That(newData.Town, Is.EqualTo(DataTool.GetBoolean("town", infoData)));
            Assert.That(newData.DecHP, Is.EqualTo(DataTool.getIntConvert("decHP", infoData, 0)));
            Assert.That(newData.ProtectItem, Is.EqualTo(DataTool.getIntConvert("protectItem", infoData, 0)));
            Assert.That(newData.ForcedReturn, Is.EqualTo(DataTool.getInt(infoData?.getChildByPath("forcedReturn"), MapId.NONE)));
            Assert.That(newData.TimeLimit, Is.EqualTo(DataTool.getIntConvert("timeLimit", infoData, -1)));
            Assert.That(newData.FieldType, Is.EqualTo(DataTool.getIntConvert("fieldType", infoData, 0)));
            Assert.That(newData.FixedMobCapacity, Is.EqualTo(DataTool.getIntConvert("fixedMobCapacity", infoData, 500)));
            Assert.That(newData.Reactors, Has.Some.Matches<MapReactorTemplate>(x => x.Id == 2001));
        }
    }
}
