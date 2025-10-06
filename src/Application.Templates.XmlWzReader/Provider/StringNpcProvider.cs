using Application.Templates.String;
using System.Globalization;
using System.Xml.Linq;

namespace Application.Templates.XmlWzReader.Provider
{
    internal class StringNpcProvider : StringBaseProvider
    {
        public StringNpcProvider(TemplateOptions options, CultureInfo cultureInfo) : base(options, cultureInfo, [StringTemplateType.Npc])
        {
        }

        protected override AbstractTemplate? SetStringTemplate(XElement rootNode)
        {
            if (int.TryParse(rootNode.GetName(), out var id))
            {
                var template = new StringNpcTemplate(id);
                foreach (var propNode in rootNode.Elements())
                {
                    var infoPropName = propNode.GetName();
                    if (infoPropName == "name")
                        template.Name = propNode.GetStringValue() ?? Defaults.WZ_MissingNo;
                    else if (infoPropName == "d0")
                        template.DefaultTalk = propNode.GetStringValue() ?? "(...)";
                }
                InsertItem(template);
                return template;
            }
            return null;
        }

        public override IEnumerable<AbstractTemplate> Search(string searchText, int maxCount = 50)
        {
            return LoadAll().OfType<StringNpcTemplate>().Where(x => x.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase)).Take(maxCount);
        }
    }
}
