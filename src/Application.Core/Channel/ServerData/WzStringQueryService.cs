using Application.Resources;
using ZLinq;

namespace Application.Core.Channel.ServerData
{
    public class WzStringQueryService
    {
        readonly IMapper _mapper;
        readonly WzStringProvider _wzStringProvider;
        public WzStringQueryService(IMapper mapper, WzStringProvider wzStringProvider)
        {
            _mapper = mapper;
            _wzStringProvider = wzStringProvider;
        }

        public WzFindResult<WzFindMapResultItem> FindMapIdByName(string name)
        {
            var filtered = _wzStringProvider.GetAllMap().AsValueEnumerable()
                .Where(x => x.PlaceName.Contains(name, StringComparison.OrdinalIgnoreCase) || x.StreetName.Contains(name, StringComparison.OrdinalIgnoreCase))
                .Select(x => new WzFindMapResultItem(x.Id, x.PlaceName, x.StreetName)).Take(50).ToList();
            return new WzFindResult<WzFindMapResultItem>(filtered, name);
        }

        public WzFindResult<WzFindResultItem> FindItemIdByName(string name)
        {
            var list = _wzStringProvider.GetAllItem().AsValueEnumerable()
                .Where(x => x.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
                .Select(x => new WzFindResultItem(x.Id, x.Name)).Take(50).ToList();
            return new WzFindResult<WzFindResultItem>(list, name);
        }

        Dictionary<int, ObjectName>? _mobNameCache;

        public WzFindResult<WzFindResultItem> FindMobIdByName(string name)
        {
            var list = _wzStringProvider.GetAllMonster().AsValueEnumerable()
                .Where(x => x.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
                .Select(x => new WzFindResultItem(x.Id, x.Name)).Take(50).ToList();
            return new WzFindResult<WzFindResultItem>(list, name);
        }
    }
}
