using Application.Templates.Item;
using Application.Templates.Item.Cash;
using Application.Templates.Item.Consume;
using Application.Templates.Item.Etc;
using Application.Templates.Item.Install;
using Application.Templates.Item.Pet;
using Application.Templates.Providers;
using System.Linq;
using System.Xml.Linq;

namespace Application.Templates.XmlWzReader.Provider
{
    public sealed class ItemProvider : ItemProviderBase
    {
        public override ProviderType ProviderName => ProviderType.Item;
        string[] _itemFiles;

        public ItemProvider(TemplateOptions options) : base(options)
        {
            _itemFiles = Directory.GetFiles(GetPath(), "*", SearchOption.AllDirectories);
        }

        protected override IEnumerable<AbstractTemplate> LoadAllInternal()
        {
            List<AbstractTemplate> all = new List<AbstractTemplate>(_itemFiles.Length);
            foreach (var item in _itemFiles)
            {
                all.AddRange(GetDataFromImg(item));
            }
            return all;
        }

        protected override string GetImgPathByTemplateId(int key)
        {
            var str = key.ToString();
            var shortCode = key / 10000;
            if (shortCode != 500)
                str = shortCode.ToString().PadLeft(4, '0');

            str += ".img.xml";
            return _itemFiles.Where(x => x.EndsWith(str)).FirstOrDefault() ?? string.Empty;
        }

        protected override IEnumerable<AbstractTemplate> GetDataFromImg(string path)
        {
            if (path.Contains("Cash"))
                return IterateCashBundleItem(path);
            else if (path.Contains("Consume"))
                return LoadConsume(path);
            else if (path.Contains("Pet"))
                return LoadPets(path);
            else if (path.Contains("Install"))
                return LoadInstall(path);
            else if (path.Contains("Etc"))
                return LoadEtc(path);
            return [];
            //else if (path.Contains("Special"))
            //    LoadSpecial(path);
        }

        public List<MonsterCardItemTemplate> GetAllMonsterCard()
        {
            return LoadConsume(_itemFiles.FirstOrDefault(x => x.EndsWith("0238.img.xml"))!).OfType<MonsterCardItemTemplate>().ToList();
        }

        public List<MasteryItemTemplate> GetAllSkillBook()
        {
            return LoadConsume(_itemFiles.FirstOrDefault(x => x.EndsWith("0228.img.xml") || x.EndsWith("0229.img.xml"))!).OfType<MasteryItemTemplate>().ToList();
        }


        private IEnumerable<AbstractTemplate> LoadPets(string imgPath)
        {
            using var fis = new FileStream(imgPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var xDoc = XDocument.Load(fis).Root!;

            if (!int.TryParse(xDoc.GetName().AsSpan(0, 7), out var petItemId))
                return [];

            var pEntry = new PetItemTemplate(petItemId);
            PetItemTemplateGenerated.ApplyProperties(pEntry, xDoc);
            InsertItem(pEntry);
            return [pEntry];
        }

        private IEnumerable<AbstractTemplate> LoadInstall(string imgPath)
        {
            using var fis = new FileStream(imgPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var xDoc = XDocument.Load(fis).Root!;

            List<AbstractTemplate> all = [];

            foreach (var itemNode in xDoc.Elements())
            {
                if (int.TryParse(itemNode.GetName(), out var installId))
                {
                    var template = new InstallItemTemplate(installId);
                    InstallItemTemplateGenerated.ApplyProperties(template, itemNode);
                    InsertItem(template);
                    all.Add(template);
                }
            }
            return all;
        }

        private IEnumerable<AbstractTemplate> LoadEtc(string imgPath)
        {
            using var fis = new FileStream(imgPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var xDoc = XDocument.Load(fis).Root!;

            List<AbstractItemTemplate> all = [];
            if (!int.TryParse(xDoc.GetName().AsSpan(0, 4), out var groupId))
                return all;

            foreach (var itemNode in xDoc.Elements())
            {
                if (int.TryParse(itemNode.GetName(), out var itemId))
                {
                    AbstractItemTemplate template;
                    if (groupId == 422)
                    {
                        var m = new IncubatorItemTemplate(itemId);
                        IncubatorItemTemplateGenerated.ApplyProperties(m, itemNode);
                        template = m;
                    }
                    else
                    {
                        var m = new EtcItemTemplate(itemId);
                        EtcItemTemplateGenerated.ApplyProperties(m, itemNode);
                        template = m;
                    }
                    InsertItem(template);
                    all.Add(template);
                }
            }
            return all;
        }

        private IEnumerable<AbstractTemplate> LoadConsume(string imgPath)
        {
            using var fis = new FileStream(imgPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var xDoc = XDocument.Load(fis).Root!;

            List<AbstractTemplate> all = [];
            if (!int.TryParse(xDoc.GetName().AsSpan(0, 4), out var groupId))
                return all;

            foreach (var rootNode in xDoc.Elements())
            {
                if (int.TryParse(rootNode.GetName(), out var itemId))
                {
                    var template = ProcessConsumeItem(groupId, itemId, rootNode);
                    InsertItem(template);
                    all.Add(template);
                }
            }
            return all;
        }

        #region Consume
        public ItemTemplateBase ProcessConsumeItem(int groupId, int itemId, XElement itemNode)
        {
            switch (groupId)
            {
                case 203:
                    return ProcessTownScroll(itemId, itemNode);
                case 204:
                    return ProcessScroll(itemId, itemNode);
                case 206:
                case 207:
                case 233:
                    return ProcessBullet(itemId, itemNode);
                case 210:
                    return ProcessSummon(itemId, itemNode);
                case 212:
                    return ProcessPetFood(itemId, itemNode);
                case 227:
                    return ProcessCatchMob(itemId, itemNode);
                case 228:
                case 229:
                    return ProcessSkill(itemId, itemNode);
                case 237:
                    return ProcessSolomen(itemId, itemNode);
                case 238:
                    return ProcessMonsterCard(itemId, itemNode);
                case 243:
                    return ProcessScriptedItem(itemId, itemNode);
                case 221:
                    {
                        var template = new MorphItemTemplate(itemId);
                        MorphItemTemplateGenerated.ApplyProperties(template, itemNode);
                        return template;
                    }
                case 236:
                    {
                        var template = new GhostItemTemplate(itemId);
                        GhostItemTemplateGenerated.ApplyProperties(template, itemNode);
                        return template;
                    }
                case 200:
                case 201:
                case 202:
                case 205:
                case 226:
                case 245:
                    {
                        var template = new PotionItemTemplate(itemId);
                        PotionItemTemplateGenerated.ApplyProperties(template, itemNode);
                        return template;
                    }
                default:
                    {
                        var template = new ConsumeItemTemplate(itemId);
                        ConsumeItemTemplateGenerated.ApplyProperties(template, itemNode);
                        return template;
                    }
            }
        }

        private ConsumeItemTemplate ProcessScriptedItem(int itemId, XElement itemNode)
        {
            var template = new ScriptItemTemplate(itemId);
            ScriptItemTemplateGenerated.ApplyProperties(template, itemNode);
            return template;
        }

        private ConsumeItemTemplate ProcessMonsterCard(int itemId, XElement itemNode)
        {
            var template = new MonsterCardItemTemplate(itemId);
            MonsterCardItemTemplateGenerated.ApplyProperties(template, itemNode);
            return template;
        }

        private ConsumeItemTemplate ProcessPetFood(int itemId, XElement itemNode)
        {
            var template = new PetFoodItemTemplate(itemId);
            PetFoodItemTemplateGenerated.ApplyProperties(template, itemNode);
            return template;
        }

        private ConsumeItemTemplate ProcessTownScroll(int itemId, XElement itemNode)
        {
            var template = new TownScrollItemTemplate(itemId);
            TownScrollItemTemplateGenerated.ApplyProperties(template, itemNode);
            return template;
        }

        private ConsumeItemTemplate ProcessScroll(int itemId, XElement itemNode)
        {
            var template = new ScrollItemTemplate(itemId);
            ScrollItemTemplateGenerated.ApplyProperties(template, itemNode);
            return template;
        }

        private ConsumeItemTemplate ProcessSolomen(int itemId, XElement itemNode)
        {
            var template = new SolomenItemTemplate(itemId);
            SolomenItemTemplateGenerated.ApplyProperties(template, itemNode);
            return template;
        }

        private ConsumeItemTemplate ProcessSkill(int itemId, XElement itemNode)
        {
            var template = new MasteryItemTemplate(itemId);
            MasteryItemTemplateGenerated.ApplyProperties(template, itemNode);
            return template;
        }

        private ConsumeItemTemplate ProcessCatchMob(int itemId, XElement itemNode)
        {
            var template = new CatchMobItemTemplate(itemId);
            CatchMobItemTemplateGenerated.ApplyProperties(template, itemNode);
            return template;
        }

        public ConsumeItemTemplate ProcessBullet(int itemId, XElement itemNode)
        {
            var template = new BulletItemTemplate(itemId);
            BulletItemTemplateGenerated.ApplyProperties(template, itemNode);
            return template;
        }
        public ConsumeItemTemplate ProcessSummon(int itemId, XElement itemNode)
        {
            var template = new SummonMobItemTemplate(itemId);
            SummonMobItemTemplateGenerated.ApplyProperties(template, itemNode);
            return template;
        }
        #endregion

        AbstractItemTemplate ProcessCashItemByGroup(int groupId, int itemId, XElement itemNode)
        {
            switch (groupId)
            {
                case 503:
                    {
                        var template = new HiredMerchantItemTemplate(itemId);
                        HiredMerchantItemTemplateGenerated.ApplyProperties(template, itemNode);
                        return template;
                    }
                case 512:
                    {
                        var template = new MapBuffItemTemplate(itemId);
                        MapBuffItemTemplateGenerated.ApplyProperties(template, itemNode);
                        return template;
                    }
                case 513:
                    {
                        var template = new SafetyCharmItemTemplate(itemId);
                        SafetyCharmItemTemplateGenerated.ApplyProperties(template, itemNode);
                        return template;
                    }
                case 520:
                    {
                        var template = new MesoBagItemTemplate(itemId);
                        MesoBagItemTemplateGenerated.ApplyProperties(template, itemNode);
                        return template;
                    }
                case 521:
                case 536:
                    {
                        var template = new CouponItemTemplate(itemId);
                        CouponItemTemplateGenerated.ApplyProperties(template, itemNode);
                        return template;
                    }
                case 524:
                    {
                        var template = new CashPetFoodItemTemplate(itemId);
                        CashPetFoodItemTemplateGenerated.ApplyProperties(template, itemNode);
                        return template;
                    }
                case 528:
                    {
                        var template = new AreaEffectItemTemplate(itemId);
                        AreaEffectItemTemplateGenerated.ApplyProperties(template, itemNode);
                        return template;
                    }
                case 530:
                    {
                        var template = new MorphItemTemplate(itemId);
                        MorphItemTemplateGenerated.ApplyProperties(template, itemNode);
                        return template;
                    }
                case 550:
                    {
                        var template = new ExtendItemTimeItemTemplate(itemId);
                        ExtendItemTimeItemTemplateGenerated.ApplyProperties(template, itemNode);
                        return template;
                    }
                case 553:
                    {
                        var template = new CashPackagedItemTemplate(itemId);
                        CashPackagedItemTemplateGenerated.ApplyProperties(template, itemNode);
                        return template;
                    }
                default:
                    {
                        var template = new CashItemTemplate(itemId);
                        CashItemTemplateGenerated.ApplyProperties(template, itemNode);
                        return template;
                    }
            }
        }

        private IEnumerable<AbstractTemplate> IterateCashBundleItem(string imgPath)
        {
            using var fis = new FileStream(imgPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var xDoc = XDocument.Load(fis).Root!;

            List<AbstractTemplate> all = [];
            if (!int.TryParse(xDoc.GetName().AsSpan(0, 4), out var groupId))
                return all;

            foreach (var rootNode in xDoc.Elements())
            {
                if (int.TryParse(rootNode.GetName(), out var itemId))
                {
                    var template = ProcessCashItemByGroup(groupId, itemId, rootNode);
                    InsertItem(template);
                    all.Add(template);
                }
            }
            return all;
        }
    }
}
