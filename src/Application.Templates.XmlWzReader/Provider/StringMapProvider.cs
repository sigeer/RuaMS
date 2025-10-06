using Application.Templates.String;
using System.Globalization;
using System.Xml.Linq;

namespace Application.Templates.XmlWzReader.Provider
{
    public sealed class StringMapProvider : StringBaseProvider
    {
        public StringMapProvider(TemplateOptions options, CultureInfo cultureInfo) : base(options, cultureInfo, [StringTemplateType.Map])
        {
        }

        public override IEnumerable<AbstractTemplate> Search(string searchText, int maxCount = 50)
        {
            return LoadAll().OfType<StringMapTemplate>().Where(x => (x.StreetName != null && x.StreetName.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                || (x.MapName != null && x.MapName.Contains(searchText, StringComparison.OrdinalIgnoreCase)));
        }

        protected override AbstractTemplate? SetStringTemplate(XElement rootNode)
        {
            if (int.TryParse(rootNode.GetName(), out var id))
            {
                var template = new StringMapTemplate(id);
                foreach (var propNode in rootNode.Elements())
                {
                    var infoPropName = propNode.GetName();
                    if (infoPropName == "mapName")
                        template.MapName = propNode.GetStringValue() ?? Defaults.WZ_NoName;
                    else if (infoPropName == "streetName")
                        template.StreetName = propNode.GetStringValue() ?? Defaults.WZ_NoName;
                }
                InsertItem(template);
                return template;
            }
            return null;
        }
    }
}
