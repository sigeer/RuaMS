using Application.Templates.Reader;
using Application.Templates.String;
using System.Globalization;

namespace Application.Templates.Reader.Xml.Provider
{
    internal class StringMobProvider : StringBaseProvider
    {
        public override ProviderType Type => ProviderType.StringMob;
        public StringMobProvider(IWzPathResolver resolver, CultureInfo cultureInfo) : base(cultureInfo, resolver)
        {
        }
    }
}
