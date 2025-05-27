using Application.EF;
using Application.EF.Entities;
using Application.Shared.Constants.Item;
using Application.Shared.Dto;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Core.Login.Services
{
    public class ShopService
    {
        HashSet<int> rechargeableItems = new();

        readonly IMapper _mapper;
        readonly ILogger<ShopService> _logger;
        readonly IDbContextFactory<DBContext> _dbContextFactory;

        public ShopService(IMapper mapper, ILogger<ShopService> logger, IDbContextFactory<DBContext> dbContextFactory)
        {
            _mapper = mapper;
            _logger = logger;
            _dbContextFactory = dbContextFactory;

            foreach (int throwingStarId in ItemId.allThrowingStarIds())
            {
                rechargeableItems.Add(throwingStarId);
            }
            rechargeableItems.Add(ItemId.BLAZE_CAPSULE);
            rechargeableItems.Add(ItemId.GLAZE_CAPSULE);
            rechargeableItems.Add(ItemId.BALANCED_FURY);
            rechargeableItems.Remove(ItemId.DEVIL_RAIN_THROWING_STAR); // doesn't exist
            foreach (int bulletId in ItemId.allBulletIds())
            {
                rechargeableItems.Add(bulletId);
            }
        }

        public ShopDto? LoadFromDB(int id, bool isShopId)
        {
            ShopDto? ret = null;
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
                    ret = _mapper.Map<ShopDto>(tmpModel);
                }
                else
                {
                    return null;
                }

                var items = new List<ShopItemDto>();
                List<int> recharges = new(rechargeableItems);
                var shopItems = dbContext.Shopitems.Where(x => x.Shopid == shopId).OrderByDescending(x => x.Position).ToList();
                shopItems.ForEach(x =>
                {
                    if (ItemConstants.isRechargeable(x.ItemId))
                    {
                        var m = _mapper.Map<ShopItemDto>(x);
                        m.Buyable = 1;
                        items.Add(m);
                        if (rechargeableItems.Contains(x.ItemId))
                        {
                            recharges.Remove(x.ItemId);
                        }
                    }
                    else
                    {
                        var m = _mapper.Map<ShopItemDto>(x);
                        m.Buyable = 1000;
                        items.Add(m);
                    }
                });
                foreach (int recharge in recharges)
                {
                    items.Add(new ShopItemDto() { Buyable = 1000, ItemId = recharge, Price = 0, Pitch = 0 });
                }
                ret.Items = items.ToArray();
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
            }
            return ret;
        }
    }
}
