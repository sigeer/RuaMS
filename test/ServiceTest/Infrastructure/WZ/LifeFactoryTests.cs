using Application.Core.Channel.DataProviders;
using Application.Templates.Reader.Img.Provider;
using Application.Templates.Mob;
using Application.Templates.Npc;
using Application.Templates.Reader;
using Application.Templates.String;
using Grpc.Net.Client.Balancer;
using Newtonsoft.Json;
using server.life;
using System.Globalization;

namespace ServiceTest.Infrastructure.WZ
{
    [TestFixture("Duey")]
    internal class LifeFactoryTests : WzTestBase
    {
        OldLifeFactory oldProvider;
        LifeFactory newProvider;
        JsonSerializerSettings options;

        public LifeFactoryTests(string readerType) : base(readerType)
        {
            options = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };
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
            return _providerSource.GetProvider(ProviderType.Mob).LoadAll()
                .Select(x => x.TemplateId)
                .OrderBy(x => x)
                .ToArray();
        }

        string ToJson(object? obj)
        {
            return JsonConvert.SerializeObject(obj, options);
        }


        [Test]
        public void MobAttackTest()
        {
            foreach (var mobId in TakeTestMobs())
            {
                MobTemplate? newDataSrc = null;
                try
                {
                    newDataSrc = newProvider.GetMonsterTrust(mobId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"数据不正常MobId={mobId}, {ex.Message}");
                    continue;
                }
                for (int i = 0; i < 9; i++)
                {
                    var oldData = MobAttackInfoFactory.getMobAttackInfo(mobId, i);
                    var newData = newDataSrc?.AttackInfos.FirstOrDefault(x => x.Index == i);

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

        // xml 与 img 里不一样
        //[Test]
        //public void GetHpBarBossesTest()
        //{
        //    var newData = _providerSource.GetProvider(ProviderType.UIMobWithBossHpBar).LoadAll().Select(x => x.TemplateId).OrderBy(x => x).ToHashSet();
        //    var oldData = OldLifeFactory.getHpBarBosses().OrderBy(x => x).ToHashSet();
        //    Assert.That(newData, Is.EqualTo(oldData));
        //}


        // [Test]
        public void FindAllScriptedNpc()
        {
            var allNpcStr = _providerSource.GetProviderByKey<StringProvider>("zh-CN").GetSubProvider(StringCategory.Npc).LoadAll().OfType<StringNpcTemplate>();

            // 用于推测是不是Npc.img中的几个字段来触发NpcTalkHandler
            var allNpc = _providerSource.GetProvider(ProviderType.Npc).LoadAll().OfType<NpcTemplate>();

            var hasScript = allNpc
                .Where(x => x.Script != null || x.MapleTV || x.TrunkGet != null || x.TrunkPut != null).ToArray();

            var exsitedNpcScripts = Directory.GetFiles(@"scripts\npc")
                .Select(x => Path.GetFileNameWithoutExtension(x));

            foreach (var item in allNpcStr)
            {
                Console.WriteLine(
                    $"NpcId  {item.TemplateId:D7}, Name: {item.Name,-20}\t, Func: {item.Func,-20}\t, 有脚本属性: {hasScript.Any(x => x.TemplateId == item.TemplateId)}, 有脚本：{exsitedNpcScripts.Contains(item.TemplateId.ToString())}");
            }

            //    NpcId  1012118, Name: 炎海                  	, Func: 弓手修炼场助手             	, 有脚本属性: False, 有脚本：True
            //    NpcId  0002100, Name: 莎丽                  	, Func:                     	    , 有脚本属性: False, 有脚本：True
        }
    }
}