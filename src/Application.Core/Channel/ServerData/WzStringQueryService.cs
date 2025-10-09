using Application.Templates.String;
using ZLinq;

namespace Application.Core.Channel.ServerData
{
    public class WzStringQueryService
    {
        public WzFindResult<WzFindMapResultItem> FindMapIdByName(IChannelClient c, string name)
        {
            var filtered = c.CurrentCulture.StringProvider.Search(StringCategory.Map, name).OfType<StringMapTemplate>()
                .Select(x => new WzFindMapResultItem(x.TemplateId, x.MapName!, x.StreetName!)).ToList();
            return new WzFindResult<WzFindMapResultItem>(filtered, name);
        }

        public WzFindResult<WzFindResultItem> FindItemIdByName(IChannelClient c, string name)
        {
            var list = c.CurrentCulture.StringProvider.Search(StringCategory.Item, name).OfType<StringTemplate>()
                .Select(x => new WzFindResultItem(x.TemplateId, x.Name)).ToList();
            return new WzFindResult<WzFindResultItem>(list, name);
        }

        public WzFindResult<WzFindResultItem> FindMobIdByName(IChannelClient c, string name)
        {
            var list = c.CurrentCulture.StringProvider.Search(StringCategory.Mob, name).OfType<StringTemplate>()
                .Select(x => new WzFindResultItem(x.TemplateId, x.Name)).ToList();
            return new WzFindResult<WzFindResultItem>(list, name);
        }
    }
}
