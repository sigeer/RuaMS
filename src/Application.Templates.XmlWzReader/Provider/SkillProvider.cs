using Application.Templates.Providers;
using Application.Templates.Skill;
using System.Xml.Linq;

namespace Application.Templates.XmlWzReader.Provider
{
    public class SkillProvider : AbstractProvider<SkillTemplate>
    {
        public override string ProviderName => ProviderNames.Skill;
        public SkillProvider(TemplateOptions options): base(options)
        {
        }

        protected override string? GetImgPathByTemplateId(int key)
        {
            var jobId = key / 10000;
            var imgName = (jobId == 0 ? "000" : jobId.ToString()) + ".img.xml";
            return Path.Combine(ProviderName, imgName);
        }

        protected override IEnumerable<AbstractTemplate> GetDataFromImg(string? filePath)
        {
            List<AbstractTemplate> imgData = [];
            using var fis = _fileProvider.ReadFile(filePath);
            var xDoc = XDocument.Load(fis).Root!;

            var skillElement = xDoc.Elements().FirstOrDefault(x => x.Attribute("name")?.Value == "skill");
            if (skillElement != null)
            {
                foreach (var skillEle in skillElement.Elements())
                {
                    if (int.TryParse(skillEle.Attribute("name")?.Value, out var skillId))
                    {
                        var pEntry = new SkillTemplate(skillId);
                        foreach (var skillProp in skillEle.Elements())
                        {
                            var propName = skillProp.Attribute("name")?.Value;
                            if (propName == "skillType")
                                pEntry.SkillType = Convert.ToInt32(skillProp.Attribute("value")?.Value);
                            else if (propName == "elemAttr")
                                pEntry.ElemAttr = skillProp.Attribute("value")?.Value;
                            else if (propName == "hit")
                                pEntry.HasHitNode = true;
                            else if (propName == "ball")
                                pEntry.HasBallNode = true;
                            else if (propName == "effect")
                                pEntry.EffectData = ProcessEffectData(skillProp);
                            else if (propName == "level")
                                pEntry.LevelData = ProcessSkillData(skillProp);
                            else if (propName == "action")
                            {
                                foreach (var actionItem in skillProp.Elements())
                                {
                                    var index = actionItem.GetName();
                                    if (index == "0")
                                    {
                                        pEntry.Action0 = actionItem.GetStringValue();
                                        break;
                                    }
                                }
                            }
                            else if (propName == "prepare")
                            {
                                foreach (var prepareProp in skillProp.Elements())
                                {
                                    var preparePropName = prepareProp.GetName();
                                    if (preparePropName == "action")
                                    {
                                        pEntry.PrepareAction = prepareProp.GetStringValue();
                                        break;
                                    }
                                }
                            }


                        }
                        InsertItem(pEntry);
                        imgData.Add(pEntry);
                    }

                }
            }

            return imgData;
        }


        private SkillEffectData[] ProcessEffectData(XElement skillProp)
        {
            List<SkillEffectData> list = [];
            foreach (var item in skillProp.Elements())
            {
                if (int.TryParse(item.Attribute("name")?.Value, out var idx))
                {
                    var data = new SkillEffectData();
                    foreach (var prop in item.Elements())
                    {
                        var propName = prop.Attribute("name")?.Value;
                        if (propName == "delay")
                        {
                            data.Delay = Convert.ToInt32(prop.Attribute("value")?.Value);
                            break;
                        }
                    }
                    list.Add(data);
                }
            }
            return list.ToArray();
        }
        private SkillLevelData[] ProcessSkillData(XElement levelNode)
        {
            List<SkillLevelData> list = new();
            foreach (var item in levelNode.Elements())
            {
                if (int.TryParse(item.Attribute("name")?.Value, out var level))
                {
                    var data = new SkillLevelData() { Level = level };
                    foreach (var prop in item.Elements())
                    {
                        var propName = prop.Attribute("name")?.Value;
                        if (propName == "prop")
                            data.Prop = prop.GetIntValue();
                        else if (propName == "cooltime")
                            data.Cooltime = prop.GetIntValue();
                        else if (propName == "time")
                            data.Time = prop.GetIntValue();

                        else if (propName == "damage")
                            data.Damage = prop.GetIntValue();
                        else if (propName == "hpCon")
                            data.HpCon = prop.GetIntValue();
                        else if (propName == "mpCon")
                            data.MpCon = prop.GetIntValue();
                        else if (propName == "mobCount")
                            data.MobCount = prop.GetIntValue();
                        else if (propName == "attackCount")
                            data.AttackCount = prop.GetIntValue();
                        else if (propName == "bulletCount")
                            data.BulletCount = prop.GetIntValue();
                        else if (propName == "bulletConsume")
                            data.BulletConsume = prop.GetIntValue();
                        else if (propName == "lt")
                            data.LeftTop = prop.GetVectorValue();
                        else if (propName == "rb")
                            data.RightBottom = prop.GetVectorValue();
                        else if (propName == "x")
                            data.X = prop.GetIntValue();
                        else if (propName == "y")
                            data.Y = prop.GetIntValue();

                        else if (propName == "moneyCon")
                            data.MoneyCon = prop.GetIntValue();
                        else if (propName == "itemCon")
                            data.ItemCon = prop.GetIntValue();
                        else if (propName == "itemConNo")
                            data.ItemConNo = prop.GetIntValue();

                        else if (propName == "pdd")
                            data.PDD = prop.GetIntValue();
                        else if (propName == "pad")
                            data.PAD = prop.GetIntValue();
                        else if (propName == "mdd")
                            data.MDD = prop.GetIntValue();
                        else if (propName == "mad")
                            data.MAD = prop.GetIntValue();
                        else if (propName == "acc")
                            data.ACC = prop.GetIntValue();
                        else if (propName == "eva")
                            data.EVA = prop.GetIntValue();
                        else if (propName == "hp")
                            data.HP = prop.GetIntValue();
                        else if (propName == "mp")
                            data.MP = prop.GetIntValue();
                        else if (propName == "morph")
                            data.Morph = prop.GetIntValue();
                        else if (propName == "speed")
                            data.Speed = prop.GetIntValue();
                        else if (propName == "jump")
                            data.Jump = prop.GetIntValue();
                    }
                    list.Add(data);
                }
            }
            return list.ToArray();
        }

    }
}
