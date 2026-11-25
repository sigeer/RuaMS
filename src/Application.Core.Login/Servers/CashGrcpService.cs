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

        public override Task<ProtoService.CashShopStateResponse> GetCashShopState(ProtoService.CashShopStateRequest request, ServerCallContext context)
        {
            return Task.FromResult(_server.CashShopManager.GetState(request));
        }

        public override Task<ProtoService.GainCashResponse> GainCash(ProtoService.GainCashRequest request, ServerCallContext context)
        {
            return Task.FromResult(_server.CashShopManager.GainCash(request));
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

        public override Task<ProtoService.TakeCashItemResponse> TakeCashItem(ProtoService.TakeCashItemRequest request, ServerCallContext context)
        {
            return Task.FromResult(_server.CashShopManager.TakeCashItem(request));
        }

        public override Task<ProtoService.PutCashItemResponse> PutCashItem(ProtoService.PutCashItemRequest request, ServerCallContext context)
        {
            return Task.FromResult(_server.CashShopManager.PutCashItem(request));
        }

        public override Task<ProtoService.BuyQuestItemResponse> BuyQuestItem(ProtoService.BuyQuestItemRequest request, ServerCallContext context)
        {
            return Task.FromResult(_server.CashShopManager.BuyQuestItem(request));
        }

        public override Task<ProtoService.AddCashItemsResponse> AddCashItems(ProtoService.AddCashItemsRequest request, ServerCallContext context)
        {
            return Task.FromResult(_server.CashShopManager.AddCashItems(request));
        }

        public override Task<ProtoService.UseCashShopSurpriseResponse> UseCashShopSurprise(ProtoService.UseCashShopSurpriseRequest request, ServerCallContext context)
        {
            return Task.FromResult(_server.CashShopManager.UseCashShopSurprise(request));
        }

        public override Task<ProtoService.UpdateWishListResponse> UpdateWishList(ProtoService.UpdateWishListRequest request, ServerCallContext context)
        {
            return Task.FromResult(_server.CashShopManager.UpdateWishList(request));
        }

        public override Task<ProtoService.GainSlotResponse> GainInventorySlots(ProtoService.GainInventorySlotsRequest request, ServerCallContext context)
        {
            return Task.FromResult(_server.CashShopManager.GainInventorySlots(request));
        }

        public override Task<ProtoService.GainSlotResponse> GainStorageSlots(ProtoService.GainStorageSlotsRequest request, ServerCallContext context)
        {
            return Task.FromResult(_server.CashShopManager.GainStorageSlots(request));
        }

        public override Task<ProtoService.GainSlotResponse> GainCharacterSlots(ProtoService.GainCharacterSlotsRequest request, ServerCallContext context)
        {
            return Task.FromResult(_server.CashShopManager.GainCharacterSlots(request));
        }
    }
}
