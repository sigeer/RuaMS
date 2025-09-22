using Application.Templates.Providers;
using Application.Templates.String;
using Application.Templates.XmlWzReader.Provider;
using ZLinq;

namespace Application.Core.Channel.ServerData
{
    public class WzStringQueryService
    {
        readonly StringProvider _stringProvider = ProviderFactory.GetProvider<StringProvider>();
        public WzFindResult<WzFindMapResultItem> FindMapIdByName(string name)
        {
            var filtered = _stringProvider.Search(StringCategory.Map, name).OfType<StringMapTemplate>()
                .Select(x => new WzFindMapResultItem(x.TemplateId, x.MapName!, x.StreetName!)).ToList();
            return new WzFindResult<WzFindMapResultItem>(filtered, name);
        }

        public WzFindResult<WzFindResultItem> FindItemIdByName(string name)
        {
            var list = _stringProvider.Search(StringCategory.Item, name).OfType<StringTemplate>()
                .Select(x => new WzFindResultItem(x.TemplateId, x.Name)).ToList();
            return new WzFindResult<WzFindResultItem>(list, name);
        }

        public WzFindResult<WzFindResultItem> FindMobIdByName(string name)
        {
            var list = _stringProvider.Search(StringCategory.Mob, name).OfType<StringTemplate>()
                .Select(x => new WzFindResultItem(x.TemplateId, x.Name)).ToList();
            return new WzFindResult<WzFindResultItem>(list, name);
        }
    }
}
