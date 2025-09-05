using Application.Templates.Providers;
using Application.Templates.Reactor;
using System.Xml.Linq;

namespace Application.Templates.XmlWzReader.Provider
{
    public sealed class ReactorProvider : AbstractProvider<ReactorTemplate>
    {
        public override ProviderType ProviderName => ProviderType.Reactor;

        public ReactorProvider(TemplateOptions options)
            : base(options) { }

        protected override string GetImgPathByTemplateId(int reactorId)
        {
            string fileName = reactorId.ToString().PadLeft(7, '0') + ".img.xml";
            return Path.Combine(GetPath(), fileName);
        }

        protected override void GetDataFromImg(string imgPath)
        {
            using var fis = new FileStream(imgPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var xDoc = XDocument.Load(fis).Root!;

            if (!int.TryParse(xDoc.Attribute("name")?.Value?.Substring(0, 7), out var reactorId))
                return;

            var pEntry = new ReactorTemplate(reactorId);
            List<ReactorTemplate.StateInfo> list = [];
            foreach (var rootPropNode in xDoc.Elements())
            {
                var rootPropName = rootPropNode.Attribute("name")?.Value;
                if (rootPropName == "info")
                {
                    foreach (var infoPropNode in rootPropNode.Elements())
                    {
                        var infoPropName = infoPropNode.GetName();
                        if (infoPropName == "link" && int.TryParse(infoPropNode.Attribute("value")?.Value, out var linkedReactorId))
                            GetItem(linkedReactorId)?.CloneLink(pEntry);
                    }
                }
                else if (int.TryParse(rootPropName, out var idx))
                {
                    var stateInfo = new ReactorTemplate.StateInfo(idx);
                    foreach (var item in rootPropNode.Elements())
                    {
                        if (item.GetName() == "event")
                        {
                            List<ReactorTemplate.StateInfo.EventInfo> events = [];
                            foreach (var eventNode in item.Elements())
                            {
                                var evtNodeName = eventNode.GetName();
                                if (evtNodeName == "timeOut")
                                    stateInfo.TimeOut = eventNode.GetIntValue();
                                else if (int.TryParse(evtNodeName, out var evtIdx))
                                {
                                    var eventInfo = new ReactorTemplate.StateInfo.EventInfo();
                                    foreach (var evtObjNode in eventNode.Elements())
                                    {
                                        var propName = evtObjNode.Attribute("name")?.Value;
                                        if (propName == "type")
                                            eventInfo.EventType = evtObjNode.GetIntValue();
                                        else if (propName == "state")
                                            eventInfo.NextState = evtObjNode.GetIntValue();
                                        else if (propName == "activeSkillID")
                                        {
                                            List<int> skillList = [];
                                            foreach (var skillItem in evtObjNode.Elements())
                                            {
                                                skillList.Add(skillItem.GetIntValue());
                                            }
                                            eventInfo.ActiveSkillId = skillList.ToArray();
                                        }
                                        else if (propName == "lt")
                                            eventInfo.Lt = evtObjNode.GetVectorValue();
                                        else if (propName == "rb")
                                            eventInfo.Rb = evtObjNode.GetVectorValue();
                                        else if (propName == "0")
                                            eventInfo.Int0Value = evtObjNode.GetIntValue();
                                        else if (propName == "1")
                                            eventInfo.Int1Value = evtObjNode.GetIntValue();
                                        else if (propName == "2")
                                            eventInfo.Int2Value = evtObjNode.GetIntValue();
                                    }
                                    events.Add(eventInfo);
                                }
                            }
                            stateInfo.EventInfos = events.ToArray();
                        }
                    }
                    list.Add(stateInfo);
                }
            }
            pEntry.StateInfoList = list.ToArray();
            InsertItem(pEntry);
        }

        protected override void LoadAllInternal()
        {
            var files = new DirectoryInfo(Path.Combine(GetPath())).GetFiles("*.xml", SearchOption.AllDirectories);

            foreach (var item in files)
            {
                GetDataFromImg(item.FullName);
            }
        }

    }
}
