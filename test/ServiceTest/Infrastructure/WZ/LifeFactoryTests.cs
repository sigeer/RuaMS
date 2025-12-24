using Application.Core.Channel.DataProviders;
using Application.Core.Game.Life;
using Application.Core.Game.Life.Monsters;
using Application.Templates.Mob;
using Application.Templates.Providers;
using Application.Templates.XmlWzReader.Provider;
using Newtonsoft.Json;
using server.life;
using System.Globalization;

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
                ContractResolver = new PrivateContractResolver("AttackInfoHolders"),
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

            var mobInfo = new MonsterInformationProvider(null, null, null);
            mobInfo.Register(mobInfo);
        }

        int[] TakeTestMobs()
        {
            return _providerSource.GetProvider<MobProvider>().LoadAll()
                .Select(x => x.TemplateId)
                .OrderBy(x => x)
                .ToArray();
        }

        string ToJson(object? obj)
        {
            return JsonConvert.SerializeObject(obj, options);
        }


        // 有大量不正常数据影响
        //[Test]
        //public void getMonsterTest()
        //{
        //    Assert.Multiple(() =>
        //    {
        //        foreach (var mobId in TakeTestMobs())
        //        {
        //            Monster? oldMonster = null;
        //            Monster? newMonster = null;
        //            try
        //            {
        //                oldMonster = oldProvider.getMonster(mobId);
        //                newMonster = newProvider.getMonster(mobId);
        //            }
        //            catch (Exception ex)
        //            {
        //                Console.WriteLine("MonsterId=" + mobId + ", " + ex.Message);
        //            }


        //            if (oldMonster == null)
        //            {
        //                Assert.That(newMonster, Is.Null, $"Id = {mobId}");
        //            }
        //            else
        //            {
        //                var oldJson = ToJson(oldMonster);
        //                var newJson = ToJson(newMonster);
        //                Assert.That(newJson, Is.EqualTo(oldJson), $"Id = {mobId}");
        //            }
        //        }
        //    });
        //}

        [Test]
        public void MobAttackTest()
        {
            foreach (var mobId in TakeTestMobs())
            {
                MonsterCore? newDataSrc = null;
                try
                {
                    newDataSrc = newProvider.getMonsterStats(mobId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"数据不正常MobId={mobId}, {ex.Message}");
                    continue;
                }
                for (int i = 0; i < 9; i++)
                {
                    var oldData = MobAttackInfoFactory.getMobAttackInfo(mobId, i);
                    var newData = newDataSrc?.AttackInfo?.FirstOrDefault(x => x.Index == i);

                    if (oldData == null)
                    {
                        Assert.That(newData, Is.Null, $"Id = {mobId}, Index={i}");
                    }
                    else
                    {
                        Assert.That(newData, Is.Not.Null, $"Id = {mobId}, Index={i}");
                        Assert.That(newData.MpBurn, Is.EqualTo(oldData.getMpBurn()), $"Id={mobId}, Index={i}");
                        Assert.That(newData.ConMP, Is.EqualTo(oldData.getMpCon()), $"Id={mobId}, Index={i}");
                        Assert.That(newData.DeadlyAttack, Is.EqualTo(oldData.isDeadlyAttack()), $"Id={mobId}, Index={i}");
                        Assert.That(newData.Disease, Is.EqualTo(oldData.getDiseaseSkill()), $"Id={mobId}, Index={i}");
                        Assert.That(newData.Level, Is.EqualTo(oldData.getDiseaseLevel()), $"Id={mobId}, Index={i}");
                    }
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
