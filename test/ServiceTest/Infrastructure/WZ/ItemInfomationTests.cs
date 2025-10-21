using Application.Core.Channel.DataProviders;
using Application.Shared.Constants.Job;
using Application.Templates.Exceptions;
using Application.Templates.Item.Consume;
using Application.Templates.Providers;
using Application.Templates.String;
using Application.Templates.XmlWzReader.Provider;
using client.inventory;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using ServiceTest.TestUtilities;
using System.Globalization;

namespace ServiceTest.Infrastructure.WZ
{
    internal class ItemInfomationTests : WzTestBase
    {
        OldItemInformationProvider oldProvider;
        ItemInformationProvider newProvider;
        JsonSerializerSettings options;

        public ItemInfomationTests()
        {
            options = new JsonSerializerSettings
            {
                ContractResolver = new PrivateContractResolver(),
                Formatting = Formatting.Indented
            };
        }

        protected override void OnProviderRegistering()
        {
            ProviderFactory.ConfigureWith(o =>
            {
                o.RegisterProvider<ItemProvider>(() => new ItemProvider(new Application.Templates.TemplateOptions()));
                o.RegisterProvider<EquipProvider>(() => new EquipProvider(new Application.Templates.TemplateOptions()));

                o.RegisterProvider<StringProvider>(() => new StringProvider(new Application.Templates.TemplateOptions(), CultureInfo.GetCultureInfo("en-US")));
            });
        }

        protected override void OnProviderRegistered()
        {
            oldProvider = new OldItemInformationProvider();
            newProvider = new ItemInformationProvider(
                new Mock<ILogger<DataBootstrap>>().Object,
                null
                );
            newProvider.Register(newProvider);
        }


        int[] TakeRandom(int count = 10)
        {
            return ProviderFactory.GetProvider<StringProvider>().GetSubProvider(StringCategory.Item).LoadAll().Select(x => x.TemplateId)
                .Where(x => x > 1000000) // 脸型、发型等也被算作装备，这里排除
                .GroupBy(x => x / 10000).ToDictionary(x => x.Key, x => x.OrderBy(x => Guid.NewGuid()).Take(count).ToArray()) // 分类 让每种类型的物品都通过测试哪怕实际上并不会调用到方法
                .Values.SelectMany(x => x).ToArray();
        }
        /// <summary>
        /// 仅返回装备
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        int[] TakeEquipRandom(int count = 10) => TakeByTypeRandom(x => x < 200, count);

        int[] TakeByTypeRandom(Func<int, bool> func, int count = 10)
        {
            return ProviderFactory.GetProvider<StringProvider>().GetSubProvider(StringCategory.Item).LoadAll().Select(x => x.TemplateId)
                .Where(x => x > 1000000) // 脸型、发型等也被算作装备，这里排除
                .GroupBy(x => x / 10000).ToDictionary(x => x.Key, x => x.OrderBy(x => Guid.NewGuid()).Take(count).ToArray()) // 分类 让每种类型的物品都通过测试哪怕实际上并不会调用到方法
                .Where(x => func(x.Key)).SelectMany(x => x.Value).ToArray();
        }

        string ToJson(object? obj)
        {
            return JsonConvert.SerializeObject(obj, options);
        }

        [Test]
        public void noCancelMouseTest()
        {
            foreach (var item in TakeRandom())
            {
                Assert.That(newProvider.noCancelMouse(item), Is.EqualTo(oldProvider.noCancelMouse(item)), $"Id = {item}");
            }
        }

        [Test]
        public void getEquipLevelReqTest()
        {
            foreach (var item in TakeEquipRandom())
            {
                Assert.That(newProvider.getEquipLevelReq(item), Is.EqualTo(oldProvider.getEquipLevelReq(item)), $"Id = {item}");
            }
        }

        [Test]
        public void isUntradeableRestrictedTest()
        {
            foreach (var item in TakeRandom())
            {
                Assert.That(newProvider.isUntradeableRestricted(item), Is.EqualTo(oldProvider.isUntradeableRestricted(item)), $"Id = {item}");
            }
        }
        [Test]
        public void isAccountRestricted()
        {
            foreach (var item in TakeRandom())
            {
                Assert.That(newProvider.isAccountRestricted(item), Is.EqualTo(oldProvider.isAccountRestricted(item)), $"Id = {item}");
            }
        }

        [Test]
        public void isDropRestricted()
        {
            foreach (var item in TakeRandom())
            {
                Assert.That(newProvider.isDropRestricted(item), Is.EqualTo(oldProvider.isDropRestricted(item)), $"Id = {item}");
            }
        }

        [Test]
        public void isPickupRestricted()
        {
            foreach (var item in TakeRandom())
            {
                Assert.That(newProvider.isPickupRestricted(item), Is.EqualTo(oldProvider.isPickupRestricted(item)), $"Id = {item}");
            }
        }

        [Test]
        public void isQuestItem()
        {
            foreach (var item in TakeRandom())
            {
                Assert.That(newProvider.isQuestItem(item), Is.EqualTo(oldProvider.isQuestItem(item)), $"Id = {item}");
            }
        }

        [Test]
        public void isPartyQuestItem()
        {
            foreach (var item in TakeRandom())
            {
                Assert.That(newProvider.isPartyQuestItem(item), Is.EqualTo(oldProvider.isPartyQuestItem(item)), $"Id = {item}");
            }
        }

        [Test]
        public void isUntradeableOnEquip()
        {
            foreach (var item in TakeRandom())
            {
                Equip? equip = null;
                try
                {
                    equip = newProvider.getEquipById(item);
                }
                catch (TemplateNotFoundException)
                {
                    Assert.That(oldProvider.isUntradeableOnEquip(item), Is.EqualTo(false), $"Id = {item}");
                }
                if (equip != null)
                    Assert.That(equip.SourceTemplate.EquipTradeBlock, Is.EqualTo(oldProvider.isUntradeableOnEquip(item)), $"Id = {item}");
            }
        }

        [Test]
        public void isKarmaAble()
        {
            foreach (var item in TakeRandom())
            {
                Assert.That(newProvider.isKarmaAble(item), Is.EqualTo(oldProvider.isKarmaAble(item)), $"Id = {item}");
            }
        }

        [Test]
        public void getStateChangeItem()
        {
            foreach (var item in TakeRandom())
            {
                Assert.That(newProvider.getStateChangeItem(item), Is.EqualTo(oldProvider.getStateChangeItem(item)), $"Id = {item}");
            }
        }

        [Test]
        public void isConsumeOnPickup()
        {
            foreach (var item in TakeRandom())
            {
                Assert.That(newProvider.isConsumeOnPickup(item), Is.EqualTo(oldProvider.isConsumeOnPickup(item)), $"Id = {item}");
            }
        }

        [Test]
        public void isCash()
        {
            // 策略不同，旧代码只会对装备类型读取wz.cash。新代码一律读取wz.cash
            foreach (var item in TakeRandom())
            {
                Assert.That(newProvider.isCash(item), Is.EqualTo(oldProvider.isCash(item)), $"Id = {item}");
            }
        }

        [Test]
        public void isUpgradeable()
        {
            foreach (var item in TakeEquipRandom())
            {
                Assert.That(newProvider.getEquipById(item).SourceTemplate.IsUpgradeable(), Is.EqualTo(oldProvider.isUpgradeable(item)), $"Id = {item}");
            }
        }

        [Test]
        public void IsEquipElemental()
        {
            foreach (var item in TakeEquipRandom())
            {
                Assert.That(newProvider.getEquipById(item).IsElemental, Is.EqualTo(oldProvider.getEquipLevel(item, false) > 1), $"Id = {item}");
            }
        }

        [Test]
        public void getEquipLevel()
        {
            foreach (var item in TakeEquipRandom())
            {
                Assert.That(newProvider.getEquipById(item).MaxLevel, Is.EqualTo(oldProvider.getEquipLevel(item, true)), $"Id = {item}");
            }
        }

        [Test]
        public void getQuestConsumablesInfo()
        {
            foreach (var item in TakeRandom())
            {
                Assert.That(ToJson(newProvider.getQuestConsumablesInfo(item)), Is.EqualTo(ToJson(oldProvider.getQuestConsumablesInfo(item))), $"Id = {item}");
            }
        }

        [Test]
        public void getMeso()
        {
            foreach (var item in TakeRandom())
            {
                Assert.That(newProvider.GetMesoBagItemTemplate(item)?.Meso ?? -1, Is.EqualTo(oldProvider.getMeso(item)), $"Id = {item}");
            }
        }

        [Test]
        public void getWholePrice()
        {
            foreach (var item in TakeRandom())
            {
                Assert.That(newProvider.getWholePrice(item), Is.EqualTo(oldProvider.getWholePrice(item)), $"Id = {item}");
            }
        }

        [Test]
        public void getUnitPrice()
        {
            foreach (var item in TakeRandom())
            {
                Assert.That(newProvider.getUnitPrice(item), Is.EqualTo(oldProvider.getUnitPrice(item)), $"Id = {item}");
            }
        }

        [Test]
        public void getPrice()
        {
            foreach (var item in TakeRandom())
            {
                Assert.That(newProvider.getPrice(item, 1), Is.EqualTo(oldProvider.getPrice(item, 1)), $"Id = {item}");
                Assert.That(newProvider.getPrice(item, 10), Is.EqualTo(oldProvider.getPrice(item, 10)), $"Id = {item}");
            }
        }

        [Test]
        public void getReplaceOnExpire()
        {
            foreach (var item in TakeRandom())
            {
                var newData = newProvider.GetReplaceItemTemplate(item);
                var oldData = oldProvider.getReplaceOnExpire(item);
                Assert.That(newData?.ItemId ?? 0, Is.EqualTo(oldData.Id), $"Id = {item}");
                Assert.That(newData?.Message ?? "", Is.EqualTo(oldData.Message), $"Id = {item}");
            }
        }

        [Test]
        public void getScrollReqs()
        {
            foreach (var item in TakeRandom())
            {
                Assert.That(newProvider.getScrollReqs(item), Is.EqualTo(oldProvider.getScrollReqs(item)), $"Id = {item}");
            }
        }

        [Test]
        public void getEquipById()
        {
            foreach (var item in TakeEquipRandom())
            {
                Assert.That(ToJson(newProvider.getEquipById(item)), Is.EqualTo(ToJson(oldProvider.getEquipById(item))), $"Id = {item}");
            }
        }

        [Test]
        public void getSummonMobs()
        {
            foreach (var item in TakeRandom())
            {
                var newData = newProvider.GetSummonMobItemTemplate(item);
                var oldData = oldProvider.getSummonMobs(item);
                if (newData == null)
                    Assert.That(oldData.Length == 0);
                else
                {
                    Assert.That(newData.SummonData.Length, Is.EqualTo(oldData.Length), $"Id = {item}");
                    for (int i = 0; i < oldData.Length; i++)
                    {
                        Assert.That(newData.SummonData, Has.Some.Matches<SummonData>(x => x.Prob == oldData[i][1] && x.Mob == oldData[i][0]), $"Id = {item}");
                    }
                }

            }
        }


        [Test]
        public void getScriptedItemInfo()
        {
            foreach (var item in TakeRandom())
            {
                var newData = newProvider.GetScriptItemTemplate(item);
                var oldData = oldProvider.getScriptedItemInfo(item);

                Assert.That(newData?.Script, Is.EqualTo(oldData?.getScript()), $"Id = {item}");
                Assert.That(newData?.Npc, Is.EqualTo(oldData?.getNpc()), $"Id = {item}");
                Assert.That(newData?.RunOnPickup, Is.EqualTo(oldData?.runOnPickup()), $"Id = {item}");
            }
        }

        [Test]
        public void GetCatchMobItemTemplate()
        {
            foreach (var item in TakeByTypeRandom(x => x == 227))
            {
                var newData = newProvider.GetCatchMobItemTemplate(item);

                Assert.That(newData?.UseDelay ?? 0, Is.EqualTo(oldProvider.getUseDelay(item)), $"Id = {item}");
                Assert.That(newData?.Create ?? 0, Is.EqualTo(oldProvider.getCreateItem(item)), $"Id = {item}");
                Assert.That(newData?.MobHP ?? 0, Is.EqualTo(oldProvider.getMobHP(item)), $"Id = {item}");
                Assert.That(newData?.Mob ?? 0, Is.EqualTo(oldProvider.getMobItem(item)), $"Id = {item}");
            }
        }

        [Test]
        public void GetSolomenItemTemplate()
        {
            foreach (var item in TakeRandom())
            {
                var newData = newProvider.GetSolomenItemTemplate(item);

                Assert.That(newData?.MaxLevel ?? 256, Is.EqualTo(oldProvider.getMaxLevelById(item)), $"Id = {item}");
                Assert.That(newData?.Exp ?? 0, Is.EqualTo(oldProvider.getExpById(item)), $"Id = {item}");
            }
        }

        [Test]
        public void getItemReward()
        {
            foreach (var item in TakeRandom())
            {
                Assert.That(ToJson(newProvider.getItemReward(item)), Is.EqualTo(ToJson(oldProvider.getItemReward(item))), $"Id = {item}");
            }
        }

        [Test]
        public void isUnmerchable()
        {
            foreach (var item in TakeRandom())
            {
                Assert.That(newProvider.isUnmerchable(item), Is.EqualTo(oldProvider.isUnmerchable(item)), $"Id = {item}");
            }
        }

        [Test]
        public void GetMasteryItemTemplate()
        {
            foreach (var job in JobFactory.GetAllJob())
            {
                foreach (var item in TakeByTypeRandom(x => x == 228 || x == 229))
                {
                    var newData = newProvider.GetMasteryItemTemplate(item);

                    if (newData != null)
                    {
                        var oldData = oldProvider.getSkillStats(item, job.getId());

                        Assert.That(newData.ReqSkillLevel, Is.EqualTo(oldData.GetValueOrDefault("reqSkillLevel")), $"Id = {item}");
                        Assert.That(newData.MasterLevel, Is.EqualTo(oldData.GetValueOrDefault("masterLevel")), $"Id = {item}");
                        Assert.That(newData.SuccessRate, Is.EqualTo(oldData.GetValueOrDefault("success")), $"Id = {item}");
                        Assert.That(newData.Skills.FirstOrDefault(x => x / 10000 == job.getId()), Is.EqualTo(oldData.GetValueOrDefault("skillid")), $"Id = {item}");
                    }
                }
            }
        }


        //[Test]
        //public void getItemLevelupStats()
        //{
        //    // 内部存在随机
        //    foreach (var item in TaskRandom(_stringProvider.GetEquipTemplates().Select(x => x.TemplateId)))
        //    {
        //        Assert.That(ToJson(newProvider.getItemLevelupStats(item, 1)), Is.EqualTo(ToJson(oldProvider.getItemLevelupStats(item, 1))), $"Id = {item}");
        //        Assert.That(ToJson(newProvider.getItemLevelupStats(item, 5)), Is.EqualTo(ToJson(oldProvider.getItemLevelupStats(item, 5))), $"Id = {item}");
        //        Assert.That(ToJson(newProvider.getItemLevelupStats(item, 10)), Is.EqualTo(ToJson(oldProvider.getItemLevelupStats(item, 10))), $"Id = {item}");
        //    }
        //}

        [Test]
        public void getWatkForProjectile()
        {
            foreach (var item in TakeByTypeRandom(x => x == 207 || x == 233 || x == 206))
            {
                var newData = newProvider.getWatkForProjectile(item);
                var oldData = oldProvider.getWatkForProjectile(item);
                Assert.That(newData, Is.EqualTo(oldData), $"Id = {item}");
            }
        }

        [Test]
        public void getItemEffect()
        {
            // 罗赛伦药水, 新旧代码对specEx/spec处理方式不同
            int[] special = [2022224, 2022225, 2022226, 2022227, 2022228];

            var specialId = 02383033;
            var newSpecialData = newProvider.getItemEffect(specialId);
            var oldSpecialData = oldProvider.getItemEffect(specialId);
            Assert.That(ToJson(newSpecialData), Is.EqualTo(ToJson(oldSpecialData)), $"Id = {specialId}");

            foreach (var item in TakeByTypeRandom(x => x >= 200 && x < 300).Except(special))
            {
                var newData = newProvider.getItemEffect(item);
                var oldData = oldProvider.getItemEffect(item);
                Assert.That(ToJson(newData), Is.EqualTo(ToJson(oldData)), $"Id = {item}");
            }
        }

    }
}
