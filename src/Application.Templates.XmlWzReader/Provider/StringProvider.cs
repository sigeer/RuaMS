using Application.Templates.Providers;
using Application.Templates.String;
using System.Xml.Linq;

namespace Application.Templates.XmlWzReader.Provider
{
    public sealed class StringProvider : AbstractProvider<GenericKeyedTemplate<StringTemplate>>
    {
        public override ProviderType ProviderName => ProviderType.String;
        string[] _files;

        public StringProvider(TemplateOptions options)
            : base(options)
        {
            _files = Directory.GetFiles(GetPath(), "*.xml");
        }


        protected override IEnumerable<AbstractTemplate> LoadAllInternal()
        {
            List<AbstractTemplate> all = [];
            foreach (var file in _files)
            {
                all.AddRange(GetDataFromImg(file));
            }
            return all;
        }


        IEnumerable<AbstractTemplate> ProcessStringXml(StringCategory type, string fileType, XElement rootNode)
        {
            var intType = (int)(type);
            if (!Contains((int)type))
                InsertItem(new GenericKeyedTemplate<StringTemplate>((int)type));

            if (fileType == StringTemplateType.Eqp || fileType == StringTemplateType.Map)
            {
                foreach (var first in rootNode.Elements())
                {
                    foreach (var typeElement in first.Elements())
                    {
                        foreach (var itemElement in typeElement.Elements())
                        {
                            var data = SetStringTemplate(type, rootNode, itemElement);
                            if (data != null)
                                yield return data;
                        }
                    }
                }
            }
            else if (fileType == StringTemplateType.Etc)
            {
                foreach (var etc in rootNode.Elements())
                {
                    foreach (var itemElement in etc.Elements())
                    {
                        var data = SetStringTemplate(type, rootNode, itemElement);
                        if (data != null)
                            yield return data;
                    }
                }
            }
            else
            {
                foreach (var itemElement in rootNode.Elements())
                {
                    var data = SetStringTemplate(type, rootNode, itemElement);
                    if (data != null)
                        yield return data;
                }
            }
        }

        private StringTemplate? SetStringTemplate(StringCategory type, XElement rootNode, XElement item)
        {
            if (int.TryParse(item.Attribute("name")?.Value, out var id))
            {
                var template = new StringTemplate(id);
                foreach (var propNode in rootNode.Elements())
                {
                    var infoPropName = propNode.Attribute("name")?.Value ?? "";
                    var infoPropValue = propNode.Attribute("value")?.Value ?? "";
                    if (infoPropName == "name")
                        template.Name = infoPropValue;
                    else if (infoPropName == "desc")
                        template.Description = infoPropValue;
                    else if (infoPropName == "mapName")
                        template.MapName = infoPropValue;
                    else if (infoPropName == "streetName")
                        template.StreetName = infoPropValue;
                }
                this[type].Add(template);
                return template;
            }
            return null;
        }

        protected override IEnumerable<AbstractTemplate> GetDataFromImg(string path)
        {
            using var fis = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            var xDoc = XDocument.Load(fis).Root!;

            var fileType = xDoc.Attribute("name")?.Value;
            switch (fileType)
            {
                case StringTemplateType.Cash:
                case StringTemplateType.Consume:
                case StringTemplateType.Eqp:
                case StringTemplateType.Etc:
                case StringTemplateType.Ins:
                case StringTemplateType.Pet:
                    return ProcessStringXml(StringCategory.Item, fileType, xDoc);
                case StringTemplateType.Map:
                    return ProcessStringXml(StringCategory.Map, fileType, xDoc);
                case StringTemplateType.Mob:
                    return ProcessStringXml(StringCategory.Mob, fileType, xDoc);
                case StringTemplateType.Npc:
                    return ProcessStringXml(StringCategory.Npc, fileType, xDoc);
                case StringTemplateType.Skill:
                    return ProcessStringXml(StringCategory.Skill, fileType, xDoc);
                default:
                    return [];
            }
        }

        public GenericKeyedTemplate<StringTemplate> this[StringCategory key] => base[(int)key];
    }
}
