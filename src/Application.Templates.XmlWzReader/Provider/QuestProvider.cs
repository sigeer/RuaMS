using Application.Templates.Providers;
using Application.Templates.Quest;
using Microsoft.Extensions.Logging;
using System.Xml;
using System.Xml.Linq;
using static Application.Templates.Quest.QuestAct;
using static Application.Templates.Quest.QuestDemand;

namespace Application.Templates.XmlWzReader.Provider
{
    public sealed class QuestProvider : AbstractCompositeProvider<QuestTemplate>
    {
        public override string ProviderName => ProviderNames.Quest;
        public QuestProvider(ProviderOption options)
            : base(options, ["QuestInfo.img.xml", "Act.img.xml", "Check.img.xml"])
        {
        }

        protected override IEnumerable<AbstractTemplate> GetDataFromImg()
        {
            try
            {
                List<QuestInfoTemplate> infoList = [];
                List<QuestActTemplate> actList = [];
                List<QuestCheckTemplate> checkList = [];

                foreach (var file in _files)
                {
                    using var fis = _fileProvider.ReadFile(file);
                    using var reader = XmlReader.Create(fis, XmlReaderUtils.ReaderSettings);
                    var xDoc = XDocument.Load(reader).Root!;

                    var fileType = xDoc.Attribute("name")?.Value;
                    if (fileType == "QuestInfo.img")
                    {
                        foreach (var questInfoNode in xDoc.Elements())
                        {
                            if (int.TryParse(questInfoNode.Attribute("name")?.Value, out var questId))
                            {
                                var quest = new QuestInfoTemplate(questId);
                                foreach (var questProp in questInfoNode.Elements())
                                {
                                    var propName = questProp.GetName();
                                    if (propName == "name")
                                        quest.Name = questProp.GetStringValue() ?? "";
                                    else if (propName == "parent")
                                        quest.Parent = questProp.GetStringValue();
                                    else if (propName == "autoStart")
                                        quest.AutoStart = questProp.GetBoolValue();
                                    else if (propName == "autoPreComplete")
                                        quest.AutoPreComplete = questProp.GetBoolValue();
                                    else if (propName == "autoComplete")
                                        quest.AutoComplete = questProp.GetBoolValue();
                                    else if (propName == "viewMedalItem")
                                        quest.ViewMedalItem = questProp.GetIntValue();
                                    else if (propName == "timeLimit")
                                        quest.TimeLimit = questProp.GetIntValue();
                                    else if (propName == "timeLimit2")
                                        quest.TimeLimit2 = questProp.GetIntValue();
                                }
                                infoList.Add(quest);
                            }
                        }
                    }

                    else if (fileType == "Act.img")
                    {
                        ProcessAct(actList, xDoc);
                    }

                    else if (fileType == "Check.img")
                    {
                        ProcessCheck(checkList, xDoc);
                    }
                }

                var allData = (from info in infoList
                               join act in actList on info.QuestId equals act.QuestId into acts
                               from a in acts.DefaultIfEmpty()
                               join chck in checkList on info.QuestId equals chck.QuestId into chcks
                               from b in chcks.DefaultIfEmpty()
                               select new QuestTemplate(info) { Act = a, Check = b });
                foreach (var item in allData)
                {
                    InsertItem(item);
                }
                return allData;
            }
            catch (Exception ex)
            {
                LibLog.Logger.LogError(ex.ToString());
                return [];
            }
        }

        private static void ProcessCheck(List<QuestCheckTemplate> checkList, XElement xDoc)
        {
            foreach (var questNode in xDoc.Elements())
            {
                if (int.TryParse(questNode.Attribute("name")?.Value, out var questId))
                {
                    var questCheckTemplate = new QuestCheckTemplate(questId);
                    foreach (var stepNode in questNode.Elements())
                    {
                        var demand = new QuestDemand();
                        foreach (var stepPropNode in stepNode.Elements())
                        {
                            var propName = stepPropNode.GetName();
                            if (propName == "npc")
                                demand.Npc = stepPropNode.GetIntValue();
                            else if (propName == "lvmin")
                                demand.LevelMin = stepPropNode.GetIntValue();
                            else if (propName == "lvmax")
                                demand.LevelMax = stepPropNode.GetIntValue();
                            else if (propName == "interval")
                                demand.Interval = stepPropNode.GetIntValue();
                            else if (propName == "startscript")
                                demand.StartScript = stepPropNode.GetStringValue();
                            else if (propName == "endscript")
                                demand.EndScript = stepPropNode.GetStringValue();
                            else if (propName == "start")
                                demand.Start = stepPropNode.GetStringValue();
                            else if (propName == "end")
                                demand.End = stepPropNode.GetStringValue();
                            else if (propName == "pettamenessmin")
                                demand.PetTamenessMin = stepPropNode.GetIntValue();
                            else if (propName == "normalAutoStart")
                                demand.NormalAutoStart = stepPropNode.GetBoolValue();
                            else if (propName == "infoNumber")
                                demand.InfoNumber = stepPropNode.GetIntValue();
                            else if (propName == "questComplete")
                                demand.QuestComplete = stepPropNode.GetIntValue();
                            else if (propName == "dayByDay")
                                demand.DayByDay = stepPropNode.GetBoolValue();
                            else if (propName == "buff")
                                demand.Buff = stepPropNode.GetIntValue();
                            else if (propName == "exceptbuff")
                                demand.ExceptBuff = stepPropNode.GetIntValue();
                            else if (propName == "money")
                                demand.Meso = stepPropNode.GetIntValue();

                            else if (propName == "item")
                            {
                                List<ItemInfo> list = [];
                                foreach (var itemNode in stepPropNode.Elements())
                                {
                                    var itemModel = new ItemInfo();
                                    foreach (var itemProp in itemNode.Elements())
                                    {
                                        var itemPropName = itemProp.GetName();
                                        if (itemPropName == "id")
                                            itemModel.ItemID = itemProp.GetIntValue();
                                        else if (itemPropName == "count")
                                            itemModel.Count = itemProp.GetIntValue();
                                    }
                                    list.Add(itemModel);
                                }
                                demand.DemandItem = list.ToArray();
                            }


                            else if (propName == "job")
                            {
                                demand.Job = stepPropNode.Elements().Select(x => x.GetIntValue()).ToArray();
                            }

                            else if (propName == "fieldEnter")
                            {
                                demand.FieldEnter = stepPropNode.Elements().Select(x => x.GetIntValue()).ToArray();
                            }

                            else if (propName == "quest")
                            {
                                List<QuestRecord> list = [];
                                foreach (var itemNode in stepPropNode.Elements())
                                {
                                    var itemModel = new QuestRecord();
                                    foreach (var itemProp in itemNode.Elements())
                                    {
                                        var itemPropName = itemProp.GetName();
                                        if (itemPropName == "id")
                                            itemModel.QuestID = itemProp.GetIntValue();
                                        else if (itemPropName == "state")
                                            itemModel.State = itemProp.GetIntValue();
                                    }
                                    list.Add(itemModel);
                                }
                                demand.DemandQuest = list.ToArray();
                            }

                            else if (propName == "mob")
                            {
                                List<MobInfo> list = [];
                                foreach (var itemNode in stepPropNode.Elements())
                                {
                                    var itemModel = new MobInfo();
                                    foreach (var itemProp in itemNode.Elements())
                                    {
                                        var itemPropName = itemProp.GetName();
                                        if (itemPropName == "id")
                                            itemModel.MobID = itemProp.GetIntValue();
                                        else if (itemPropName == "count")
                                            itemModel.Count = itemProp.GetIntValue();
                                    }
                                    list.Add(itemModel);
                                }
                                demand.DemandMob = list.ToArray();
                            }

                            else if (propName == "pet")
                            {
                                List<PetInfo> list = [];
                                foreach (var itemNode in stepPropNode.Elements())
                                {
                                    var itemModel = new PetInfo();
                                    foreach (var itemProp in itemNode.Elements())
                                    {
                                        var itemPropName = itemProp.GetName();
                                        if (itemPropName == "id")
                                            itemModel.PetID = itemProp.GetIntValue();
                                    }
                                    list.Add(itemModel);
                                }
                                demand.Pet = list.ToArray();
                            }

                            else if (propName == "infoex")
                            {
                                List<QuestInfoEx> list = [];
                                foreach (var itemNode in stepPropNode.Elements())
                                {
                                    var itemModel = new QuestInfoEx();
                                    foreach (var itemProp in itemNode.Elements())
                                    {
                                        var itemPropName = itemProp.GetName();
                                        if (itemPropName == "value")
                                            itemModel.Value = itemProp.GetStringValue() ?? "";
                                        if (itemPropName == "cond")
                                            itemModel.Cond = itemProp.GetIntValue();
                                    }
                                    list.Add(itemModel);
                                }
                                demand.InfoEx = list.ToArray();
                            }
                        }

                        if (stepNode.GetName() == "0")
                            questCheckTemplate.StartDemand = demand;
                        else if (stepNode.GetName() == "1")
                            questCheckTemplate.EndDemand = demand;
                    }
                    checkList.Add(questCheckTemplate);
                }
            }
        }

        private static void ProcessAct(List<QuestActTemplate> actList, XElement xDoc)
        {
            foreach (var questNode in xDoc.Elements())
            {
                if (int.TryParse(questNode.Attribute("name")?.Value, out var questId))
                {
                    var questActTemplate = new QuestActTemplate() { QuestId = questId };
                    foreach (var stepNode in questNode.Elements())
                    {
                        var act = new QuestAct();
                        foreach (var stepPropNode in stepNode.Elements())
                        {
                            var propName = stepPropNode.Attribute("name")?.Value;
                            if (propName == "nextQuest")
                                act.NextQuest = stepPropNode.GetIntValue();
                            else if (propName == "npc")
                                act.Npc = stepPropNode.GetIntValue();
                            else if (propName == "money")
                                act.Money = stepPropNode.GetIntValue();
                            else if (propName == "exp")
                                act.Exp = stepPropNode.GetIntValue();
                            else if (propName == "fame")
                                act.Fame = stepPropNode.GetIntValue();
                            else if (propName == "lvmin")
                                act.LevelMin = stepPropNode.GetIntValue();
                            else if (propName == "lvmax")
                                act.LevelMax = stepPropNode.GetIntValue();
                            else if (propName == "info")
                                act.Info = stepPropNode.GetStringValue();
                            else if (propName == "pettameness")
                                act.PetTameness = stepPropNode.GetIntValue();
                            else if (propName == "petskill")
                                act.PetSkill = stepPropNode.GetIntValue();
                            else if (propName == "petspeed")
                                act.PetSpeed = stepPropNode.GetIntValue();
                            else if (propName == "buffItemID")
                                act.BuffItemID = stepPropNode.GetIntValue();
                            else if (propName == "interval")
                                act.Interval = stepPropNode.GetIntValue();

                            else if (propName == "item")
                            {
                                List<ActItem> list = [];
                                foreach (var itemNode in stepPropNode.Elements())
                                {
                                    var itemModel = new ActItem();
                                    foreach (var itemProp in itemNode.Elements())
                                    {
                                        var itemPropName = itemProp.GetName();
                                        if (itemPropName == "id")
                                            itemModel.ItemID = itemProp.GetIntValue();
                                        else if (itemPropName == "count")
                                            itemModel.Count = itemProp.GetIntValue();
                                        else if (itemPropName == "prop")
                                            itemModel.Prop = itemProp.GetIntValue();
                                        else if (itemPropName == "gender")
                                            itemModel.Gender = itemProp.GetIntValue();
                                        else if (itemPropName == "period")
                                            itemModel.Period = itemProp.GetIntValue();
                                        else if (itemPropName == "job")
                                            itemModel.Job = itemProp.GetIntValue();
                                    }
                                    list.Add(itemModel);
                                }
                                act.Items = list.ToArray();
                            }
                            else if (propName == "skill")
                            {
                                List<ActSkill> list = [];
                                foreach (var itemNode in stepPropNode.Elements())
                                {
                                    var itemModel = new ActSkill();
                                    foreach (var itemProp in itemNode.Elements())
                                    {
                                        var itemPropName = itemProp.GetName();
                                        if (itemPropName == "id")
                                            itemModel.SkillID = itemProp.GetIntValue();
                                        else if (itemPropName == "skillLevel")
                                            itemModel.SkillLevel = itemProp.GetIntValue();
                                        else if (itemPropName == "masterLevel")
                                            itemModel.MasterLevel = itemProp.GetIntValue();
                                        else if (itemPropName == "job")
                                            itemModel.Job = itemProp.Elements().Select(x => x.GetIntValue()).ToArray();
                                    }
                                    list.Add(itemModel);
                                }
                                act.Skills = list.ToArray();
                            }

                            else if (propName == "quest")
                            {
                                List<ActQuest> list = [];
                                foreach (var itemNode in stepPropNode.Elements())
                                {
                                    var itemModel = new ActQuest();
                                    foreach (var itemProp in itemNode.Elements())
                                    {
                                        var itemPropName = itemProp.GetName();
                                        if (itemPropName == "id")
                                            itemModel.QuestId = itemProp.GetIntValue();
                                        else if (itemPropName == "state")
                                            itemModel.State = itemProp.GetIntValue();
                                    }
                                    list.Add(itemModel);
                                }
                                act.Quests = list.ToArray();
                            }
                        }

                        if (stepNode.Attribute("name")?.Value == "0")
                            questActTemplate.StartAct = act;
                        else if (stepNode.Attribute("name")?.Value == "1")
                            questActTemplate.EndAct = act;
                    }
                    actList.Add(questActTemplate);
                }
            }
        }
    }
}
