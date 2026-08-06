using Application.Core.EF.Entities.Gachapons;
using Application.Core.Login.Dtos.Gachapon;
using Application.Core.Login.Shared;
using Application.EF;
using Application.EF.Entities;
using Application.Shared.Message;
using Application.Templates.Reader;
using Application.Templates.String;
using ItemProto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Polly;

namespace Application.Core.Login.ServerData
{
    public class GachaponManager
    {
        public bool IsDirty { get; private set; }
        readonly IMapper _mapper;
        IDbContextFactory<DBContext> _dbContextFactory;
        readonly MasterServer _server;
        public GachaponManager(IDbContextFactory<DBContext> dbContextFactory, ILogger<ReadonlyStorageBase> logger, MasterServer server, IMapper mapper)
        {
            _server = server;
            _mapper = mapper;
            _dbContextFactory = dbContextFactory;
        }

        public GacheponDataDto GetGachaponData()
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var _pools = _mapper.Map<List<ItemProto.GachaponPoolDto>>(dbContext.GachaponPools.AsNoTracking().ToList());
            var _itemChance = _mapper.Map<List<ItemProto.GachaponPoolChanceDto>>(dbContext.GachaponPoolLevelChances.AsNoTracking().ToList());
            var _item = _mapper.Map<List<ItemProto.GachaponPoolItemDto>>(dbContext.GachaponPoolItems.AsNoTracking().ToList());
            var res = new GacheponDataDto();
            res.Pools.AddRange(_pools);
            res.Items.AddRange(_item);
            res.Chances.AddRange(_itemChance);
            return res;
        }

        #region Console
        public List<GachaponResponseDto> GetAllGachaponList(int? itemId, string locale)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var pools = dbContext.GachaponPools.AsNoTracking();
            if (itemId != null)
            {
                var items = dbContext.GachaponPoolItems.Where(x => x.ItemId == itemId).Select(x => x.PoolId).ToList();
                pools = pools.Where(x => items.Contains(x.Id));
            }
            var dataList = pools.ProjectToType<GachaponResponseDto>().ToList();
            foreach (var item in dataList)
            {
                item.NpcName = ProviderSource.Instance.GetProviderByKey<IStringProvider>(locale).GetSubProvider(Templates.String.StringCategory.Npc)
                    ?.GetItem(item.NpcId)?.Name ?? "";
            }
            return dataList;
        }

        public GachaponDetailResponseDto GetGachaponDetail(int id, string locale)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var pool = dbContext.GachaponPools.AsNoTracking().FirstOrDefault(x => x.Id == id);
            if (pool == null)
            {
                return new GachaponDetailResponseDto { Id = id };
            }
            var settings = dbContext.GachaponPoolLevelChances.Where(x => x.PoolId == id).ProjectToType<GachaponSettingResponseDto>().ToList();
            var items = dbContext.GachaponPoolItems.Where(x => x.PoolId == id).ProjectToType<GachaponItemResponseDto>().ToList();
            foreach (var item in items)
            {
                item.ItemName = ProviderSource.Instance.GetProviderByKey<IStringProvider>(locale).GetSubProvider(Templates.String.StringCategory.Item)
                    ?.GetItem(item.ItemId)?.Name ?? "";
            }
            return new GachaponDetailResponseDto
            {
                Id = id,
                NpcId = pool.NpcId,
                Items = items,
                LevelSettings = settings
            };
        }

        public async Task Submit(GachaponRequestDto editModel)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var dbModel = await dbContext.GachaponPools.FirstOrDefaultAsync(x => x.Id == editModel.Id);
            if (dbModel == null)
            {
                await dbContext.GachaponPools.AddAsync(new GachaponPoolEntity(editModel.NpcId));
            }
            else
            {
                _mapper.Map(editModel, dbModel);
            }

            await dbContext.SaveChangesAsync();
            IsDirty = true;
        }

        public async Task Remove(int id)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            await dbContext.GachaponPools.Where(x => x.Id == id).ExecuteDeleteAsync();
            await dbContext.GachaponPoolItems.Where(x => x.PoolId == id).ExecuteDeleteAsync();
            await dbContext.GachaponPoolLevelChances.Where(x => x.PoolId == id).ExecuteDeleteAsync();
            IsDirty = true;
        }

        public async Task SubmitReward(GachaponItemRequestDto editModel)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var dbModel = await dbContext.GachaponPoolItems.FirstOrDefaultAsync(x => x.Id == editModel.Id);
            if (dbModel == null)
            {
                await dbContext.GachaponPoolItems.AddAsync(new GachaponPoolItemEntity(editModel.PoolId, editModel.Level, editModel.ItemId, editModel.Quantity));
            }
            else
            {
                _mapper.Map(editModel, dbModel);
            }

            await dbContext.SaveChangesAsync();
            IsDirty = true;
        }

        public async Task RemoveReward(int id)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            await dbContext.GachaponPoolItems.Where(x => x.Id == id).ExecuteDeleteAsync();
            IsDirty = true;
        }

        public async Task FlushData()
        {
            if (!IsDirty)
                return;
            await _server.Transport.BroadcastMessageN(ChannelRecvCode.GachaponDataUpdated);
            IsDirty = false;
        }
        #endregion
    }
}
