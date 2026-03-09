using Application.EF;
using Application.EF.Entities;
using Application.Shared.Constants.Item;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Core.Login.Services
{
    public class ShopService
    {
        readonly IMapper _mapper;
        readonly ILogger<ShopService> _logger;
        readonly IDbContextFactory<DBContext> _dbContextFactory;

        public ShopService(IMapper mapper, ILogger<ShopService> logger, IDbContextFactory<DBContext> dbContextFactory)
        {
            _mapper = mapper;
            _logger = logger;
            _dbContextFactory = dbContextFactory;
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
    }
}
