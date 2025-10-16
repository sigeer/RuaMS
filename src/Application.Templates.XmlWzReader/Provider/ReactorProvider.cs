using Application.Templates.Exceptions;
using Application.Templates.Providers;
using Application.Templates.Reactor;
using Microsoft.Extensions.Logging;
using System.Xml;
using System.Xml.Linq;

namespace Application.Templates.XmlWzReader.Provider
{
    public sealed class ReactorProvider : AbstractGroupProvider<ReactorTemplate>
    {
        public override string ProviderName => ProviderNames.Reactor;

        public ReactorProvider(TemplateOptions options)
            : base(options) { }

        protected override string? GetImgPathByTemplateId(int reactorId)
        {
            string fileName = reactorId.ToString().PadLeft(7, '0') + ".img.xml";
            return Path.Combine(ProviderName, fileName);
        }

        protected override IEnumerable<AbstractTemplate> GetDataFromImg(string? imgPath)
        {
            try
            {
                using var fis = _fileProvider.ReadFile(imgPath);
                using var reader = XmlReader.Create(fis, XmlReaderUtils.ReaderSettings);
                var xDoc = XDocument.Load(reader).Root!;

                if (!int.TryParse(xDoc.GetName().AsSpan(0, 7), out var reactorId))
                    throw new TemplateFormatException(ProviderName, imgPath);

                var pEntry = new ReactorTemplate(reactorId);
                List<ReactorTemplate.StateInfo> list = [];
                int linkedId = -1;
                foreach (var rootPropNode in xDoc.Elements())
                {
                    var rootPropName = rootPropNode.Attribute("name")?.Value;
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
