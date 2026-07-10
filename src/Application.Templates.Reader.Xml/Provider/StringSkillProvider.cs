using Application.Templates.Reader;
using Application.Templates.String;
using System.Globalization;
using System.Xml.Linq;

namespace Application.Templates.Reader.Xml.Provider
{
    public sealed class StringSkillProvider : StringBaseProvider
    {
        public override ProviderType Type => ProviderType.StringSkill;
        public StringSkillProvider(IWzPathResolver resolver, CultureInfo cultureInfo) : base(cultureInfo, resolver)
        {
        }

        protected override AbstractTemplate? SetStringTemplate(XElement rootNode)
        {
            var skillIdStr = rootNode.GetName();
            if (string.IsNullOrEmpty(skillIdStr) || skillIdStr.Length < 7)
                return null;

            if (int.TryParse(skillIdStr, out var id))
            {
                var template = new StringTemplate(id);
                foreach (var propNode in rootNode.Elements())
                {
                    var infoPropName = propNode.GetName();
                    if (infoPropName == "name")
                        template.Name = propNode.GetStringValue() ?? "";
                }
                InsertItem(template);
                return template;
            }
            return null;
        }
    }
}
