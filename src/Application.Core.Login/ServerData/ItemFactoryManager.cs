using Application.Core.EF.Entities.Items;
using Application.Core.Login.Models;
using Application.EF;
using Application.Shared.Items;
using Application.Utility;
using Application.Utility.Exceptions;
using AutoMapper;
using ItemProto;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace Application.Core.Login.ServerData
{
    public class ItemFactoryManager
    {
        ConcurrentDictionary<ItemFactoryKey, List<ItemModel>> _groupedItems = new();

        readonly MasterServer _server;
        readonly IDbContextFactory<DBContext> _dbContextFactory;
        readonly IMapper _mapper;

        public ItemFactoryManager(MasterServer server, IDbContextFactory<DBContext> dbContextFactory, IMapper mapper)
        {
            _server = server;
            _dbContextFactory = dbContextFactory;
            _mapper = mapper;
        }

        public List<ItemModel> LoadItems(int itemType, int key)
        {
            var keyObj = new ItemFactoryKey(itemType, key);
            if (_groupedItems.TryGetValue(keyObj, out var d))
                return d;

            var typeInfo = EnumClassCache<ItemFactory>.Values.FirstOrDefault(x => x.getValue() == itemType) ?? throw new BusinessFatalException($"不存在的道具分类 {itemType}");
            using var dbContext = _dbContextFactory.CreateDbContext();
            var dataList = (from a in dbContext.Inventoryitems.AsNoTracking()
                .Where(x => typeInfo.IsAccount ? x.Accountid == key : x.Characterid == key)
                .Where(x => x.Type == itemType)
                            join b in dbContext.Inventoryequipments on a.Inventoryitemid equals b.Inventoryitemid into bss
                            from bs in bss.DefaultIfEmpty()
                            join c in dbContext.Pets.AsNoTracking() on a.Petid equals c.Petid into css
                            from cs in css.DefaultIfEmpty()
                            select new ItemEntityPair(a, bs, cs)).ToList();

            var ringIds = dataList.Where(x => x.Equip != null && x.Equip.RingId > 0).Select(x => x.Equip!.RingId).ToArray();
            var rings = _server.RingManager.LoadRings(ringIds);
            List<ItemModel> items = [];
            foreach (var item in dataList)
            {
                RingSourceModel? ring = null;
                if (item.Equip != null && item.Equip.RingId > 0)
                    ring = rings.FirstOrDefault(x => x.RingId1 == item.Equip.RingId || x.RingId2 == item.Equip.RingId);

                var obj = _mapper.Map<ItemModel>(item);
                if (obj.EquipInfo != null)
                {
                    obj.EquipInfo.RingSourceInfo = ring;
                }
                items.Add(obj);
            }
            return items;
        }

        public void Store(int itemType, int key, List<ItemModel> items)
        {
            var keyObj = new ItemFactoryKey(itemType, key);
            _groupedItems[keyObj] = items;
        }

        public StoreItemsResponse Store(StoreItemsRequest request)
        {
            Store(request.ItemFactory, request.Key, _mapper.Map<List<ItemModel>>(request.Items));
            return new StoreItemsResponse();
        }

        public LoadItemsFromStoreResponse LoadItems(LoadItemsFromStoreRequest request)
        {
            var items = LoadItems(request.ItemFactory, request.Key);
            var res = new LoadItemsFromStoreResponse();
            res.ItemFactory = request.ItemFactory;
            res.Key = request.Key;
            res.Items.AddRange(_mapper.Map<Dto.ItemDto[]>(items));
            return res;
        }
    }
}
