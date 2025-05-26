using Application.Core.Game.Life;
using Application.EF;
using Application.EF.Entities;
using Application.Shared.Constants.Item;
using Application.Shared.Items;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Org.BouncyCastle.Cms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZLinq;

namespace Application.Core.Login.Services
{
    public class ItemService
    {
        readonly IDbContextFactory<DBContext> _dbContextFactory;
        readonly IMapper _mapper;

        public ItemService(IDbContextFactory<DBContext> dbContextFactory, IMapper mapper)
        {
            _dbContextFactory = dbContextFactory;
            _mapper = mapper;
        }

        public Dictionary<int, List<DropDto>> LoadAllReactorDrops()
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            return dbContext.Reactordrops.Where(x => x.Chance >= 0)
                .ToList()
                .AsValueEnumerable()
                .GroupBy(x => x.Reactorid)
                .Select(x => new KeyValuePair<int, List<DropDto>>(x.Key, _mapper.Map<List<DropDto>>(x.ToList())))
                .ToDictionary();
        }

        public int[] LoadReactorSkillBooks()
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            return dbContext.Reactordrops.Where(x => x.Itemid >= ItemId.SKILLBOOK_MIN_ITEMID && x.Itemid < ItemId.SKILLBOOK_MAX_ITEMID)
            .Select(x => x.Itemid)
            .ToArray();
        }

        public SpecialCashItem[] LoadSpecialCashItems()
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            return dbContext.Specialcashitems.AsNoTracking().ToList()
                   .Select(x => new SpecialCashItem(x.Sn, x.Modifier, (byte)x.Info)).ToArray();
        }

        public void InsertGift(int toId, string from, string message, int sn, int ringid = -1)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var giftModel = new GiftEntity(toId, from, message, sn, ringid);
            dbContext.Gifts.Add(giftModel);
            dbContext.SaveChanges();
        }

        public GiftDto[] LoadPlayerGifts(int playerId)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            return _mapper.Map<GiftDto[]>(dbContext.Gifts.AsNoTracking().Where(x => x.To == playerId).ToArray());
        }

        public void ClearGifts(int[] giftIdArray)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            dbContext.Gifts.Where(x => giftIdArray.Contains(x.Id)).ExecuteDelete();
        }
    }
}
