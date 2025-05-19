using Application.EF;
using Application.EF.Entities;
using Application.Shared.Items;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Core.Login.Datas
{
    public class ItemManager
    {
        Dictionary<int, ItemDto[]> _invUpdate = new();
        Dictionary<int, ItemDto[]> _cashShopUpdate = new();
        Dictionary<int, ItemDto[]> _storageUpdate = new();
        Dictionary<int, ItemDto[]> _merchantUpdate = new();
        Dictionary<int, ItemDto[]> _dueyUpdate = new();
        Dictionary<int, ItemDto[]> _marriageUpdate = new();

        readonly IDbContextFactory<DBContext> _dbContextFactory;
        readonly IMapper _mapper;

        public ItemManager(IDbContextFactory<DBContext> dbContextFactory, IMapper mapper)
        {
            _dbContextFactory = dbContextFactory;
            _mapper = mapper;
        }

        public void UpdateCharacterInventory(int characterId, ItemDto[] inventoryItems)
        {
            _invUpdate[characterId] = inventoryItems;
        }

        public async Task CommitInventoryAsync()
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var allItems = dbContext.Inventoryitems.Where(x => _invUpdate.Keys.Contains(x.Characterid ?? 0)).ToList();
            var inventoryItems = allItems.Select(x => x.Inventoryitemid).ToArray();
            var petIds = allItems.Select(x => x.Petid).ToArray();
            await dbContext.Inventoryitems.Where(x => inventoryItems.Contains(x.Inventoryitemid)).ExecuteDeleteAsync();
            await dbContext.Inventoryequipments.Where(x => inventoryItems.Contains(x.Inventoryitemid)).ExecuteDeleteAsync();
            await dbContext.Pets.Where(x => petIds.Contains(x.Petid)).ExecuteDeleteAsync();

            await dbContext.Inventoryitems.AddRangeAsync(_mapper.Map<Inventoryitem[]>(_invUpdate.Values.SelectMany(x => x)));

            _invUpdate.Clear();
        }
    }
}
