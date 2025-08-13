using Application.Core.Login.Services;
using CashProto;
using Google.Protobuf;
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
    internal class CashGrcpService : ServiceProto.CashService.CashServiceBase
    {
        readonly MasterServer _server;
        readonly ItemService _itemService;

        public CashGrcpService(MasterServer server, ItemService itemService)
        {
            _server = server;
            _itemService = itemService;
        }

        public override Task<BuyCashItemResponse> BuyCashItem(BuyCashItemRequest request, ServerCallContext context)
        {
            return Task.FromResult(_server.CashShopDataManager.BuyCashItem(request));
        }

        public override Task<Empty> CommitRetrieveGift(CommitRetrieveGiftRequest request, ServerCallContext context)
        {
            _server.GiftManager.CommitRetrieveGift(request.IdList.ToArray());
            return Task.FromResult(new Empty());
        }

        public override Task<GetMyGiftsResponse> LoadGifts(GetMyGiftsRequest request, ServerCallContext context)
        {
            return Task.FromResult(_server.GiftManager.LoadGifts(request));
        }

        public override Task<MosterSellerInfo> LoadMosterSellItems(Empty request, ServerCallContext context)
        {
            var res = new MosterSellerInfo();
            var all  = _server.CashShopDataManager.GetMostSellerCashItems();
            foreach (var item in all)
            {
                var tab = new MonsterSellerTab();
                tab.ItemIdList.AddRange(item);
                res.Tabs.Add(tab);
            }
            return Task.FromResult(res);
        }

        public override Task<SpecialCashItemListDto> LoadSpecialItems(Empty request, ServerCallContext context)
        {
            return Task.FromResult(_itemService.LoadSpecialCashItems());
        }
    }
}
