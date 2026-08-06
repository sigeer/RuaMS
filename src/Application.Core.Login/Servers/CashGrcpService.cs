using Application.Core.Login.Services;
using ItemService = Application.Core.Login.Services.ItemService;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace Application.Core.Login.Servers
{
    internal class CashGrcpService : ProtoService.CashService.CashServiceBase
    {
        readonly MasterServer _server;
        readonly ItemService _itemService;

        public CashGrcpService(MasterServer server, ItemService itemService)
        {
            _server = server;
            _itemService = itemService;
        }

        public override Task<ProtoService.BuyCashItemResponse> BuyCashItem(ProtoService.BuyCashItemRequest request, ServerCallContext context)
        {
            return Task.FromResult(_server.CashShopDataManager.BuyCashItem(request));
        }

        public override Task<Empty> CommitRetrieveGift(ProtoService.CommitRetrieveGiftRequest request, ServerCallContext context)
        {
            _server.GiftManager.CommitRetrieveGift(request.IdList.ToArray());
            return Task.FromResult(new Empty());
        }

        public override Task<ProtoService.GetMyGiftsResponse> LoadGifts(ProtoService.GetMyGiftsRequest request, ServerCallContext context)
        {
            return Task.FromResult(_server.GiftManager.LoadGifts(request));
        }

        public override Task<ProtoModel.MosterSellerInfo> LoadMosterSellItems(Empty request, ServerCallContext context)
        {
            var res = new ProtoModel.MosterSellerInfo();
            var all = _server.CashShopDataManager.GetMostSellerCashItems();
            foreach (var item in all)
            {
                var tab = new ProtoModel.MonsterSellerTab();
                tab.ItemIdList.AddRange(item);
                res.Tabs.Add(tab);
            }
            return Task.FromResult(res);
        }

        public override Task<ProtoModel.SpecialCashItemListProto> LoadSpecialItems(Empty request, ServerCallContext context)
        {
            return Task.FromResult(_itemService.LoadSpecialCashItems());
        }
    }
}
