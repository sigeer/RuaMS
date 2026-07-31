using Application.Core.Login.Dtos.Shop;
using Application.EF;
using Application.EF.Entities;
using Application.Shared.Constants.Item;
using Application.Shared.Message;
using Application.Templates.Reader;
using Application.Templates.String;
using Application.Utility.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SQLitePCL;

namespace Application.Core.Login.Services
{
    public class ShopManager
    {
        readonly IMapper _mapper;
        readonly ILogger<ShopManager> _logger;
        readonly IDbContextFactory<DBContext> _dbContextFactory;
        readonly MasterServer _server;

        public bool IsDirty { get; private set; }

        public ShopManager(IMapper mapper, ILogger<ShopManager> logger, IDbContextFactory<DBContext> dbContextFactory, MasterServer server)
        {
            _mapper = mapper;
            _logger = logger;
            _dbContextFactory = dbContextFactory;
            _server = server;
        }

        public Dto.ShopDto? LoadFromDB(int id, bool isShopId)
        {
            Dto.ShopDto? ret = null;
            int shopId;
            try
            {
                using var dbContext = _dbContextFactory.CreateDbContext();
                ShopEntity? tmpModel = null;
                if (isShopId)
                {
                    tmpModel = dbContext.Shops.Where(x => x.ShopId == id).FirstOrDefault();
                }
                else
                {
                    tmpModel = dbContext.Shops.Where(x => x.NpcId == id).FirstOrDefault();
                }

                if (tmpModel != null)
                {
                    shopId = tmpModel.ShopId;
                    ret = _mapper.Map<Dto.ShopDto>(tmpModel);
                }
                else
                {
                    return null;
                }

                var items = new List<Dto.ShopItemDto>();
                var shopItems = dbContext.Shopitems.Where(x => x.Shopid == shopId).OrderByDescending(x => x.Position).ToList();
                shopItems.ForEach(x =>
                {
                    if (ItemConstants.isRechargeable(x.ItemId))
                    {
                        var m = _mapper.Map<Dto.ShopItemDto>(x);
                        m.Buyable = 1;
                        items.Add(m);
                    }
                    else
                    {
                        var m = _mapper.Map<Dto.ShopItemDto>(x);
                        m.Buyable = 1000;
                        items.Add(m);
                    }
                });
                ret.Items.AddRange(items);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
            }
            return ret;
        }

        #region Console
        public (List<ShopResponseDto>, int Count) GetPagedData(int? itemId, int pageIndex, int pageSize, string locale)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var shops = dbContext.Shops.AsNoTracking();
            if (itemId != null)
            {
                var filteredShopIdList = dbContext.Shopitems.Where(x => x.ItemId == itemId).Select(x => x.Shopid).ToList();
                shops = shops.Where(x => filteredShopIdList.Contains(x.ShopId));
            }
            var shopList = shops.ProjectToType<ShopResponseDto>().ToPage(pageIndex, pageSize).ToList();
            foreach (var item in shopList)
            {
                item.NpcName = ProviderSource.Instance.GetProviderByKey<IStringProvider>(locale).GetSubProvider(Templates.String.StringCategory.Npc)
                    ?.GetItem(item.NpcId)?.Name ?? "";
            }
            return (shopList, shops.Count());
        }

        public async Task<ShopDetailDto?> GetShopItems(int shopId, string locale)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var shop = await dbContext.Shops.FirstOrDefaultAsync(x => x.ShopId == shopId);
            if (shop == null)
            {
                return null;
            }
            var detail = _mapper.Map<ShopDetailDto>(shop);

            var items = await dbContext.Shopitems.Where(x => x.Shopid == shopId).ProjectToType<ShopItemResponseDto>().ToListAsync();
            foreach (var item in items)
            {
                item.ItemName = ProviderSource.Instance.GetProviderByKey<IStringProvider>(locale).GetSubProvider(Templates.String.StringCategory.Item)
                    ?.GetItem(item.ItemId)?.Name ?? "";
            }
            detail.Items = items;
            return detail;
        }


        public void SubmitShop(CreateShopRequestDto model)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();

            var dbModel = dbContext.Shops.FirstOrDefault(x => x.ShopId == model.ShopId);
            if (dbModel == null)
            {
                dbModel = new ShopEntity(model.NpcId);
                dbContext.Shops.Add(dbModel);
            }
            else
            {
                dbModel.NpcId = model.NpcId;
            }
            dbContext.SaveChanges();

            IsDirty = true;
        }

        public async Task DeleteShop(int id)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            await dbContext.Shops.Where(x => x.ShopId == id).ExecuteDeleteAsync();
            await dbContext.Shopitems.Where(x => x.Shopid == id).ExecuteDeleteAsync();

            IsDirty = true;
        }

        public async Task SubmitShopItem(EditShopItemRequestDto model)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();

            var dbModel = dbContext.Shopitems.FirstOrDefault(x => x.Shopitemid == model.Id);
            if (dbModel == null)
            {
                dbModel = new ShopItemEntity(model.ShopId, model.ItemId, model.Price, 0, model.Position);
                dbContext.Shopitems.Add(dbModel);
            }
            else
            {
                dbModel.ItemId = model.ItemId;
                dbModel.Price = model.Price;
                dbModel.Position = model.Position;
            }
            await dbContext.SaveChangesAsync();

            IsDirty = true;
        }

        public async Task DeleteShopItem(int id)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            await dbContext.Shopitems.Where(x => x.Shopitemid == id).ExecuteDeleteAsync();

            IsDirty = true;
        }

        public async Task FlushData()
        {
            if (!IsDirty)
                return;
            await _server.Transport.BroadcastMessageN(ChannelRecvCode.ShopDataUpdated);
            IsDirty = false;
        }
        #endregion
    }
}
