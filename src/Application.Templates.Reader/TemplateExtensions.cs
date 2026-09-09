using Application.Templates.Map;

namespace Application.Templates.Reader
{
    public static class TemplateExtensions
    {
        public static int GetMapAreaCode(this MapTemplate mapTemplate)
        {
            var type = mapTemplate.TemplateId / 10000000;
            return ProviderSource.Instance.GetProvider<IProvider<MapAreaCodeTemplate>>(ProviderType.MapAreaCode).GetItem(type)?.Code ?? 0;
        }
    }
}
