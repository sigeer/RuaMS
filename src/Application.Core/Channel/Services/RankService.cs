using Application.Core.ServerTransports;
using Microsoft.Extensions.Caching.Memory;

namespace Application.Core.Servers.Services
{
    public class RankService
    {
        readonly IChannelServerTransport _transport;
        readonly IMemoryCache _cache;
        readonly IMapper _mapper;

        public RankService(IChannelServerTransport transport, IMemoryCache cache, IMapper mapper)
        {
            _transport = transport;
            _cache = cache;
            _mapper = mapper;
        }

        public List<RankedCharacterInfo> LoadPlayerRanking(int topCount = 50)
        {
            return _cache.GetOrCreate("Rank", e =>
            {
                e.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                var remote = _mapper.Map<List<RankedCharacterInfo>>(_transport.LoadPlayerRanking(topCount).List);
                return remote;
            }) ?? [];
            ;
        }
    }
}
