using Application.Templates.String;
using System.Globalization;

namespace Application.Templates.XmlWzReader.Provider
{
    public sealed class StringItemProvider : StringBaseProvider
    {
        public StringItemProvider(ProviderOption options, CultureInfo cultureInfo) : base(options, cultureInfo, [
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
