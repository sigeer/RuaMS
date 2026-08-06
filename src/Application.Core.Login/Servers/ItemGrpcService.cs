using Application.Core.Login.Services;
using ItemService = Application.Core.Login.Services.ItemService;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace Application.Core.Login.Servers
{
    internal class ItemGrpcService : ProtoService.ItemService.ItemServiceBase
    {
        readonly MasterServer _server;
        readonly ItemService _itemService;

        public ItemGrpcService(MasterServer server, ItemService itemService)
        {
            _server = server;
            _itemService = itemService;
        }

        public override Task<ProtoService.OwlSearchRecordResponse> LoadOwlSearchRecords(Empty request, ServerCallContext context)
        {
            return Task.FromResult(_server.PlayerShopManager.GetOwlSearchedItems());
        }

        public override Task<ProtoService.OwlSearchResponse> UseOwlSearch(ProtoService.OwlSearchRequest request, ServerCallContext context)
        {
            return Task.FromResult(_server.PlayerShopManager.OwlSearch(request));
        }

        public override Task<ProtoService.CreateTVMessageResponse> UseTVMessage(ProtoService.CreateTVMessageRequest request, ServerCallContext context)
        {
            return Task.FromResult(_itemService.BroadcastTV(request));
        }
    }
}
