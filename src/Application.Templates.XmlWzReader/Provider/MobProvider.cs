using Application.Templates.Exceptions;
using Application.Templates.Mob;
using Application.Templates.Providers;
using Microsoft.Extensions.Logging;
using System.Xml;
using System.Xml.Linq;
using static Application.Templates.Mob.MobTemplate;

namespace Application.Templates.XmlWzReader.Provider
{
    public class MobProvider : AbstractGroupProvider<MobTemplate>
    {
        public override string ProviderName => ProviderNames.Mob;

        public MobProvider(ProviderOption options)
            : base(options)
        {
            _files = Directory.GetFiles(Path.Combine(GetBaseDir(), ProviderName), "*.xml")
                .Select(x => Path.GetRelativePath(GetBaseDir(), x))
                .ToArray();
        }

        protected override string? GetImgPathByTemplateId(int mobId)
        {
            string fileName = mobId.ToString().PadLeft(7, '0') + ".img.xml";
            return Path.Combine(GetBaseDir(), ProviderName, fileName);
        }

        protected override IEnumerable<AbstractTemplate> GetDataFromImg(string? imgPath)
        {
            try
            {
                using var fis = _fileProvider.ReadFile(imgPath);
                using var reader = XmlReader.Create(fis, XmlReaderUtils.ReaderSettings);
                var xDoc = XDocument.Load(reader).Root!;

                if (!int.TryParse(xDoc.GetName().AsSpan(0, 7), out var mobId))
                    throw new TemplateFormatException(ProviderName, imgPath);

                var pEntry = new MobTemplate(mobId);
                var attackList = new List<MobAttackTemplate>();
                foreach (var rootItem in xDoc.Elements())
                {
                    var itemName = rootItem.GetName();
                    if (itemName == "info")
                    {
                        foreach (var infoProp in rootItem.Elements())
                        {
                            var infoPropName = infoProp.GetName();
                            if (infoPropName == "level")
                                pEntry.Level = infoProp.GetIntValue();
                            else if (infoPropName == "maxHP")
                                pEntry.MaxHP = infoProp.GetIntValue();
                            else if (infoPropName == "maxMP")
                                pEntry.MaxMP = infoProp.GetIntValue();
                            else if (infoPropName == "hpRecovery")
                                pEntry.HpRecovery = infoProp.GetIntValue();
                            else if (infoPropName == "mpRecovery")
                                pEntry.MpRecovery = infoProp.GetIntValue();

                            else if (infoPropName == "PADamage")
                                pEntry.PAD = infoProp.GetIntValue();
                            else if (infoPropName == "PDDamage")
                                pEntry.PDD = infoProp.GetIntValue();
                            else if (infoPropName == "MADamage")
                                pEntry.MAD = infoProp.GetIntValue();
                            else if (infoPropName == "MDDamage")
                                pEntry.MDD = infoProp.GetIntValue();
                            else if (infoPropName == "MDRate")
                                pEntry.MDR = infoProp.GetIntValue();
                            else if (infoPropName == "PDRate")
                                pEntry.PDR = infoProp.GetIntValue();
                            else if (infoPropName == "eva")
                                pEntry.EVA = infoProp.GetIntValue();
                            else if (infoPropName == "acc")
                                pEntry.ACC = infoProp.GetIntValue();

                            else if (infoPropName == "exp")
                                pEntry.Exp = infoProp.GetIntValue();
                            else if (infoPropName == "boss")
                                pEntry.Boss = infoProp.GetIntValue() > 0;
                            else if (infoPropName == "buff")
                                pEntry.Buff = infoProp.GetIntValue();
                            else if (infoPropName == "removeAfter")
                                pEntry.RemoveAfter = infoProp.GetIntValue();
                            else if (infoPropName == "hpTagBgcolor")
                                pEntry.HpTagBgColor = infoProp.GetIntValue();
                            else if (infoPropName == "hpTagColor")
                                pEntry.HpTagColor = infoProp.GetIntValue();
                            else if (infoPropName == "noFlip")
                                pEntry.NoFlip = infoProp.GetBoolValue();

                            else if (infoPropName == "damagedByMob")
                                pEntry.DamagedByMob = infoProp.GetBoolValue();
                            else if (infoPropName == "explosiveReward")
                                pEntry.ExplosiveReward = infoProp.GetBoolValue();
                            else if (infoPropName == "publicReward")
                                pEntry.PublicReward = infoProp.GetIntValue();
                            else if (infoPropName == "undead")
                                pEntry.UnDead = infoProp.GetBoolValue();
                            else if (infoPropName == "getCP")
                                pEntry.GetCP = infoProp.GetIntValue();
                            else if (infoPropName == "removeOnMiss")
                                pEntry.RemoveOnMiss = infoProp.GetBoolValue();

                            else if (infoPropName == "ban")
                            {
                                var model = new MobBanTemplate();
                                foreach (var banProp in infoProp.Elements())
                                {
                                    var banPropName = banProp.GetName();
                                    if (banPropName == "banMsg")
                                        model.Message = banProp.GetStringValue();
                                    else if (banPropName == "banMap")
                                    {
                                        var firstData = banProp.Elements().FirstOrDefault();
                                        if (firstData != null)
                                        {
                                            foreach (var banMapItem in firstData.Elements())
                                            {
                                                var banMapItemName = banMapItem.GetName();
                                                if (banMapItemName == "field")
                                                    model.Map = banMapItem.GetIntValue();
                                                else if (banMapItemName == "portal")
                                                    model.PortalName = banMapItem.GetStringValue();
                                            }
                                        }
                                    }

                                }
                                pEntry.Ban = model;
                            }

                            else if (infoPropName == "link")
                            {
                                pEntry.Link = infoProp.GetIntValue();
                                GetItem(pEntry.Link)?.CloneLink(pEntry);
                            }

                            else if (infoPropName == "selfDestruction")
                            {
                                foreach (var sdProp in infoProp.Elements())
                                {
                                    var sdPropName = sdProp.GetName();
                                    if (sdPropName == "action")
                                        pEntry.SelfDestructActionType = sdProp.GetIntValue();
                                    else if (sdPropName == "removeAfter")
                                        pEntry.SelfDestructRemoveAfter = sdProp.GetIntValue();
                                    else if (sdPropName == "hp")
                                        pEntry.SelfDestructHp = sdProp.GetIntValue();
                                }
                            }

                            else if (infoPropName == "coolDamage")
                                pEntry.CoolDamage = infoProp.GetIntValue();
                            else if (infoPropName == "coolDamageProb")
                                pEntry.CoolDamageProb = infoProp.GetIntValue();

                            else if (infoPropName == "elemAttr")
                                pEntry.ElementStr = infoProp.GetStringValue();
                            else if (infoPropName == "firstAttack")
                                pEntry.IsFirstAttack = infoProp.GetBoolValue();
                            else if (infoPropName == "dropItemPeriod")
                                pEntry.DropItemPeriod = infoProp.GetIntValue();

                            else if (infoPropName == "loseItem")
                            {
                                var list = new List<MobLosedItem>();
                                foreach (var loseItem in infoProp.Elements())
                                {
                                    var model = new MobLosedItem();
                                    foreach (var loseProp in loseItem.Elements())
                                    {
                                        var n = loseProp.GetName();
                                        if (n == "id")
                                            model.Id = loseProp.GetIntValue();
                                        if (n == "prob")
                                            model.Prob = loseProp.GetIntValue();
                                        if (n == "x")
                                            model.X = loseProp.GetIntValue();
                                    }
                                    list.Add(model);
                                }
                                pEntry.LosedItems = list.ToArray();
                            }

                            else if (infoPropName == "revive")
                            {
                                var list = new List<int>();
                                foreach (var reviveItem in infoProp.Elements())
                                {
                                    list.Add(infoProp.GetIntValue());
                                }
                                pEntry.Revive = list.ToArray();
                            }

                            else if (infoPropName == "skill")
                            {
                                var list = new List<MobDataSkillTemplate>();
                                foreach (var skillItem in infoProp.Elements())
                                {
                                    var model = new MobDataSkillTemplate();
                                    foreach (var skillProp in skillItem.Elements())
                                    {
                                        var n = skillProp.GetName();
                                        if (n == "action")
                                            model.Action = skillProp.GetIntValue();
                                        else if (n == "effectAfter")
                                            model.EffectAfter = skillProp.GetIntValue();
                                        else if (n == "skill")
                                            model.Skill = skillProp.GetIntValue();
                                        else if (n == "level")
                                            model.Level = skillProp.GetIntValue();
                                    }
                                    list.Add(model);
                                }
                                pEntry.Skill = list.ToArray();
                            }
                        }
                    }
                    else if (itemName != null)
                    {
                        if (itemName.StartsWith("attack"))
                        {
                            var attack = ProcessAttackInfo(rootItem, itemName);
                            if (attack != null)
                            {
                                attackList.Add(attack);
                            }
                        }
                        else if (itemName == "stand")
                        {
                            var item0 = rootItem.Elements().FirstOrDefault(x => x.GetName() == "0");
                            if (item0 != null)
                            {
                                var originItem = item0.Elements().FirstOrDefault(x => x.GetName() == "origin");
                                if (originItem != null)
                                {
                                    pEntry.Stand0OriginX = originItem.GetIntValue("x");
                                }
                            }
                        }
                        int delay = 0;
                        foreach (var subItem in rootItem.Elements())
                        {
                            foreach (var subItemProp in subItem.Elements())
                            {
                                if (subItemProp.GetName() == "delay")
                                {
                                    delay += subItemProp.GetIntValue();
                                    break;
                                }
                            }
                        }
                        pEntry.AnimateDelay[itemName] = delay;
                    }
                }
                pEntry.AttackInfos = attackList.OrderBy(x => x.Index).ToArray();
                InsertItem(pEntry);
                return [pEntry];
            }
            catch (Exception ex)
            {
                LibLog.Logger.LogError(ex.ToString());
                return [];
            }
        }

        private static MobAttackTemplate? ProcessAttackInfo(XElement infoProp, string infoPropName)
        {
            if (int.TryParse(infoPropName.AsSpan(6), out var idx))
            {
                var model = new MobAttackTemplate(idx - 1);
                var mobAniList = new List<MobAttackAnimationTemplate>();
                foreach (var attackItem in infoProp.Elements())
                {
                    if (attackItem.GetName() == "info")
                    {
                        foreach (var attackInfoProp in attackItem.Elements())
                        {
                            var attackInfoPropName = attackInfoProp.GetName();
                            if (attackInfoPropName == "conMP")
                                model.ConMP = attackInfoProp.GetIntValue();
                            else if (attackInfoPropName == "attackAfter")
                                model.AttackAfter = attackInfoProp.GetIntValue();
                            else if (attackInfoPropName == "PADamage")
                                model.PADamage = attackInfoProp.GetIntValue();
                            else if (attackInfoPropName == "deadlyAttack")
                                model.DeadlyAttack = attackInfoProp.GetBoolValue();
                            else if (attackInfoPropName == "mpBurn")
                                model.MpBurn = attackInfoProp.GetIntValue();
                            else if (attackInfoPropName == "disease")
                                model.Disease = attackInfoProp.GetIntValue();
                            else if (attackInfoPropName == "level")
                                model.Level = attackInfoProp.GetIntValue();
                        }
                    }
                    else if (int.TryParse(attackItem.GetName(), out var innerIdx))
                    {
                        var ani = new MobAttackAnimationTemplate(innerIdx);
                        foreach (var itemProp in attackItem.Elements())
                        {
                            if (itemProp.GetName() == "delay")
                                ani.Delay = itemProp.GetIntValue();
                        }
                        mobAniList.Add(ani);
                    }
                }
                model.Animations = mobAniList.ToArray();
                return model;
            }
            return null;
        }
    }
}