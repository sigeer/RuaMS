using Application.Core.Login.Dtos.Drop;
using Application.EF;
using Application.EF.Entities;
using Application.Shared.Message;
using Application.Templates.Reader;
using Application.Templates.String;
using Application.Utility.Extensions;
using Dto;
using Microsoft.EntityFrameworkCore;
using XmlWzReader;

namespace Application.Core.Login.ServerData
{
    public class DropDataManager
    {
        readonly IDbContextFactory<DBContext> _dbContextFactory;
        readonly IMapper _mapper;
        readonly MasterServer _server;

        public bool IsDirty { get; private set; }
        public DropDataManager(IDbContextFactory<DBContext> dbContextFactory, IMapper mapper, MasterServer server)
        {
            _dbContextFactory = dbContextFactory;
            _mapper = mapper;
            _server = server;
        }

        #region Console
        public (List<DropResponseDto>, int Total) QueryMobDrop(int filterMob, int? filterItem, int filterQuest, int pageIndex, int pageSize, string locale)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var dbSet = dbContext.DropData.AsNoTracking();
            if (filterMob > 0)
            {
                dbSet = dbSet.Where(x => filterMob == x.Dropperid);
            }

            if (filterItem != null)
            {
                dbSet = dbSet.Where(x => filterItem == (x.Itemid));
            }

            if (filterQuest > 0)
            {
                dbSet = dbSet.Where(x => filterQuest == (x.Questid));
            }

            var items = dbSet.OrderBy(x => x.Id).ProjectToType<DropResponseDto>().ToPage(pageIndex, pageSize).ToList();
            foreach (var item in items)
            {
                item.ItemName = ProviderSource.Instance.GetProviderByKey<IStringProvider>(locale).GetSubProvider(Templates.String.StringCategory.Item)
                    ?.GetRequiredItem<StringTemplate>(item.ItemId)?.Name ?? "";
                item.MobName = ProviderSource.Instance.GetProviderByKey<IStringProvider>(locale).GetSubProvider(Templates.String.StringCategory.Mob)
                    ?.GetRequiredItem<StringTemplate>(item.DropperId)?.Name ?? "";
            }
            return (items, dbSet.Count());
        }

        public async Task SubmitMobDropData(DropRequestDto editModel)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var dbModel = await dbContext.DropData.FirstOrDefaultAsync(x => x.Id == editModel.Id);
            if (dbModel == null)
            {
                await dbContext.DropData.AddAsync(new DropDataEntity(editModel.DropperId, editModel.ItemId, editModel.MinCount, editModel.MaxCount, editModel.QuestId, editModel.Chance));
            }
            else
            {
                _mapper.Map(editModel, dbModel);
            }

            await dbContext.SaveChangesAsync();
            IsDirty = true;
        }

        public async Task<int> DeleteDropData(int id)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            await dbContext.DropData.Where(x => x.Id == id).ExecuteDeleteAsync();

            IsDirty = true;

            return 1;
        }

        public (List<DropResponseDto>, int Total) QueryGlobalDrop(int filterContinent, int? filterItem, int filterQuest, int pageIndex, int pageSize, string locale)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var dbSet = dbContext.DropDataGlobals.AsNoTracking();
            if (filterContinent > 0)
            {
                dbSet = dbSet.Where(x => filterContinent == x.Continent);
            }

            if (filterItem != null)
            {
                dbSet = dbSet.Where(x => filterItem == (x.Itemid));
            }

            if (filterQuest > 0)
            {
                dbSet = dbSet.Where(x => filterQuest == (x.Questid));
            }

            var items = dbSet.OrderBy(x => x.Id).ProjectToType<DropResponseDto>().ToPage(pageIndex, pageSize).ToList();
            foreach (var item in items)
            {
                item.ItemName = ProviderSource.Instance.GetProviderByKey<IStringProvider>(locale).GetSubProvider(Templates.String.StringCategory.Item)
                    ?.GetRequiredItem<StringTemplate>(item.ItemId)?.Name ?? "";
            }
            return (items, dbSet.Count());
        }

        public async Task SubmitGlobalDropData(DropRequestDto editModel)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var dbModel = await dbContext.DropDataGlobals.FirstOrDefaultAsync(x => x.Id == editModel.Id);
            if (dbModel == null)
            {
                await dbContext.DropDataGlobals.AddAsync(new DropDataGlobalEntity((sbyte)editModel.DropperId, editModel.ItemId, editModel.MinCount, editModel.MaxCount,editModel.QuestId, editModel.Chance));
            }
            else
            {
                _mapper.Map(editModel, dbModel);
            }

            await dbContext.SaveChangesAsync();
            IsDirty = true;
        }

        public async Task<bool> DeleteGlobalDropData(int id)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            await dbContext.DropDataGlobals.Where(x => x.Id == id).ExecuteDeleteAsync();

            IsDirty = true;

            return true;
        }

        public (List<DropResponseDto>, int Total) QueryReactorDrop(int? filterItem, int filterQuest, int pageIndex, int pageSize, string locale)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var dbSet = dbContext.Reactordrops.AsNoTracking();

            if (filterItem != null)
            {
                dbSet = dbSet.Where(x => filterItem == (x.Itemid));
            }

            if (filterQuest > 0)
            {
                dbSet = dbSet.Where(x => filterQuest == (x.Questid));
            }

            var items = dbSet.OrderBy(x => x.Reactordropid).ProjectToType<DropResponseDto>().ToPage(pageIndex, pageSize).ToList();
            foreach (var item in items)
            {
                item.ItemName = ProviderSource.Instance.GetProviderByKey<IStringProvider>(locale).GetSubProvider(Templates.String.StringCategory.Item)
                    ?.GetItem(item.ItemId)?.Name ?? "";
                item.QuestName = ProviderSource.Instance.GetProviderByKey<IStringProvider>(locale).GetSubProvider(Templates.String.StringCategory.Quest)
                    ?.GetItem(item.ItemId)?.Name;
            }
            return (items, dbSet.Count());
        }

        public async Task SubmitReactorDropData(DropRequestDto editModel)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var dbModel = await dbContext.Reactordrops.FirstOrDefaultAsync(x => x.Reactordropid == editModel.Id);
            if (dbModel == null)
            {
                await dbContext.Reactordrops.AddAsync(new ReactorDropEntity(editModel.DropperId, editModel.ItemId, editModel.Chance, editModel.QuestId));
            }
            else
            {
                _mapper.Map(editModel, dbModel);
            }

            await dbContext.SaveChangesAsync();
            IsDirty = true;
        }

        public async Task<bool> DeleteReactorDropData(int id)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            await dbContext.Reactordrops.Where(x => x.Reactordropid == id).ExecuteDeleteAsync();

            IsDirty = true;

            return true;
        }

        public async Task FlushData()
        {
            if (!IsDirty)
                return;
            await _server.Transport.BroadcastMessageN(ChannelRecvCode.DropDataUpdated);
            IsDirty = false;
        }
        #endregion

        public Dto.DropAllDto LoadMobDropDto()
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var mobDrop = dbContext.DropData.Where(x => x.Chance >= 0).AsNoTracking().ToList();
            var globalDrop = dbContext.DropDataGlobals.Where(x => x.Chance >= 0).AsNoTracking().ToList();
            var data = new DropAllDto();
            data.Items.AddRange(_mapper.Map<Dto.DropItemDto[]>(mobDrop));
            data.Items.AddRange(_mapper.Map<Dto.DropItemDto[]>(globalDrop));
            return data;
        }

        public Dto.DropAllDto LoadAllReactorDrops()
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var dbData = dbContext.Reactordrops.Where(x => x.Chance >= 0).AsNoTracking().ToList();
            var data = new DropAllDto();
            data.Items.AddRange(_mapper.Map<Dto.DropItemDto[]>(dbData));
            return data;
        }
    }
}
