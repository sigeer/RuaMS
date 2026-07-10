using Application.Templates.Exceptions;
using Application.Templates.Reader;
using Application.Templates.Reader.Resolvers;
using Application.Templates.Reactor;
using Duey.Abstractions;
using Duey.Provider.WZ.Files;
using Microsoft.Extensions.Logging;

namespace Application.Templates.Reader.Img.Provider
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
                var fullPath = _resolver.ResolveFullPath(imgPath);
                var rootNode = new WZImage(fullPath);

                if (!int.TryParse(rootNode.Name.AsSpan(0, 7), out var reactorId))
                    throw new TemplateFormatException("Reactor.wz", imgPath);

                var pEntry = new ReactorTemplate(reactorId);
                List<ReactorTemplate.StateInfo> stateInfoList = [];
                int linkedId = -1;

                foreach (var rootPropNode in rootNode.Children)
                {
                    var rootPropName = rootPropNode.Name;
                    if (rootPropName == "info")
                    {
                        foreach (var infoPropNode in rootPropNode.Children)
                        {
                            var infoPropName = infoPropNode.Name;
                            if (infoPropName == "link")
                            {
                                if (int.TryParse(infoPropNode.GetStringValue(), out var linkedReactorId))
                                    linkedId = linkedReactorId;
                            }
                        }
                    }
                    else if (int.TryParse(rootPropName, out var idx))
                    {
                        var stateInfo = new ReactorTemplate.StateInfo(idx);
                        foreach (var item in rootPropNode.Children)
                        {
                            if (item.Name == "event")
                            {
                                List<ReactorTemplate.StateInfo.EventInfo> events = [];
                                foreach (var eventNode in item.Children)
                                {
                                    var evtNodeName = eventNode.Name;
                                    if (string.Equals(evtNodeName, "timeOut", StringComparison.OrdinalIgnoreCase))
                                        stateInfo.TimeOut = eventNode.GetIntValue(defaultValue: -1);
                                    else if (int.TryParse(evtNodeName, out var evtIdx))
                                    {
                                        var eventInfo = new ReactorTemplate.StateInfo.EventInfo();
                                        foreach (var evtObjNode in eventNode.Children)
                                        {
                                            var propName = evtObjNode.Name;
                                            if (propName == "type")
                                                eventInfo.EventType = evtObjNode.GetIntValue();
                                            else if (propName == "state")
                                                eventInfo.NextState = evtObjNode.GetIntValue();
                                            else if (propName == "activeSkillID")
                                            {
                                                List<int> skillList = [];
                                                foreach (var skillItem in evtObjNode.Children)
                                                    skillList.Add(skillItem.GetIntValue());
                                                eventInfo.ActiveSkillId = [.. skillList];
                                            }
                                            else if (propName == "lt")
                                            {
                                                var vec = evtObjNode.ResolveVector();
                                                if (vec.HasValue)
                                                    eventInfo.Lt = new System.Drawing.Point(vec.Value.Item1, vec.Value.Item2);
                                            }
                                            else if (propName == "rb")
                                            {
                                                var vec = evtObjNode.ResolveVector();
                                                if (vec.HasValue)
                                                    eventInfo.Rb = new System.Drawing.Point(vec.Value.Item1, vec.Value.Item2);
                                            }
                                            else if (propName == "0")
                                                eventInfo.Int0Value = evtObjNode.GetIntValue();
                                            else if (propName == "1")
                                                eventInfo.Int1Value = evtObjNode.GetIntValue(defaultValue: 1);
                                            else if (propName == "2")
                                                eventInfo.Int2Value = evtObjNode.GetIntValue();
                                        }
                                        events.Add(eventInfo);
                                    }
                                }
                                stateInfo.EventInfos = [.. events];
                            }
                        }
                        stateInfoList.Add(stateInfo);
                    }
                    else if (rootPropName == "action")
                    {
                        pEntry.Action = rootPropNode.GetStringValue();
                    }
                }

                pEntry.StateInfoList = [.. stateInfoList.OrderBy(x => x.State)];
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
