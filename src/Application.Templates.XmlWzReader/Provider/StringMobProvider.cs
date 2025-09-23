using Application.Templates.String;

namespace Application.Templates.XmlWzReader.Provider
{
    internal class StringMobProvider : StringBaseProvider
    {
        public StringMobProvider(TemplateOptions options) : base(options, [StringTemplateType.Mob])
        {
        }
    }
}
