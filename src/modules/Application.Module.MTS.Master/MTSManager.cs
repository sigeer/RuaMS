using Application.Core.Login;
using Application.Core.Login.Models;
using Application.Core.Login.Shared;
using Application.EF;
using Application.Module.MTS.Common;
using Application.Module.MTS.Master.Models;
using Application.Shared.Items;
using Application.Utility;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MTSProto;
using System.Collections.Concurrent;

namespace Application.Module.MTS.Master
{
    public class MTSManager : StorageBase<int, UpdateField<MTSProductModel>>
    {
        readonly ILogger<MTSManager> _logger;
        readonly IDbContextFactory<DBContext> _dbContextFactory;
        readonly MasterTransport _transport;
        readonly IMapper _mapper;
        readonly MasterServer _server;


        ConcurrentDictionary<int, MTSProductModel> _dataSource = new();
        ConcurrentDictionary<int, MTSCartModel> _cartData = new();
        int _localProductId = 0;

        public MTSManager(ILogger<MTSManager> logger, IDbContextFactory<DBContext> dbContextFactory, MasterTransport transport, IMapper mapper, MasterServer server)
        {
            _logger = logger;
            _dbContextFactory = dbContextFactory;
            _transport = transport;
            _mapper = mapper;
            _server = server;
        }

        private MTSProto.PlayerMTSInfo LoadMyData(int sellerId)
        {
            var all = _dataSource.Values.Where(x => x.OwnerId == sellerId);
            var res = new PlayerMTSInfo();
            res.InTransfer.AddRange(_mapper.Map<MTSProto.MTSItemDto[]>(all.Where(x => x.IsInTransfer)));
            res.OnSale.AddRange(_mapper.Map<MTSProto.MTSItemDto[]>(all.Where(x => !x.IsInTransfer)));
            return res;
        }

        private MTSProto.PagedItems LoadPageData(int tab, int type, int page)
        {
            var all = _dataSource.Values.Where(x => !x.IsInTransfer && x.Tab == tab && (type == 0 || type == x.Type))
                .OrderByDescending(x => x.Id);

            var pagedData = all
                .Skip(page * 16)
                .Take(16)
                .ToList();
            var totalPages = (int)Math.Ceiling(all.Count() / 16.0f);
            var res = new PagedItems()
            {
                TotalPages = totalPages,
                Page = page,
                Tab = tab,
                Type = type,
            };
            res.Items.AddRange(_mapper.Map<MTSProto.MTSItemDto[]>(all));
            return res;
        }

        private MTSProto.PagedItems LoadCartData(int ownerId)
        {
            var allItems = _cartData.Values.Where(x => x.PlayerId == ownerId).SelectMany(x => x.Products).ToArray();
            var all = _dataSource.Values.Where(x => !x.IsInTransfer && allItems.Contains(x.Id))
                .OrderByDescending(x => x.Id);

            var pagedData = all
                .Take(16)
                .ToList();
            var totalPages = (int)Math.Ceiling(all.Count() / 16.0f);
            var res = new PagedItems()
            {
                TotalPages = totalPages,
                Tab = 4,
                Page = 0,
                Type = 0,
            };
            res.Items.AddRange(_mapper.Map<MTSProto.MTSItemDto[]>(all));
            return res;
        }
        public MTSProto.ChangePageResponse ChangePage(MTSProto.ChangePageRequest request)
        {
            var query = _dataSource.Values.Where(x => !x.IsInTransfer);
            if (request.Tab == 4 && request.Type == 0)
            {
                var cartItems = _cartData.GetValueOrDefault(request.MasterId)?.Products ?? [];
                query = query.Where(x => cartItems.Contains(x.Id)).OrderByDescending(x => x.Id);
            }
            else if (!string.IsNullOrEmpty(request.SearchText))
            {
                query = query.Where(x => x.Tab == request.Tab && (request.Type == 0 || request.Type == x.Type));
                if (request.SearchType == 0)
                {
                    //var sellerIds = _server.CharacterManager.FindSmiliar(request.SearchText);
                    //query = query.Where(x => sellerIds.Contains(x.OwnerId));
                }
                else
                {
                    query = query.Where(x => request.FilterItemId.Contains(x.Item.Itemid));
                }
            }

            var totalPages = (int)Math.Ceiling(query.Count() / 16.0f);
            var pagedData = query.Skip(request.Page * 16).Take(16).ToList();
            var res = new ChangePageResponse
            {
                Type = request.Type,
                Tab = request.Tab,
                Page = request.Page,
                MasterId = request.MasterId,
                MyMTSInfo = LoadMyData(request.MasterId),
                TotalPages = totalPages
            };
            res.Items.AddRange(_mapper.Map<MTSProto.MTSItemDto[]>(pagedData));
            return res;
        }

        public MTSProto.SaleItemResponse AddItemToSale(MTSProto.SaleItemRequest request)
        {
            var count = _dataSource.Values.Where(x => x.OwnerId == request.MasterId).Count();
            if (count > 10)
            {
                return new MTSProto.SaleItemResponse
                {
                    Code = 1,
                    Transaction = _server.ItemTransactionManager.CreateTransaction(request.Transaction, ItemTransactionStatus.PendingForRollback)
                };
            }

            var product = new MTSProductModel
            {
                Id = Interlocked.Increment(ref _localProductId),
                OwnerId = request.MasterId,
                IsInTransfer = false,
                Tab = 1,
                Type = 0,
                Price = request.Price,
                Item = _mapper.Map<ItemModel>(request.Item)
            };
            _dataSource[product.Id] = product;

            return new MTSProto.SaleItemResponse
            {
                MasterId = request.MasterId,
                Transaction = _server.ItemTransactionManager.CreateTransaction(request.Transaction, ItemTransactionStatus.PendingForCommit),
                MyMTSInfo = LoadMyData(request.MasterId),
                PageData = LoadPageData(1, 0, 0)
            };
        }

        public CancelSaleItemResponse CancelMtsSale(CancelSaleItemRequest request)
        {
            if (_dataSource.TryGetValue(request.ProductId, out var product))
            {
                if (product.OwnerId != request.MasterId)
                {
                    return new CancelSaleItemResponse { Code = (int)MTSResponseCode.NoAccess };
                }
                product.IsInTransfer = true;
                return new CancelSaleItemResponse();
            }
            return new CancelSaleItemResponse() { Code = (int)MTSResponseCode.NotFound };
        }

        public void AddToCart(AddItemToCartRequest request)
        {
            if (_cartData.TryGetValue(request.Query.MasterId, out var cart))
            {
                cart.Products.Add(request.ProductId);

            }
            else
            {
                _cartData[request.Query.MasterId] = new MTSCartModel();
            }
        }

        public void DeleteCart(RemoveItemFromCartRequest request)
        {
            if (_cartData.TryGetValue(request.Query.MasterId, out var cart))
            {
                cart.Products.Remove(request.ProductId);
            }
        }

        public BuyResponse Buy(BuyRequest request)
        {
            if (_dataSource.TryGetValue(request.ProductId, out var product))
            {
                if (!product.IsInTransfer)
                {
                    return new BuyResponse { Code = (int)MTSResponseCode.NotFound };
                }

                if (product.OwnerId == request.Query.MasterId)
                {
                    new BuyResponse { Code = (int)MTSResponseCode.Buy_CannotBuyOwn };
                }

                var chr = _server.CharacterManager.FindPlayerById(request.Query.MasterId)!;
                var accData = _server.AccountManager.GetAccountGameData(chr.Character.AccountId)!;
                var finalCost = product.Price + 100 + (int)(product.Price * 0.1);

                if (accData.NxPrepaid - finalCost < 0)
                {
                    return new BuyResponse { Code = (int)MTSResponseCode.Buy_CostNotEnough };
                }

                var owner = _server.CharacterManager.FindPlayerById(product.OwnerId)!;
                var ownerAccData = _server.AccountManager.GetAccountGameData(owner.Character.AccountId)!;

                product.IsInTransfer = true;
                product.OwnerId = chr.Character.Id;

                accData.NxPrepaid -= product.Price;
                ownerAccData.NxPrepaid += product.Price;

                _server.AccountManager.UpdateAccountGame(accData);
                _server.AccountManager.UpdateAccountGame(ownerAccData);

                return new BuyResponse
                {
                    Item = _mapper.Map<Dto.ItemDto>(product.Item),
                    MasterId = request.Query.MasterId,
                    CartItems = LoadCartData(request.Query.MasterId),
                    MyMTSInfo = LoadMyData(request.Query.MasterId),
                    PageData = LoadPageData(request.Query.Tab, request.Query.Type, request.Query.Page)
                };
            }
            return new BuyResponse { Code = (int)MTSResponseCode.NotFound };
        }

        protected override Task CommitInternal(DBContext dbContext, Dictionary<int, UpdateField<MTSProductModel>> updateData)
        {
            throw new NotImplementedException();
        }
    }
}
