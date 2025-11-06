using Application.Templates.String;
using System.Globalization;

namespace Application.Templates.XmlWzReader.Provider
{
    internal class StringMobProvider : StringBaseProvider
    {
        public StringMobProvider(ProviderOption options, CultureInfo cultureInfo) : base(options, cultureInfo, [StringTemplateType.Mob])
        {
        }
    }
}
