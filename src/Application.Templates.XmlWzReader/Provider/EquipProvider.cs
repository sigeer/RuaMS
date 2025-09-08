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
            return _itemFiles.FirstOrDefault(x => x.EndsWith(fileName)) ?? string.Empty;
        }

        protected override IEnumerable<AbstractTemplate> GetDataFromImg(string imgPath)
        {
            if (!File.Exists(imgPath))
                return [];

            using var fis = new FileStream(imgPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var xDoc = XDocument.Load(fis).Root!;

            if (!int.TryParse(xDoc.Attribute("name")?.Value?.Substring(0, 8), out var equipItemId))
                return [];

            var pEntry = new EquipTemplate(equipItemId);
            foreach (var rootPropNode in xDoc.Elements())
            {
                var rootPropName = rootPropNode.Attribute("name")?.Value;
                if (rootPropName == "info")
                {
                    foreach (var infoPropNode in rootPropNode.Elements())
                    {
                        var infoPropName = infoPropNode.Attribute("name")?.Value;
                        if (infoPropName == "reqLevel")
                            pEntry.ReqLevel = infoPropNode.GetIntValue();
                        else if (infoPropName == "reqJob")
                            pEntry.ReqJob = infoPropNode.GetIntValue();
                        else if (infoPropName == "reqSTR")
                            pEntry.ReqSTR = infoPropNode.GetIntValue();
                        else if (infoPropName == "reqDEX")
                            pEntry.ReqDEX = infoPropNode.GetIntValue();
                        else if (infoPropName == "reqINT")
                            pEntry.ReqINT = infoPropNode.GetIntValue();
                        else if (infoPropName == "reqLUK")
                            pEntry.ReqLUK = infoPropNode.GetIntValue();
                        else if (infoPropName == "reqPOP")
                            pEntry.ReqPOP = infoPropNode.GetIntValue();
                        else if (infoPropName == "incSTR")
                            pEntry.IncSTR = infoPropNode.GetIntValue();
                        else if (infoPropName == "incDEX")
                            pEntry.IncDEX = infoPropNode.GetIntValue();
                        else if (infoPropName == "incINT")
                            pEntry.IncINT = infoPropNode.GetIntValue();
                        else if (infoPropName == "incLUK")
                            pEntry.IncLUK = infoPropNode.GetIntValue();
                        else if (infoPropName == "incPAD")
                            pEntry.IncPAD = infoPropNode.GetIntValue();
                        else if (infoPropName == "incPDD")
                            pEntry.IncPDD = infoPropNode.GetIntValue();
                        else if (infoPropName == "incMAD")
                            pEntry.IncMAD = infoPropNode.GetIntValue();
                        else if (infoPropName == "incMDD")
                            pEntry.IncMDD = infoPropNode.GetIntValue();
                        else if (infoPropName == "incJump")
                            pEntry.IncJump = infoPropNode.GetIntValue();
                        else if (infoPropName == "incSpeed")
                            pEntry.IncSpeed = infoPropNode.GetIntValue();
                        else if (infoPropName == "incMHP")
                            pEntry.IncMHP = infoPropNode.GetIntValue();
                        else if (infoPropName == "incMMP")
                            pEntry.IncMMP = infoPropNode.GetIntValue();
                        else if (infoPropName == "tuc")
                            pEntry.TUC = infoPropNode.GetIntValue();
                        else if (infoPropName == "incACC")
                            pEntry.IncACC = infoPropNode.GetIntValue();
                        else if (infoPropName == "incEVA")
                            pEntry.IncEVA = infoPropNode.GetIntValue();
                        else if (infoPropName == "islot")
                            pEntry.Islot = infoPropNode.GetStringValue();
                        else if (infoPropName == "equipTradeBlock")
                            pEntry.EquipTradeBlock = infoPropNode.GetBoolValue();
                        else if (infoPropName == "fs")
                            pEntry.Fs = infoPropNode.GetIntValue();
                        else if (infoPropName == "level")
                        {
                            ProcessEquipLevelData(pEntry, infoPropNode);
                        }
                        else
                            SetItemTemplateInfo(pEntry, infoPropName, infoPropNode);
                    }
                }
            }

            InsertItem(pEntry);
            return [pEntry];
        }

        private static void ProcessEquipLevelData(EquipTemplate pEntry, XElement infoPropNode)
        {
            List<EquipLevelData> list = [];
            foreach (var item in infoPropNode.Elements())
            {
                if (item.GetName() == "info")
                {
                    foreach (var levelData in item.Elements())
                    {
                        if (int.TryParse(levelData.GetName(), out var level))
                        {
                            var model = new EquipLevelData(level);
                            foreach (var levelDataProp in levelData.Elements())
                            {
                                var levelDataPropName = levelDataProp.GetName();
                                if (levelDataPropName == "exp")
                                    model.Exp = levelDataProp.GetIntValue();
                                else
                                {
                                    pEntry.IsElemental = true;
                                    if (levelDataPropName == "incSTRMin")
                                        model.IncSTRMin = levelDataProp.GetIntValue();
                                    else if (levelDataPropName == "incSTRMax")
                                        model.IncSTRMax = levelDataProp.GetIntValue();
                                    else if (levelDataPropName == "incINTMin")
                                        model.IncINTMin = levelDataProp.GetIntValue();
                                    else if (levelDataPropName == "incINTMax")
                                        model.IncINTMax = levelDataProp.GetIntValue();
                                    else if (levelDataPropName == "incDEXMin")
                                        model.IncDEXMin = levelDataProp.GetIntValue();
                                    else if (levelDataPropName == "incDEXMax")
                                        model.IncDEXMax = levelDataProp.GetIntValue();
                                    else if (levelDataPropName == "incLUKMin")
                                        model.IncLUKMin = levelDataProp.GetIntValue();
                                    else if (levelDataPropName == "incLUKMax")
                                        model.IncLUKMax = levelDataProp.GetIntValue();
                                    else if (levelDataPropName == "incPADMin")
                                        model.IncPADMin = levelDataProp.GetIntValue();
                                    else if (levelDataPropName == "incPADMax")
                                        model.IncPADMax = levelDataProp.GetIntValue();
                                    else if (levelDataPropName == "incPDDMin")
                                        model.IncPDDMin = levelDataProp.GetIntValue();
                                    else if (levelDataPropName == "incPDDMax")
                                        model.IncPDDMax = levelDataProp.GetIntValue();
                                    else if (levelDataPropName == "incMADMin")
                                        model.IncMADMin = levelDataProp.GetIntValue();
                                    else if (levelDataPropName == "incMADMax")
                                        model.IncMADMax = levelDataProp.GetIntValue();
                                    else if (levelDataPropName == "incMDDMin")
                                        model.IncMDDMin = levelDataProp.GetIntValue();
                                    else if (levelDataPropName == "incMDDMax")
                                        model.IncMDDMax = levelDataProp.GetIntValue();
                                    else if (levelDataPropName == "incACCMin")
                                        model.IncACCMin = levelDataProp.GetIntValue();
                                    else if (levelDataPropName == "incACCMax")
                                        model.IncACCMax = levelDataProp.GetIntValue();
                                    else if (levelDataPropName == "incEVAMin")
                                        model.IncEVAMin = levelDataProp.GetIntValue();
                                    else if (levelDataPropName == "incEVAMax")
                                        model.IncEVAMax = levelDataProp.GetIntValue();
                                    else if (levelDataPropName == "incSpeedMin")
                                        model.IncSpeedMin = levelDataProp.GetIntValue();
                                    else if (levelDataPropName == "incSpeedMax")
                                        model.IncSpeedMax = levelDataProp.GetIntValue();
                                    else if (levelDataPropName == "incJumpMin")
                                        model.IncJumpMin = levelDataProp.GetIntValue();
                                    else if (levelDataPropName == "incJumpMax")
                                        model.IncJumpMax = levelDataProp.GetIntValue();
                                    else if (levelDataPropName == "incMMPMin")
                                        model.IncMMPMin = levelDataProp.GetIntValue();
                                    else if (levelDataPropName == "incMMPMax")
                                        model.IncMMPMax = levelDataProp.GetIntValue();
                                    else if (levelDataPropName == "incMHPMin")
                                        model.IncMHPMin = levelDataProp.GetIntValue();
                                    else if (levelDataPropName == "incMHPMax")
                                        model.IncMHPMax = levelDataProp.GetIntValue();
                                }
                            }
                            list.Add(model);
                        }

                    }
                    break;
                }
            }
            pEntry.LevelData = list.ToArray();
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
    }
}
