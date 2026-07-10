using Application.Templates.Reader;
using Application.Templates.String;
using Duey.Abstractions;
using System.Globalization;

namespace Application.Templates.Reader.Img.Provider
{
    internal class StringNpcProvider : StringBaseProvider
    {
        public override ProviderType Type => ProviderType.StringNpc;
        public StringNpcProvider(CultureInfo cultureInfo, IWzPathResolver resolver) : base(cultureInfo, resolver)
        {
        }

        protected override AbstractTemplate? SetStringTemplate(IDataNode rootNode)
        {
            if (int.TryParse(rootNode.Name, out var id))
            {
                var template = new StringNpcTemplate(id);
                foreach (var propNode in rootNode.Children)
                {
                    var infoPropName = propNode.Name;
                    if (string.IsNullOrEmpty(infoPropName))
                        continue;

                    if (infoPropName == "name")
                        template.Name = propNode.ResolveString() ?? WzDefaults.WZ_MissingNo;
                    else if (infoPropName == "d0")
                        template.DefaultTalk0 = propNode.ResolveString() ?? "(...)";
                    else if (infoPropName == "d1")
                        template.DefaultTalk1 = propNode.ResolveString() ?? "(...)";
                    else if (infoPropName == "func")
                        template.Func = propNode.ResolveString();
                }
                InsertItem(template);
                return template;
            }
            return null;
        }

        public override IEnumerable<AbstractTemplate> Search(string searchText, int maxCount = 50)
        {
            return LoadAll().OfType<StringNpcTemplate>()
                .Where(x => x.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                .Take(maxCount);
        }
    }
}
