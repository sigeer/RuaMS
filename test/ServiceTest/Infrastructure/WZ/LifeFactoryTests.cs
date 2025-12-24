using Application.Core.Channel.DataProviders;
using Application.Templates.Providers;
using Application.Templates.XmlWzReader.Provider;
using Newtonsoft.Json;
using System.Globalization;
using server.life;

namespace ServiceTest.Infrastructure.WZ
{
    internal class LifeFactoryTests : WzTestBase
    {
        OldLifeFactory oldProvider;
        LifeFactory newProvider;
        JsonSerializerSettings options;

        public LifeFactoryTests()
        {
            options = new JsonSerializerSettings
            {
                ContractResolver = new PrivateContractResolver("SourceTemplate", "EffectTemplate"),
                Formatting = Formatting.Indented
            };
        }

        protected override void OnProviderRegistering()
        {
            _providerSource.TryRegisterProvider<MobProvider>(o => new MobProvider(o));
            _providerSource.TryRegisterProvider<NpcProvider>(o => new NpcProvider(o));
            _providerSource.TryRegisterProvider<MobSkillProvider>(o => new MobSkillProvider(o));

            _providerSource.TryRegisterKeydProvider("zh-CN", o => new StringProvider(o, CultureInfo.GetCultureInfo("zh-CN")));
            _providerSource.TryRegisterKeydProvider("en-US", o => new StringProvider(o, CultureInfo.GetCultureInfo("en-US")));
            _providerSource.TryRegisterProvider<MobWithBossHpBarProvider>(o => new MobWithBossHpBarProvider(o));

            ProviderSource.Instance = _providerSource;
        }

        protected override void OnProviderRegistered()
        {
            oldProvider = new OldLifeFactory();
            newProvider = LifeFactory.Instance;

            oldProvider.Register(oldProvider);

            var mobInfo = new MonsterInformationProvider(null, null, null);
            mobInfo.Register(mobInfo);
        }

        int[] TakeTestMobs()
        {
            return _providerSource.GetProviderByKey<StringProvider>("zh-CN").GetSubProvider(Application.Templates.String.StringCategory.Mob).LoadAll()
                .Select(x => x.TemplateId)
                .Where(id => id < 9300000) // exclude some special mobs
                .OrderBy(x => x)
                .ToArray();
        }

        string ToJson(object? obj)
        {
            return JsonConvert.SerializeObject(obj, options);
        }


        [Test]
        public void getMonsterTest()
        {
            foreach (var mobId in TakeTestMobs())
            {
                var oldMonster = oldProvider.getMonster(mobId);
                var newMonster = newProvider.getMonster(mobId);

                if (oldMonster == null)
                {
                    Assert.That(newMonster, Is.Null, $"Id = {mobId}");
                }
                else
                {
                    var oldJson = ToJson(oldMonster);
                    var newJson = ToJson(newMonster);
                    if (oldJson == newJson)
                    {
                        Assert.Pass();
                        return;
                    }
                    else
                    {
                        if (newJson.Contains(oldJson))
                        {
                            Console.WriteLine("================");
                            Console.WriteLine("OldJson");
                            Console.WriteLine(oldJson);
                            Console.WriteLine("NewJson");
                            Console.WriteLine(newJson);
                            Assert.Pass();
                            return;
                        }
                    }
                    Assert.Fail();
                }
            }
        }

        [Test]
        public void GetHpBarBossesTest()
        {
            Assert.That(_providerSource.GetProvider<MobWithBossHpBarProvider>().LoadAll().Select(x => x.TemplateId).OrderBy(x => x).ToHashSet(),
                Is.EqualTo(OldLifeFactory.getHpBarBosses().OrderBy(x => x).ToHashSet()));
        }
    }
}
