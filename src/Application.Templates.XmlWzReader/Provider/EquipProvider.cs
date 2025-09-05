using Application.Templates.Character;
using Application.Templates.Providers;
using System.Xml.Linq;

namespace Application.Templates.XmlWzReader.Provider
{
    public sealed class EquipProvider : ItemProviderBase
    {
        public override ProviderType ProviderName => ProviderType.Equip;
        string[] _itemFiles;
        public EquipProvider(TemplateOptions options) : base(options)
        {
            _itemFiles = Directory.GetFiles(GetPath(), "*", SearchOption.AllDirectories);
        }

        protected override string GetImgPathByTemplateId(int itemId)
        {
            string fileName = itemId.ToString().PadLeft(8, '0') + ".img.xml";
            return _itemFiles.FirstOrDefault(x => x.EndsWith(fileName));
        }

        protected override void GetDataFromImg(string imgPath)
        {
            using var fis = new FileStream(imgPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var xDoc = XDocument.Load(fis).Root!;

            if (!int.TryParse(xDoc.Attribute("name")?.Value?.Substring(0, 8), out var equipItemId))
                return;

            var pEntry = new EquipTemplate(equipItemId);
            foreach (var rootPropNode in xDoc.Elements())
            {
                var rootPropName = rootPropNode.Attribute("name")?.Value;
                if (rootPropName == "info")
                {
                    foreach (var infoPropNode in rootPropNode.Elements())
                    {
                        var infoPropName = infoPropNode.Attribute("name")?.Value;
                        var infoPropValue = infoPropNode.Attribute("value")?.Value;
                        if (infoPropName == "reqLevel")
                            pEntry.ReqLevel = Convert.ToInt32(infoPropValue);
                        else if (infoPropName == "reqJob")
                            pEntry.ReqJob = Convert.ToInt32(infoPropValue);
                        else if (infoPropName == "reqSTR")
                            pEntry.ReqSTR = Convert.ToInt32(infoPropValue);
                        else if (infoPropName == "reqDEX")
                            pEntry.ReqDEX = Convert.ToInt32(infoPropValue);
                        else if (infoPropName == "reqINT")
                            pEntry.ReqINT = Convert.ToInt32(infoPropValue);
                        else if (infoPropName == "reqLUK")
                            pEntry.ReqLUK = Convert.ToInt32(infoPropValue);
                        else if (infoPropName == "incPAD")
                            pEntry.incPAD = Convert.ToInt32(infoPropValue);
                        else if (infoPropName == "incPDD")
                            pEntry.incPDD = Convert.ToInt32(infoPropValue);
                        else if (infoPropName == "incMAD")
                            pEntry.incMAD = Convert.ToInt32(infoPropValue);
                        else if (infoPropName == "incMDD")
                            pEntry.incMDD = Convert.ToInt32(infoPropValue);
                        else if (infoPropName == "incJump")
                            pEntry.incJump = Convert.ToInt32(infoPropValue);
                        else if (infoPropName == "incSpeed")
                            pEntry.incSpeed = Convert.ToInt32(infoPropValue);
                        else
                            SetItemTemplate(pEntry, infoPropName, infoPropValue);
                    }
                }
            }

            InsertItem(pEntry);
        }

        protected override void LoadAllInternal()
        {
            foreach (var item in _itemFiles)
            {
                GetDataFromImg(item);
            }
        }
    }
}
