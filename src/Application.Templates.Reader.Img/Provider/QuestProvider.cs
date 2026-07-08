using Application.Templates.Reader;
using Application.Templates.Reader.Resolvers;
using Application.Templates.Quest;
using Duey.Abstractions;
using Duey.Provider.WZ.Files;
using Microsoft.Extensions.Logging;
using static Application.Templates.Quest.QuestAct;
using static Application.Templates.Quest.QuestDemand;

namespace Application.Templates.Reader.Img.Provider
{
    public sealed class QuestProvider : AbstractCompositeProvider<QuestTemplate>
    {
        public override ProviderType Type =>  ProviderType.Quest;
        public QuestProvider(IWzPathResolver resolver)
            : base(resolver)
        {
        }

        protected override IEnumerable<QuestTemplate> GetDataFromImg()
        {
            try
            {
                List<QuestInfoTemplate> infoList = [];
                List<QuestActTemplate> actList = [];
                List<QuestCheckTemplate> checkList = [];

                foreach (var file in _resolver.ResolveGroup(Type))
                {
                    var fullPath = _resolver.ResolveFullPath(file);
                    var rootNode = new WZImage(fullPath);

                    var fileType = rootNode.Name;
                    if (fileType == "QuestInfo.img")
                    {
                        foreach (var questInfoNode in rootNode.Children)
                        {
                            if (int.TryParse(questInfoNode.Name, out var questId))
                            {
                                var quest = new QuestInfoTemplate(questId);
                                foreach (var questProp in questInfoNode.Children)
                                {
                                    var propName = questProp.Name;
                                    if (propName == "name") quest.Name = questProp.GetStringValue() ?? "";
                                    else if (propName == "parent") quest.Parent = questProp.GetStringValue();
                                    else if (propName == "autoStart") quest.AutoStart = questProp.ResolveBool() ?? false;
                                    else if (propName == "autoPreComplete") quest.AutoPreComplete = questProp.ResolveBool() ?? false;
                                    else if (propName == "autoComplete") quest.AutoComplete = questProp.ResolveBool() ?? false;
                                    else if (propName == "viewMedalItem") quest.ViewMedalItem = questProp.GetIntValue();
                                    else if (propName == "timeLimit") quest.TimeLimit = questProp.GetIntValue();
                                    else if (propName == "timeLimit2") quest.TimeLimit2 = questProp.GetIntValue();
                                    else if (propName == "area") quest.Area = questProp.GetIntValue();
                                }
                                infoList.Add(quest);
                            }
                        }
                    }
                    else if (fileType == "Act.img")
                        ProcessAct(actList, rootNode);
                    else if (fileType == "Check.img")
                        ProcessCheck(checkList, rootNode);
                }

                var allData = from info in infoList
                              join act in actList on info.QuestId equals act.QuestId into acts
                              from a in acts.DefaultIfEmpty()
                              join chck in checkList on info.QuestId equals chck.QuestId into chcks
                              from b in chcks.DefaultIfEmpty()
                              select new QuestTemplate(info) { Act = a, Check = b };
                foreach (var item in allData)
                    InsertItem(item);
                return allData;
            }
            catch (Exception ex)
            {
                LibLog.Logger.LogError(ex.ToString());
                return [];
            }
        }

        private static void ProcessCheck(List<QuestCheckTemplate> checkList, IDataNode xDoc)
        {
            foreach (var questNode in xDoc.Children)
            {
                if (int.TryParse(questNode.Name, out var questId))
                {
                    var questCheckTemplate = new QuestCheckTemplate(questId);
                    foreach (var stepNode in questNode.Children)
                    {
                        var demand = new QuestDemand();
                        foreach (var stepPropNode in stepNode.Children)
                        {
                            var propName = stepPropNode.Name;
                            if (propName == "npc") demand.Npc = stepPropNode.GetIntValue();
                            else if (propName == "lvmin") demand.LevelMin = stepPropNode.GetIntValue();
                            else if (propName == "lvmax") demand.LevelMax = stepPropNode.GetIntValue();
                            else if (propName == "interval") demand.Interval = stepPropNode.GetIntValue();
                            else if (propName == "startscript") demand.StartScript = stepPropNode.GetStringValue();
                            else if (propName == "endscript") demand.EndScript = stepPropNode.GetStringValue();
                            else if (propName == "start") demand.Start = stepPropNode.GetStringValue();
                            else if (propName == "end") demand.End = stepPropNode.GetStringValue();
                            else if (propName == "pettamenessmin") demand.PetTamenessMin = stepPropNode.GetIntValue();
                            else if (propName == "normalAutoStart") demand.NormalAutoStart = stepPropNode.ResolveBool() ?? false;
                            else if (propName == "infoNumber") demand.InfoNumber = stepPropNode.GetIntValue();
                            else if (propName == "questComplete") demand.QuestComplete = stepPropNode.GetIntValue();
                            else if (propName == "dayByDay") demand.DayByDay = stepPropNode.ResolveBool() ?? false;
                            else if (propName == "buff") demand.Buff = stepPropNode.GetIntValue();
                            else if (propName == "exceptbuff") demand.ExceptBuff = stepPropNode.GetIntValue();
                            else if (propName == "money") demand.Meso = stepPropNode.GetIntValue();
                            else if (propName == "item")
                            {
                                List<ItemInfo> list = [];
                                foreach (var itemNode in stepPropNode.Children)
                                {
                                    var itemModel = new ItemInfo();
                                    foreach (var itemProp in itemNode.Children)
                                    {
                                        var itemPropName = itemProp.Name;
                                        if (itemPropName == "id") itemModel.ItemID = itemProp.GetIntValue();
                                        else if (itemPropName == "count") itemModel.Count = itemProp.GetIntValue();
                                    }
                                    list.Add(itemModel);
                                }
                                demand.DemandItem = [.. list];
                            }
                            else if (propName == "job")
                                demand.Job = stepPropNode.Children.Select(x => x.GetIntValue()).ToArray();
                            else if (propName == "fieldEnter")
                                demand.FieldEnter = stepPropNode.Children.Select(x => x.GetIntValue()).ToArray();
                            else if (propName == "quest")
                            {
                                List<QuestRecord> list = [];
                                foreach (var itemNode in stepPropNode.Children)
                                {
                                    var itemModel = new QuestRecord();
                                    foreach (var itemProp in itemNode.Children)
                                    {
                                        var itemPropName = itemProp.Name;
                                        if (itemPropName == "id") itemModel.QuestID = itemProp.GetIntValue();
                                        else if (itemPropName == "state") itemModel.State = itemProp.GetIntValue();
                                    }
                                    list.Add(itemModel);
                                }
                                demand.DemandQuest = [.. list];
                            }
                            else if (propName == "mob")
                            {
                                List<MobInfo> list = [];
                                foreach (var itemNode in stepPropNode.Children)
                                {
                                    var itemModel = new MobInfo();
                                    foreach (var itemProp in itemNode.Children)
                                    {
                                        var itemPropName = itemProp.Name;
                                        if (itemPropName == "id") itemModel.MobID = itemProp.GetIntValue();
                                        else if (itemPropName == "count") itemModel.Count = itemProp.GetIntValue();
                                    }
                                    list.Add(itemModel);
                                }
                                demand.DemandMob = [.. list];
                            }
                            else if (propName == "pet")
                            {
                                List<PetInfo> list = [];
                                foreach (var itemNode in stepPropNode.Children)
                                {
                                    var itemModel = new PetInfo();
                                    foreach (var itemProp in itemNode.Children)
                                    {
                                        var itemPropName = itemProp.Name;
                                        if (itemPropName == "id") itemModel.PetID = itemProp.GetIntValue();
                                    }
                                    list.Add(itemModel);
                                }
                                demand.Pet = [.. list];
                            }
                            else if (propName == "infoex")
                            {
                                List<QuestInfoEx> list = [];
                                foreach (var itemNode in stepPropNode.Children)
                                {
                                    var itemModel = new QuestInfoEx();
                                    foreach (var itemProp in itemNode.Children)
                                    {
                                        var itemPropName = itemProp.Name;
                                        if (itemPropName == "value") itemModel.Value = itemProp.GetStringValue() ?? "";
                                        if (itemPropName == "cond") itemModel.Cond = itemProp.GetIntValue();
                                    }
                                    list.Add(itemModel);
                                }
                                demand.InfoEx = [.. list];
                            }
                        }
                        if (stepNode.Name == "0")
                            questCheckTemplate.StartDemand = demand;
                        else if (stepNode.Name == "1")
                            questCheckTemplate.EndDemand = demand;
                    }
                    checkList.Add(questCheckTemplate);
                }
            }
        }

        private static void ProcessAct(List<QuestActTemplate> actList, IDataNode xDoc)
        {
            foreach (var questNode in xDoc.Children)
            {
                if (int.TryParse(questNode.Name, out var questId))
                {
                    var questActTemplate = new QuestActTemplate { QuestId = questId };
                    foreach (var stepNode in questNode.Children)
                    {
                        var act = new QuestAct();
                        foreach (var stepPropNode in stepNode.Children)
                        {
                            var propName = stepPropNode.Name;
                            if (propName == "nextQuest") act.NextQuest = stepPropNode.GetIntValue();
                            else if (propName == "npc") act.Npc = stepPropNode.GetIntValue();
                            else if (propName == "money") act.Money = stepPropNode.GetIntValue();
                            else if (propName == "exp") act.Exp = stepPropNode.GetIntValue();
                            else if (propName == "fame" || propName == "pop") act.Fame = stepPropNode.GetIntValue();
                            else if (propName == "lvmin") act.LevelMin = stepPropNode.GetIntValue();
                            else if (propName == "lvmax") act.LevelMax = stepPropNode.GetIntValue();
                            else if (propName == "info") act.Info = stepPropNode.GetStringValue();
                            else if (propName == "pettameness") act.PetTameness = stepPropNode.GetIntValue();
                            else if (propName == "petskill") act.PetSkill = stepPropNode.GetIntValue();
                            else if (propName == "petspeed") act.PetSpeed = stepPropNode.GetIntValue();
                            else if (propName == "buffItemID") act.BuffItemID = stepPropNode.GetIntValue();
                            else if (propName == "interval") act.Interval = stepPropNode.GetIntValue();
                            else if (propName == "item")
                            {
                                List<ActItem> list = [];
                                foreach (var itemNode in stepPropNode.Children)
                                {
                                    var itemModel = new ActItem();
                                    foreach (var itemProp in itemNode.Children)
                                    {
                                        var itemPropName = itemProp.Name;
                                        if (itemPropName == "id") itemModel.ItemID = itemProp.GetIntValue();
                                        else if (itemPropName == "count") itemModel.Count = itemProp.GetIntValue();
                                        else if (itemPropName == "prop") itemModel.Prop = itemProp.GetIntValue();
                                        else if (itemPropName == "gender") itemModel.Gender = itemProp.GetIntValue();
                                        else if (itemPropName == "period") itemModel.Period = itemProp.GetIntValue();
                                        else if (itemPropName == "job") itemModel.Job = itemProp.GetIntValue();
                                    }
                                    list.Add(itemModel);
                                }
                                act.Items = [.. list];
                            }
                            else if (propName == "skill")
                            {
                                List<ActSkill> list = [];
                                foreach (var itemNode in stepPropNode.Children)
                                {
                                    var itemModel = new ActSkill();
                                    foreach (var itemProp in itemNode.Children)
                                    {
                                        var itemPropName = itemProp.Name;
                                        if (itemPropName == "id") itemModel.SkillID = itemProp.GetIntValue();
                                        else if (itemPropName == "skillLevel") itemModel.SkillLevel = itemProp.GetIntValue();
                                        else if (itemPropName == "masterLevel") itemModel.MasterLevel = itemProp.GetIntValue();
                                        else if (itemPropName == "job")
                                            itemModel.Job = itemProp.Children.Select(x => x.GetIntValue()).ToArray();
                                    }
                                    list.Add(itemModel);
                                }
                                act.Skills = [.. list];
                            }
                            else if (propName == "quest")
                            {
                                List<ActQuest> list = [];
                                foreach (var itemNode in stepPropNode.Children)
                                {
                                    var itemModel = new ActQuest();
                                    foreach (var itemProp in itemNode.Children)
                                    {
                                        var itemPropName = itemProp.Name;
                                        if (itemPropName == "id") itemModel.QuestId = itemProp.GetIntValue();
                                        else if (itemPropName == "state") itemModel.State = itemProp.GetIntValue();
                                    }
                                    list.Add(itemModel);
                                }
                                act.Quests = [.. list];
                            }
                        }
                        if (stepNode.Name == "0")
                            questActTemplate.StartAct = act;
                        else if (stepNode.Name == "1")
                            questActTemplate.EndAct = act;
                    }
                    actList.Add(questActTemplate);
                }
            }
        }
    }
}
