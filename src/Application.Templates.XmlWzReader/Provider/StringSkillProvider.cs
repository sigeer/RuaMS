using Application.Templates.String;

namespace Application.Templates.XmlWzReader.Provider
{
    public sealed class StringSkillProvider : StringBaseProvider
    {
        public StringSkillProvider(TemplateOptions options) : base(options, [StringTemplateType.Skill])
        {
        }
    }
}
