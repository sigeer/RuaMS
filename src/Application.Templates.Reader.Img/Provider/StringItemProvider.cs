using Application.Templates.Reader;
using Application.Templates.String;
using System.Globalization;

namespace Application.Templates.Reader.Img.Provider
{
    public sealed class StringItemProvider : StringBaseProvider
    {
        public override ProviderType Type => ProviderType.StringItem;
        public StringItemProvider(CultureInfo cultureInfo, IWzPathResolver resolver) : base(cultureInfo, resolver)
        {
        }
    }
}
