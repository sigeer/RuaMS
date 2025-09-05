using Application.Templates.Item.Cash;
using Application.Templates.Item.Consume;
using Application.Templates.Item.Etc;
using Application.Templates.Item.Install;
using Application.Templates.Item.Pet;
using Application.Templates.Providers;
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

        protected override void LoadAllInternal()
        {
            foreach (var item in _itemFiles)
            {
                GetDataFromImg(item);
            }
        }

        protected override string GetImgPathByTemplateId(int key)
        {
            var str = key.ToString();
            var shortCode = key / 10000;
            if (shortCode <= 0)
            {
                str = str.PadLeft(4, '0');
            }
            str += ".img.xml";
            return _itemFiles.Where(x => x == str).FirstOrDefault();
        }

        protected override void GetDataFromImg(string path)
        {
            if (path.Contains("Cash"))
                IterateCashBundleItem(path);
            else if (path.Contains("Consume"))
                LoadConsume(path);
            else if (path.Contains("Pet"))
                LoadPets(path);
            else if (path.Contains("Install"))
                LoadInstall(path);
            else if (path.Contains("Etc"))
                LoadEtc(path);
            //else if (path.Contains("Special"))
            //    LoadSpecial(path);
        }

        private void LoadPets(string imgPath)
        {
            using var fis = new FileStream(imgPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var xDoc = XDocument.Load(fis).Root!;

            if (!int.TryParse(xDoc.Attribute("name")?.Value?.Substring(0, 7), out var petItemId))
                return;

            var pEntry = new PetItemTemplate(petItemId);
            foreach (var rootPropNode in xDoc.Elements())
            {
                var rootPropName = rootPropNode.Attribute("name")?.Value;
                if (rootPropName == "info")
                {
                    foreach (var infoPropNode in rootPropNode.Elements())
                    {
                        var infoPropName = infoPropNode.Attribute("name")?.Value;
                        var infoPropValue = infoPropNode.Attribute("value")?.Value;
                        if (infoPropName == "hungry")
                            pEntry.Hungry = Convert.ToInt32(infoPropValue);
                        else
                            SetItemTemplate(pEntry, infoPropName, infoPropValue);
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
                                    item.Prob = Convert.ToInt32(itemPropNode.Attribute("value")?.Value);
                                else if (itemPropName == "inc")
                                    item.Inc = Convert.ToInt32(itemPropNode.Attribute("value")?.Value);
                            }
                            list.Add(item);
                        }
                    }
                    pEntry.InterActs = list.ToArray();
                }
            }

            InsertItem(pEntry);
        }

        private void LoadInstall(string imgPath)
        {
            using var fis = new FileStream(imgPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var xDoc = XDocument.Load(fis).Root!;

            foreach (var itemNode in xDoc.Elements())
            {
                if (int.TryParse(itemNode.Attribute("name")?.Value, out var installId))
                {
                    var template = new InstallItemTemplate(installId);
                    foreach (var propNode in itemNode.Elements())
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
                                    SetItemTemplate(template, infoPropName, infoPropValue);
                            }
                        }
                    }
                    InsertItem(template);
                }
            }
        }

        private void LoadEtc(string imgPath)
        {
            using var fis = new FileStream(imgPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var xDoc = XDocument.Load(fis).Root!;

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
                                    SetItemTemplate(template, infoPropName, infoPropValue);
                            }
                        }
                    }
                    InsertItem(template);
                }
            }
        }

        private void LoadConsume(string imgPath)
        {
            using var fis = new FileStream(imgPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var xDoc = XDocument.Load(fis).Root!;

            foreach (var itemNode in xDoc.Elements())
            {
                if (int.TryParse(itemNode.Attribute("name")?.Value, out var itemId))
                {
                    var template = new ConsumeItemTemplate(itemId);
                    foreach (var propNode in itemNode.Elements())
                    {
                        var nodeName = propNode.Attribute("name")?.Value;
                        if (nodeName == "info")
                        {
                            foreach (var infoPropNode in propNode.Elements())
                            {
                                var infoPropName = infoPropNode.Attribute("name")?.Value;
                                var infoPropValue = infoPropNode.Attribute("value")?.Value;
                                SetItemTemplate(template, infoPropName, infoPropValue);
                            }

                        }
                    }
                    InsertItem(template);
                }
            }
        }

        private void IterateCashBundleItem(string imgPath)
        {
            using var fis = new FileStream(imgPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var xDoc = XDocument.Load(fis).Root!;

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
                                var infoPropValue = infoPropNode.Attribute("value")?.Value;
                                SetItemTemplate(template, infoPropName, infoPropValue);
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
                }

            }
        }
    }
}
