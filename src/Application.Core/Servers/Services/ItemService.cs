using Application.Core.Game.Items;
using Application.Core.ServerTransports;
using Application.Shared.Characters;
using AutoMapper;
using client.inventory;
using Microsoft.Extensions.Logging;
using server;

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
    }
}
