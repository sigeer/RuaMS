using Application.Core.Channel;
using Application.Core.Game.Trades;
using Application.Core.Model;
using Application.Core.ServerTransports;
using Application.Shared.Items;
using AutoMapper;
using client.autoban;
using client.inventory;
using client.inventory.manipulator;
using client.processor.npc;
using Microsoft.Extensions.Logging;
using server;
using tools;

namespace Application.Core.Servers.Services
{
    public class ItemService
    {
        readonly IMapper _mapper;
        readonly IChannelServerTransport _transport;
        readonly ILogger<ItemService> _logger;
        readonly WorldChannelServer _server;

        public ItemService(IMapper mapper, IChannelServerTransport transport, ILogger<ItemService> logger, WorldChannelServer server)
        {
            _mapper = mapper;
            _transport = transport;
            _logger = logger;
            _server = server;
        }

        public List<SpecialCashItem> GetSpecialCashItems()
        {
            return _mapper.Map<List<SpecialCashItem>>(_transport.RequestSpecialCashItems().Items);
        }

        public GiftModel[] LoadPlayerGifts(int id)
        {
            var remoteData = _transport.LoadPlayerGifts(id);
            return _mapper.Map<GiftModel[]>(remoteData);
        }

        internal Shop GetShop(int id, bool isShopId)
        {
            return _mapper.Map<Shop>(_transport.GetShop(id, isShopId));
        }
    }
}
