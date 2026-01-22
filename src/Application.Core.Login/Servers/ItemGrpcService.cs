using Application.Core.Login.Services;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using ItemProto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Core.Login.Servers
{
    internal class ItemGrpcService: ServiceProto.ItemService.ItemServiceBase
    {
        readonly MasterServer _server;
        readonly ItemService _itemService;

        public ItemGrpcService(MasterServer server, ItemService itemService)
        {
            _server = server;
            _itemService = itemService;
        }

        public override Task<LoadItemsFromStoreResponse> LoadItemFromStore(LoadItemsFromStoreRequest request, ServerCallContext context)
        {
            return Task.FromResult(_server.ItemFactoryManager.LoadItems(request));
        }

        public override Task<OwlSearchRecordResponse> LoadOwlSearchRecords(Empty request, ServerCallContext context)
        {
            return Task.FromResult(_server.PlayerShopManager.GetOwlSearchedItems());
        }

        public override Task<StoreItemsResponse> SaveItems(StoreItemsRequest request, ServerCallContext context)
        {
            return Task.FromResult(_server.ItemFactoryManager.Store(request));
        }

        public override Task<OwlSearchResponse> UseOwlSearch(OwlSearchRequest request, ServerCallContext context)
        {
            return Task.FromResult(_server.PlayerShopManager.OwlSearch(request));
        }

        public override async Task<CreateTVMessageResponse> UseTVMessage(CreateTVMessageRequest request, ServerCallContext context)
        {
            return await _itemService.BroadcastTV(request);
        }
    }
}
