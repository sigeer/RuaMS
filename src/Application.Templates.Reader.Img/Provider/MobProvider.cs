using Application.Templates.Exceptions;
using Application.Templates.Mob;
using Application.Templates.Reader;
using Application.Templates.Reader.Resolvers;
using Duey.Abstractions;
using Duey.Provider.WZ.Files;
using Microsoft.Extensions.Logging;
using static Application.Templates.Mob.MobTemplate;

namespace Application.Templates.Reader.Img.Provider
{
    public class MobProvider : AbstractGroupProvider<MobTemplate>
    {
        public override ProviderType Type => ProviderType.Mob;

        public MobProvider(IWzPathResolver resolver) : base(resolver)
        {
        }

        protected override IEnumerable<MobTemplate> GetDataFromImg(string? imgPath)
        {
            try
            {
                var fullPath = _resolver.ResolveFullPath(imgPath);
                var rootNode = new WZImage(fullPath);

                if (!int.TryParse(rootNode.Name.AsSpan(0, 7), out var mobId))
                    throw new TemplateFormatException("Mob.wz", imgPath);

                var pEntry = new MobTemplate(mobId);
                var attackList = new List<MobAttackTemplate>();

                foreach (var rootItem in rootNode.Children)
                {
                    var itemName = rootItem.Name;
                    if (itemName == "info")
                    {
                        foreach (var infoProp in rootItem.Children)
                        {
                            var infoPropName = infoProp.Name;
                            if (infoPropName == "level") pEntry.Level = infoProp.GetIntValue();
                            else if (infoPropName == "maxHP") pEntry.MaxHP = infoProp.GetIntValue();
                            else if (infoPropName == "maxMP") pEntry.MaxMP = infoProp.GetIntValue();
                            else if (infoPropName == "hpRecovery") pEntry.HpRecovery = infoProp.GetIntValue();
                            else if (infoPropName == "mpRecovery") pEntry.MpRecovery = infoProp.GetIntValue();
                            else if (infoPropName == "PADamage") pEntry.PAD = infoProp.GetIntValue();
                            else if (infoPropName == "PDDamage") pEntry.PDD = infoProp.GetIntValue();
                            else if (infoPropName == "MADamage") pEntry.MAD = infoProp.GetIntValue();
                            else if (infoPropName == "MDDamage") pEntry.MDD = infoProp.GetIntValue();
                            else if (infoPropName == "MDRate") pEntry.MDR = infoProp.GetIntValue();
                            else if (infoPropName == "PDRate") pEntry.PDR = infoProp.GetIntValue();
                            else if (infoPropName == "eva") pEntry.EVA = infoProp.GetIntValue();
                            else if (infoPropName == "acc") pEntry.ACC = infoProp.GetIntValue();
                            else if (infoPropName == "exp") pEntry.Exp = infoProp.GetIntValue();
                            else if (infoPropName == "boss") pEntry.Boss = infoProp.ResolveBool() ?? false;
                            else if (infoPropName == "buff") pEntry.Buff = infoProp.GetIntValue(defaultValue: -1);
                            else if (infoPropName == "removeAfter") pEntry.RemoveAfter = infoProp.GetIntValue();
                            else if (infoPropName == "hpTagBgcolor") pEntry.HpTagBgColor = infoProp.GetIntValue();
                            else if (infoPropName == "hpTagColor") pEntry.HpTagColor = infoProp.GetIntValue();
                            else if (infoPropName == "noFlip") pEntry.NoFlip = infoProp.ResolveBool() ?? false;
                            else if (infoPropName == "damagedByMob") pEntry.DamagedByMob = infoProp.ResolveBool() ?? false;
                            else if (infoPropName == "explosiveReward") pEntry.ExplosiveReward = infoProp.ResolveBool() ?? false;
                            else if (infoPropName == "publicReward") pEntry.PublicReward = infoProp.GetIntValue();
                            else if (infoPropName == "undead") pEntry.UnDead = infoProp.ResolveBool() ?? false;
                            else if (infoPropName == "getCP") pEntry.GetCP = infoProp.GetIntValue();
                            else if (infoPropName == "removeOnMiss") pEntry.RemoveOnMiss = infoProp.ResolveBool() ?? false;
                            else if (infoPropName == "firstAttack") pEntry.IsFirstAttack = infoProp.ResolveBool() ?? false;
                            else if (infoPropName == "dropItemPeriod") pEntry.DropItemPeriod = infoProp.GetIntValue();
                            else if (infoPropName == "coolDamage") pEntry.CoolDamage = infoProp.GetIntValue();
                            else if (infoPropName == "coolDamageProb") pEntry.CoolDamageProb = infoProp.GetIntValue();
                            else if (infoPropName == "elemAttr") pEntry.ElementStr = infoProp.GetStringValue();
                            else if (infoPropName == "link")
                            {
                                pEntry.Link = infoProp.GetIntValue();
                                if (pEntry.Link > 0)
                                    GetItem(pEntry.Link)?.CloneLink(pEntry);
                            }
                            else if (infoPropName == "ban")
                            {
                                var model = new MobBanTemplate();
                                foreach (var banProp in infoProp.Children)
                                {
                                    if (banProp.Name == "banMsg")
                                        model.Message = banProp.GetStringValue();
                                    else if (banProp.Name == "banMap")
                                    {
                                        var firstData = banProp.Children.FirstOrDefault();
                                        if (firstData != null)
                                        {
                                            foreach (var banMapItem in firstData.Children)
                                            {
                                                if (banMapItem.Name == "field")
                                                    model.Map = banMapItem.GetIntValue();
                                                else if (banMapItem.Name == "portal")
                                                    model.PortalName = banMapItem.GetStringValue() ?? "sp";
                                            }
                                        }
                                    }
                                }
                                pEntry.Ban = model;
                            }
                            else if (infoPropName == "selfDestruction")
                            {
                                var builder = new MobSelfDestructionBuilder();
                                foreach (var sdProp in infoProp.Children)
                                {
                                    var sdPropName = sdProp.Name;
                                    if (sdPropName == "action")
                                        builder.ActionType = sdProp.GetIntValue();
                                    else if (sdPropName == "removeAfter")
                                        builder.RemoveAfter = sdProp.GetIntValue();
                                    else if (sdPropName == "hp")
                                        builder.Hp = sdProp.GetIntValue();
                                }
                                pEntry.SelfDestruction = builder.Build();
                            }
                            else if (infoPropName == "loseItem")
                            {
                                var list = new List<MobLosedItem>();
                                foreach (var loseItem in infoProp.Children)
                                {
                                    var model = new MobLosedItem();
                                    foreach (var loseProp in loseItem.Children)
                                    {
                                        var n = loseProp.Name;
                                        if (n == "id") model.Id = loseProp.GetIntValue();
                                        if (n == "prop") model.Prop = loseProp.GetIntValue();
                                        if (n == "x") model.X = loseProp.GetIntValue();
                                    }
                                    list.Add(model);
                                }
                                pEntry.LosedItems = [.. list];
                            }
                            else if (infoPropName == "revive")
                            {
                                pEntry.Revive = infoProp.Children.Select(x => x.GetIntValue()).ToArray();
                            }
                            else if (infoPropName == "skill")
                            {
                                var list = new List<MobDataSkillTemplate>();
                                foreach (var skillItem in infoProp.Children)
                                {
                                    if (!int.TryParse(skillItem.Name, out var skillIdx)) continue;
                                    var model = new MobDataSkillTemplate(skillIdx);
                                    foreach (var skillProp in skillItem.Children)
                                    {
                                        var n = skillProp.Name;
                                        if (n == "action") model.Action = skillProp.GetIntValue();
                                        else if (n == "effectAfter") model.EffectAfter = skillProp.GetIntValue();
                                        else if (n == "skill") model.Skill = skillProp.GetIntValue();
                                        else if (n == "level") model.Level = skillProp.GetIntValue();
                                    }
                                    list.Add(model);
                                }
                                pEntry.Skill = [.. list.OrderBy(x => x.Index)];
                            }
                            else if (infoPropName == "speak")
                            {
                                var list = new List<MobSpeakInfoTemplate>();
                                foreach (var item in infoProp.Children)
                                {
                                    if (int.TryParse(item.Name, out var spkIdx))
                                    {
                                        var model = new MobSpeakInfoTemplate(spkIdx);
                                        List<int> msgList = [];
                                        foreach (var modelProp in item.Children)
                                        {
                                            var n = modelProp.Name;
                                            if (n == "hp") model.Hp = modelProp.GetIntValue(defaultValue: int.MaxValue);
                                            else if (n == "prob") model.Prob = modelProp.GetIntValue(defaultValue: 100);
                                            else if (int.TryParse(n, out var msgId))
                                                msgList.Add(msgId);
                                        }
                                        model.Messages = [.. msgList.OrderBy(x => x)];
                                        list.Add(model);
                                    }
                                }
                                pEntry.SpeakInfos = [.. list.OrderBy(x => x.Hp)];
                            }
                        }
                    }
                    else if (itemName != null)
                    {
                        if (itemName.StartsWith("attack"))
                        {
                            var attack = ProcessAttackInfo(rootItem);
                            if (attack != null)
                                attackList.Add(attack);
                        }
                        else if (itemName == "stand")
                        {
                            var item0 = rootItem.Children.FirstOrDefault(x => x.Name == "0");
                            if (item0 != null)
                            {
                                var originItem = item0.Children.FirstOrDefault(x => x.Name == "origin");
                                if (originItem != null)
                                    pEntry.Stand0OriginX = originItem.GetIntValue("x");
                            }
                        }
                        int delay = 0;
                        foreach (var subItem in rootItem.Children)
                            foreach (var subItemProp in subItem.Children)
                                if (subItemProp.Name == "delay")
                                {
                                    delay += subItemProp.GetIntValue();
                                    break;
                                }
                        pEntry.AnimateDelay[itemName] = delay;
                    }
                }
                pEntry.AttackInfos = [.. attackList.OrderBy(x => x.Index)];
                InsertItem(pEntry);
                return [pEntry];
            }
            catch (Exception ex)
            {
                LibLog.Logger.LogError(ex.ToString());
                return [];
            }
        }

        private static MobAttackTemplate? ProcessAttackInfo(IDataNode infoProp)
        {
            var name = infoProp.Name;
            if (name.Length <= 6 || !int.TryParse(name.AsSpan(6), out var idx))
                return null;

            var model = new MobAttackTemplate(idx - 1);
            var mobAniList = new List<MobAttackAnimationTemplate>();

            foreach (var attackItem in infoProp.Children)
            {
                if (attackItem.Name == "info")
                {
                    foreach (var attackInfoProp in attackItem.Children)
                    {
                        var attackInfoPropName = attackInfoProp.Name;
                        if (attackInfoPropName == "conMP") model.ConMP = attackInfoProp.GetIntValue();
                        else if (attackInfoPropName == "attackAfter") model.AttackAfter = attackInfoProp.GetIntValue();
                        else if (attackInfoPropName == "PADamage") model.PADamage = attackInfoProp.GetIntValue();
                        else if (attackInfoPropName == "deadlyAttack") model.DeadlyAttack = attackInfoProp.ResolveBool() ?? false;
                        else if (attackInfoPropName == "mpBurn") model.MpBurn = attackInfoProp.GetIntValue();
                        else if (attackInfoPropName == "disease") model.Disease = attackInfoProp.GetIntValue();
                        else if (attackInfoPropName == "level") model.Level = attackInfoProp.GetIntValue();
                    }
                }
                else if (int.TryParse(attackItem.Name, out var innerIdx))
                {
                    var ani = new MobAttackAnimationTemplate(innerIdx);
                    foreach (var itemProp in attackItem.Children)
                        if (itemProp.Name == "delay")
                            ani.Delay = itemProp.GetIntValue();
                    mobAniList.Add(ani);
                }
            }

            model.Animations = [.. mobAniList];
            return model;
        }
    }
}
