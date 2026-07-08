using Application.Templates.Reader;
using Application.Templates.String;
using System.Globalization;

namespace Application.Templates.Reader.Xml.Provider
{
    public sealed class StringItemProvider : StringBaseProvider
    {
        public override ProviderType Type =>  ProviderType.StringItem;
        public StringItemProvider(IWzPathResolver resolver, CultureInfo cultureInfo) : base(cultureInfo, resolver)
        {
        }
    }
}
