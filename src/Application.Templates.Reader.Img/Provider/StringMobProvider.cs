using Application.Templates.Reader;
using Application.Templates.String;
using System.Globalization;

namespace Application.Templates.Reader.Img.Provider
{
    internal class StringMobProvider : StringBaseProvider
    {
        public override ProviderType Type => ProviderType.StringMob;
        public StringMobProvider(CultureInfo cultureInfo, IWzPathResolver resolver) : base(cultureInfo, resolver)
        {
        }
    }
}
