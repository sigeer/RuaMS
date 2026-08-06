using Application.Templates.Reader;
using Application.Templates.String;
using Duey.Abstractions;
using System.Globalization;

namespace Application.Templates.Reader.Img.Provider
{
    public sealed class StringMapProvider : StringBaseProvider
    {
        public override ProviderType Type => ProviderType.StringMap;
        public StringMapProvider(CultureInfo cultureInfo, IWzPathResolver resolver) : base(cultureInfo, resolver)
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

        protected override StringTemplateBase? SetStringTemplate(IDataNode rootNode)
        {
            if (int.TryParse(rootNode.Name, out var id))
            {
                var template = new StringMapTemplate(id);
                foreach (var propNode in rootNode.Children)
                {
                    var infoPropName = propNode.Name;
                    if (infoPropName == "mapName")
                        template.MapName = propNode.ResolveString() ?? WzDefaults.WZ_NoName;
                    else if (infoPropName == "streetName")
                        template.StreetName = propNode.ResolveString() ?? WzDefaults.WZ_NoName;
                }
                InsertItem(template);
                return template;
            }
            return null;
        }
    }
}
