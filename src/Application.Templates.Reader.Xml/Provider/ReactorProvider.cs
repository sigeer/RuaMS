using Application.Templates.Exceptions;
using Application.Templates.Reader;
using Application.Templates.Reader.Resolvers;
using Application.Templates.Reactor;
using Microsoft.Extensions.Logging;
using System.Xml;
using System.Xml.Linq;

namespace Application.Templates.Reader.Xml.Provider
{
    public sealed class ReactorProvider : AbstractGroupProvider<ReactorTemplate>
    {
        public override ProviderType Type => ProviderType.Reactor;

        public ReactorProvider(IWzPathResolver resolver, bool useCache = true) : base(resolver, useCache)
        {

        }

        protected override IEnumerable<ReactorTemplate> GetDataFromImg(string? imgPath)
        {
            try
            {
                using var fis = File.Open(_resolver.ResolveFullPath(imgPath), FileMode.Open, FileAccess.Read, FileShare.Read);
                using var reader = XmlReader.Create(fis, XmlReaderUtils.ReaderSettings);
                var xDoc = XDocument.Load(reader).Root!;

                if (!int.TryParse(xDoc.GetName().AsSpan(0, 7), out var reactorId))
                    throw new TemplateFormatException("Reactor.wz", imgPath);

                var pEntry = new ReactorTemplate(reactorId);
                List<ReactorTemplate.StateInfo> list = [];
                int linkedId = -1;
                foreach (var rootPropNode in xDoc.Elements())
                {
                    var rootPropName = rootPropNode.GetName();
                    if (rootPropName == "info")
                    {
                        foreach (var infoPropNode in rootPropNode.Elements())
                        {
                            var infoPropName = infoPropNode.GetName();
                            if (infoPropName == "link" && int.TryParse(infoPropNode.Attribute("value")?.Value, out var linkedReactorId))
                            {
                                linkedId = linkedReactorId;
                            }
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
                                    if (evtNodeName != null && evtNodeName.Equals("timeOut", StringComparison.OrdinalIgnoreCase))
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
                    else if (rootPropName == "action")
                    {
                        pEntry.Action = rootPropNode.GetStringValue();
                    }
                }
                pEntry.StateInfoList = list.OrderBy(x => x.State).ToArray();
                if (linkedId > 0)
                    GetItem(linkedId)?.CloneLink(pEntry);

                InsertItem(pEntry);
                return [pEntry];
            }
            catch (Exception ex)
            {
                LibLog.Logger.LogError(ex.ToString());
                return [];
            }
        }
    }
}
