using Application.Templates.Reader;
using Application.Templates.String;
using Duey.Abstractions;
using System.Globalization;

namespace Application.Templates.Reader.Img.Provider
{
    public sealed class StringSkillProvider : StringBaseProvider
    {
        public override ProviderType Type => ProviderType.StringSkill;
        public StringSkillProvider(CultureInfo cultureInfo, IWzPathResolver resolver) : base(cultureInfo, resolver)
        {
        }

        protected override AbstractTemplate? SetStringTemplate(IDataNode rootNode)
        {
            var skillIdStr = rootNode.Name;
            if (string.IsNullOrEmpty(skillIdStr) || skillIdStr.Length < 7)
                return null;

            if (int.TryParse(skillIdStr, out var id))
            {
                var template = new StringTemplate(id);
                foreach (var propNode in rootNode.Children)
                {
                    var infoPropName = propNode.Name;
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
