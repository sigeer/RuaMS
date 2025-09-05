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


        protected override void LoadAllInternal()
        {
            foreach (var file in _files)
            {
                using var fis = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
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
                        ProcessStringXml(StringCategory.Item, fileType, xDoc);
                        break;
                    case StringTemplateType.Map:
                        ProcessStringXml(StringCategory.Map, fileType, xDoc);
                        break;
                    case StringTemplateType.Mob:
                        ProcessStringXml(StringCategory.Mob, fileType, xDoc);
                        break;
                    case StringTemplateType.Npc:
                        ProcessStringXml(StringCategory.Npc, fileType, xDoc);
                        break;
                    case StringTemplateType.Skill:
                        ProcessStringXml(StringCategory.Skill, fileType, xDoc);
                        break;
                    default:
                        break;
                }
            }
        }

        void ProcessStringXml(StringCategory type, string fileType, XElement rootNode)
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
                            SetStringTemplate(type, rootNode, itemElement);
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
                        SetStringTemplate(type, rootNode, itemElement);
                    }
                }
            }
            else
            {
                foreach (var itemElement in rootNode.Elements())
                {
                    SetStringTemplate(type, rootNode, itemElement);
                }
            }
        }

        private void SetStringTemplate(StringCategory type, XElement rootNode, XElement item)
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
            }
        }

        public GenericKeyedTemplate<StringTemplate> this[StringCategory key] => base[(int)key];
    }
}
