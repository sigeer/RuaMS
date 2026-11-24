using Grpc.Core;

namespace Application.Core.Login.Servers
{
    /// <summary>
    /// 拍卖行（MTS / ITC）的跨服接口实现
    /// </summary>
    internal class MTSGrpcService : ProtoService.MTSService.MTSServiceBase
    {
        readonly MasterServer _server;

        public MTSGrpcService(MasterServer server)
        {
            _server = server;
        }

        public override Task<ProtoService.ChangePageResponse> ChangePage(ProtoService.ChangePageRequest request, ServerCallContext context)
        {
            return Task.FromResult(_server.MTSManager.ChangePage(request));
        }

        public override Task<ProtoService.SaleItemResponse> SaleItem(ProtoService.SaleItemRequest request, ServerCallContext context)
        {
            return Task.FromResult(_server.MTSManager.SaleItem(request));
        }

        public override Task<ProtoService.CancelSaleItemResponse> CancelSaleItem(ProtoService.CancelSaleItemRequest request, ServerCallContext context)
        {
            return Task.FromResult(_server.MTSManager.CancelSaleItem(request));
        }

        public override Task<ProtoService.AddItemToCartResponse> AddItemToCart(ProtoService.AddItemToCartRequest request, ServerCallContext context)
        {
            return Task.FromResult(_server.MTSManager.AddItemToCart(request));
        }

        public override Task<ProtoService.RemoveItemFromCartResponse> RemoveItemFromCart(ProtoService.RemoveItemFromCartRequest request, ServerCallContext context)
        {
            return Task.FromResult(_server.MTSManager.RemoveItemFromCart(request));
        }

        public override Task<ProtoService.TakeItemResponse> TakeItem(ProtoService.TakeItemRequest request, ServerCallContext context)
        {
            return Task.FromResult(_server.MTSManager.TakeItem(request));
        }

        public override Task<ProtoService.BuyResponse> Buy(ProtoService.BuyRequest request, ServerCallContext context)
        {
            return Task.FromResult(_server.MTSManager.Buy(request));
        }

        public override Task<ProtoService.MTSOperationResponse> RegisterWanted(ProtoService.WantedRequest request, ServerCallContext context)
        {
            return Task.FromResult(_server.MTSManager.RegisterWanted(request));
        }

        public override Task<ProtoService.MTSOperationResponse> CancelWanted(ProtoService.ProductRequest request, ServerCallContext context)
        {
            return Task.FromResult(_server.MTSManager.CancelWanted(request));
        }

        public override Task<ProtoService.MTSOperationResponse> FulfillWanted(ProtoService.FulfillWantedRequest request, ServerCallContext context)
        {
            return Task.FromResult(_server.MTSManager.FulfillWanted(request));
        }

        public override Task<ProtoService.MTSOperationResponse> LoadWanted(ProtoService.ProductRequest request, ServerCallContext context)
        {
            return Task.FromResult(_server.MTSManager.LoadWanted(request));
        }

        public override Task<ProtoService.SaleItemResponse> RegisterAuction(ProtoService.SaleItemRequest request, ServerCallContext context)
        {
            return Task.FromResult(_server.MTSManager.RegisterAuction(request));
        }

        public override Task<ProtoService.MTSOperationResponse> BidAuction(ProtoService.BidRequest request, ServerCallContext context)
        {
            return Task.FromResult(_server.MTSManager.BidAuction(request));
        }
    }
}
