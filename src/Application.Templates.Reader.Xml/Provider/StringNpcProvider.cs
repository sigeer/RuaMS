using Application.Templates.Reader;
using Application.Templates.String;
using System.Globalization;
using System.Xml.Linq;

namespace Application.Templates.Reader.Xml.Provider
{
    internal class StringNpcProvider : StringBaseProvider
    {
        public override ProviderType Type => ProviderType.StringNpc;
        public StringNpcProvider(IWzPathResolver resolver, CultureInfo cultureInfo) : base(cultureInfo, resolver)
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
                    if (infoPropName == null)
                        continue;

                    if (infoPropName == "name")
                        template.Name = propNode.GetStringValue() ?? WzDefaults.WZ_MissingNo;
                    else if (infoPropName == "d0")
                        template.DefaultTalk0 = propNode.GetStringValue() ?? "(...)";
                    else if (infoPropName == "d1")
                        template.DefaultTalk1 = propNode.GetStringValue() ?? "(...)";
                    else if (infoPropName == "func")
                        template.Func = propNode.GetStringValue();
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
