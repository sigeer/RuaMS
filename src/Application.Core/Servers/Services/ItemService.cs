using Application.Core.Model;
using Application.Core.ServerTransports;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace Application.Core.Servers.Services
{
    public class ItemService
    {
        readonly IMapper _mapper;
        readonly IChannelServerTransport _transport;
        readonly ILogger<ItemService> _logger;

        public ItemService(IMapper mapper, IChannelServerTransport transport, ILogger<ItemService> logger)
        {
            _mapper = mapper;
            _transport = transport;
            _logger = logger;
        }

        public GiftModel[] LoadPlayerGifts(int id)
        {
            var remoteData = _transport.LoadPlayerGifts(id);
            return _mapper.Map<GiftModel[]>(remoteData);
        }
    }
}
