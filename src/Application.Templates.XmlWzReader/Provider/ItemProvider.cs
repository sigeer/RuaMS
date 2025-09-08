using Application.Templates.Item.Cash;
using Application.Templates.Item.Consume;
using Application.Templates.Item.Etc;
using Application.Templates.Item.Install;
using Application.Templates.Item.Pet;
using Application.Templates.Providers;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;
using static Application.Templates.Item.Consume.MonsterCardItemTemplate;

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

            str+= ".img.xml";
            return _itemFiles.Where(x => x.EndsWith(str)).FirstOrDefault();
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

        private IEnumerable<AbstractTemplate> LoadPets(string imgPath)
        {
            using var fis = new FileStream(imgPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var xDoc = XDocument.Load(fis).Root!;

            if (!int.TryParse(xDoc.Attribute("name")?.Value?.Substring(0, 7), out var petItemId))
                return [];

            var pEntry = new PetItemTemplate(petItemId);
            foreach (var rootPropNode in xDoc.Elements())
            {
                var rootPropName = rootPropNode.Attribute("name")?.Value;
                if (rootPropName == "info")
                {
                    foreach (var infoPropNode in rootPropNode.Elements())
                    {
                        var infoPropName = infoPropNode.Attribute("name")?.Value;
                        if (infoPropName == "hungry")
                            pEntry.Hungry = infoPropNode.GetIntValue();
                        else
                            SetItemTemplateInfo(pEntry, infoPropName, infoPropNode);
                    }
                }
                else if (rootPropName == "interact")
                {
                    List<PetInterActData> list = [];
                    foreach (var itemNode in rootPropNode.Elements())
                    {
                        if (int.TryParse(itemNode.Attribute("name")?.Value, out var idx))
                        {
                            var item = new PetInterActData() { Id = idx };
                            foreach (var itemPropNode in itemNode.Elements())
                            {
                                var itemPropName = itemPropNode.Attribute("name")?.Value;
                                if (itemPropName == "prob")
                                    item.Prob = itemPropNode.GetIntValue();
                                else if (itemPropName == "inc")
                                    item.Inc = itemPropNode.GetIntValue();
                            }
                            list.Add(item);
                        }
                    }
                    pEntry.InterActs = list.ToArray();
                }
            }

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
                foreach (var sourceNode in itemNode.Elements())
                {
                    if (int.TryParse(sourceNode.Attribute("name")?.Value, out var installId))
                    {
                        var template = new InstallItemTemplate(installId);
                        foreach (var propNode in sourceNode.Elements())
                        {
                            var nodeName = propNode.Attribute("name")?.Value;
                            if (nodeName == "info")
                            {
                                foreach (var infoPropNode in propNode.Elements())
                                {
                                    var infoPropName = infoPropNode.Attribute("name")?.Value;
                                    var infoPropValue = infoPropNode.Attribute("value")?.Value;
                                    if (infoPropName == "recoveryMP")
                                        template.RecoveryMP = Convert.ToInt32(infoPropNode.Attribute("value")?.Value);
                                    else if (infoPropName == "recoveryHP")
                                        template.RecoveryHP = Convert.ToInt32(infoPropNode.Attribute("value")?.Value);
                                    else if (infoPropName == "reqLevel")
                                        template.ReqLevel = Convert.ToInt32(infoPropNode.Attribute("value")?.Value);
                                    else if (infoPropName == "tamingMob")
                                        template.TamingMob = Convert.ToInt32(infoPropNode.Attribute("value")?.Value);
                                    else
                                        SetItemTemplateInfo(template, infoPropName, infoPropNode);
                                }
                            }
                        }
                        InsertItem(template);
                        all.Add(template);
                    }
                }

            }
            return all;
        }

        private IEnumerable<AbstractTemplate> LoadEtc(string imgPath)
        {
            using var fis = new FileStream(imgPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var xDoc = XDocument.Load(fis).Root!;

            List<AbstractTemplate> all = [];
            foreach (var itemNode in xDoc.Elements())
            {
                if (int.TryParse(itemNode.Attribute("name")?.Value, out var itemId))
                {
                    var template = new EtcItemTemplate(itemId);
                    foreach (var propNode in itemNode.Elements())
                    {
                        var nodeName = propNode.Attribute("name")?.Value;
                        if (nodeName == "info")
                        {
                            foreach (var infoPropNode in propNode.Elements())
                            {
                                var infoPropName = infoPropNode.Attribute("name")?.Value;
                                var infoPropValue = infoPropNode.Attribute("value")?.Value;
                                if (infoPropName == "exp")
                                    template.Exp = Convert.ToInt32(infoPropNode.Attribute("value")?.Value);
                                else if (infoPropName == "lv")
                                    template.lv = Convert.ToInt32(infoPropNode.Attribute("value")?.Value);
                                else
                                    SetItemTemplateInfo(template, infoPropName, infoPropNode);
                            }
                        }
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
            if (!int.TryParse(xDoc.GetName()?.Substring(0, 4), out var groupId))
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
        public ConsumeItemTemplate ProcessConsumeItem(int groupId, int itemId, XElement itemNode)
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
                case 200:
                case 201:
                case 202:
                case 205:
                case 221:
                case 226:
                case 236:
                case 245:
                    {
                        var template = new PotionItemTemplate(itemId);
                        PotionItemTemplateGenerated.ApplyProperties(template, itemNode);
                        return template;
                    }
                default:
                    return new ConsumeItemTemplate(itemId);
            }
        }

        private ConsumeItemTemplate ProcessScriptedItem(int itemId, XElement itemNode)
        {
            var template = new ScriptItemTemplate(itemId);
            ScriptItemTemplateGenerated.ApplyProperties(template, itemNode);
            return template;
            //foreach (var propNode in itemNode.Elements())
            //{
            //    var propName = propNode.GetName();
            //    if (propName == "info")
            //    {
            //        foreach (var infoPropNode in propNode.Elements())
            //        {
            //            SetItemTemplateInfo(template, infoPropNode.GetName(), infoPropNode);
            //        }
            //    }
            //    else if (propName == "spec")
            //    {
            //        foreach (var infoPropNode in propNode.Elements())
            //        {
            //            var infoPropName = infoPropNode.GetName();
            //            if (infoPropName == "script")
            //                template.Script = infoPropNode.GetStringValue();
            //            else if (infoPropName == "npc")
            //                template.Npc = infoPropNode.GetIntValue();
            //            else if (infoPropName == "runOnPickup")
            //                template.RunOnPickup = infoPropNode.GetBoolValue();
            //            else
            //                SetItemTemplateSpec(template, infoPropNode.GetName(), infoPropNode);
            //        }
            //    }
            //}

            //return template;
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

        private IEnumerable<AbstractTemplate> IterateCashBundleItem(string imgPath)
        {
            using var fis = new FileStream(imgPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var xDoc = XDocument.Load(fis).Root!;

            List<AbstractTemplate> all = [];
            foreach (var itemNode in xDoc.Elements())
            {
                if (int.TryParse(itemNode.Attribute("name")?.Value, out var itemId))
                {
                    var template = new CashItemTemplate(itemId);
                    foreach (var propNode in itemNode.Elements())
                    {
                        var nodeName = propNode.Attribute("name")?.Value;
                        if (nodeName == "info")
                        {
                            foreach (var infoPropNode in propNode.Elements())
                            {
                                var infoPropName = infoPropNode.Attribute("name")?.Value;
                                SetItemTemplateInfo(template, infoPropName, infoPropNode);
                            }
                        }

                        else if (nodeName == "spec")
                        {
                            foreach (var specPropNode in propNode.Elements())
                            {
                                var specPropName = specPropNode.Attribute("name")?.Value;
                                var specPropValue = specPropNode.Attribute("value")?.Value;

                            }
                        }
                    }
                    InsertItem(template);
                    all.Add(template);
                }
            }
            return all;
        }
    }
}
