using Application.Templates.String;

namespace Application.Templates.XmlWzReader.Provider
{
    public sealed class StringItemProvider : StringBaseProvider
    {
        public StringItemProvider(TemplateOptions options) : base(options, [
            StringTemplateType.Cash,
            StringTemplateType.Consume,
            StringTemplateType.Eqp,
            StringTemplateType.Etc,
            StringTemplateType.Pet,
            StringTemplateType.Ins])
        {
        }
    }
}
