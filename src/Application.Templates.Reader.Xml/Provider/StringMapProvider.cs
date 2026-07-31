using Application.Templates.Reader;
using Application.Templates.String;
using System.Globalization;
using System.Xml.Linq;

namespace Application.Templates.Reader.Xml.Provider
{
    public sealed class StringMapProvider : StringBaseProvider
    {
        public override ProviderType Type => ProviderType.StringMap;
        public StringMapProvider(IWzPathResolver resolver, CultureInfo cultureInfo) : base(cultureInfo, resolver)
        {
        }

        public override IEnumerable<StringTemplateBase> Search(string searchText, int maxCount = 50)
        {
            return LoadAll().OfType<StringMapTemplate>().Where(x =>
                x.StreetName.Contains(searchText, StringComparison.OrdinalIgnoreCase)
                || x.MapName.Contains(searchText, StringComparison.OrdinalIgnoreCase)
                || x.TemplateId.ToString().Contains(searchText))
                .OrderByDescending(x => x.MapName == searchText)
                .ThenByDescending(x => x.StreetName == searchText)
                .ThenByDescending(x => x.TemplateId.ToString() == searchText)
                .Take(maxCount);
        }

        protected override StringTemplateBase? SetStringTemplate(XElement rootNode)
        {
            if (int.TryParse(rootNode.GetName(), out var id))
            {
                var template = new StringMapTemplate(id);
                foreach (var propNode in rootNode.Elements())
                {
                    var infoPropName = propNode.GetName();
                    if (infoPropName == "mapName")
                        template.MapName = propNode.GetStringValue() ?? WzDefaults.WZ_NoName;
                    else if (infoPropName == "streetName")
                        template.StreetName = propNode.GetStringValue() ?? WzDefaults.WZ_NoName;
                }
                InsertItem(template);
                return template;
            }
            return null;
        }
    }
}
