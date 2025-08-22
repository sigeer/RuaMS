using Application.Core.Login.Models;
using Application.Core.Login.Shared;
using Application.EF;
using Application.Shared.Items;
using Application.Utility;
using ItemProto;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace Application.Core.Login.ServerData
{
    public class CDKManager : StorageBase<int, CdkCodeModel>
    {
        readonly MasterServer _server;
        readonly IMapper _mapper;
        readonly IDbContextFactory<DBContext> _dbContextFactory;

        public CDKManager(MasterServer server, IMapper mapper, IDbContextFactory<DBContext> dbContextFactory)
        {
            _server = server;
            _mapper = mapper;
            _dbContextFactory = dbContextFactory;
        }


        protected override async Task CommitInternal(DBContext dbContext, Dictionary<int, StoreUnit<CdkCodeModel>> updateData)
        {
            await dbContext.CdkItems.Where(x => updateData.Keys.Contains(x.CodeId)).ExecuteDeleteAsync();
            await dbContext.CdkRecords.Where(x => updateData.Keys.Contains(x.CodeId)).ExecuteDeleteAsync();

            foreach (var item in updateData)
            {
                if (item.Value.Flag == StoreFlag.AddOrUpdate)
                {
                    var obj = item.Value.Data!;
                    dbContext.CdkItems.AddRange(obj.Items.Select(x => new Application.EF.Entities.CdkItemEntity(obj.Id, x.Type, x.ItemId, x.Quantity)));
                    dbContext.CdkRecords.AddRange(obj.Histories.Select(x => new CdkRecordEntity(obj.Id, x.RecipientId, x.RecipientTime)));
                }

            }
            await dbContext.SaveChangesAsync();
        }

        public override List<CdkCodeModel> Query(Expression<Func<CdkCodeModel, bool>> expression)
        {
            using var dbContext = _dbContextFactory.CreateDbContext(); ;

            var dataFromDB = dbContext.CdkCodes.AsNoTracking().ProjectToType<CdkCodeModel>().Where(expression).ToList();

            var filteredCodeIds = dataFromDB.Select(x => x.Id).ToArray();
            var allCodeItems = dbContext.CdkItems.AsNoTracking().Where(x => filteredCodeIds.Contains(x.CodeId)).ToList();
            var allHistories = dbContext.CdkRecords.AsNoTracking().Where(x => filteredCodeIds.Contains(x.CodeId)).ToList();
            foreach (var item in dataFromDB)
            {
                item.Items = _mapper.Map<List<CdkItemModel>>(allCodeItems.Where(y => y.CodeId == item.Id));
                item.Histories = _mapper.Map<List<CdkRecordModel>>(allHistories.Where(y => y.CodeId == item.Id));
            }
            return QueryWithDirty(dataFromDB, expression.Compile());
        }

        public override Task InitializeAsync(DBContext dbContext)
        {
            return Task.CompletedTask;
        }

        ConcurrentDictionary<string, Lock> _cdkLocks = new ConcurrentDictionary<string, Lock>();

        public ItemProto.UseCdkResponse UseCdk(ItemProto.UseCdkRequest request)
        {
            var chr = _server.CharacterManager.FindPlayerById(request.MasterId);
            if (chr == null || chr.Channel != -1)
                return new UseCdkResponse { Code = (int)UseCdkResponseCode.FetalError };

            var data = Query(x => x.Code == request.Cdk).FirstOrDefault();

            if (data != null)
            {
                var lockObj = _cdkLocks.GetOrAdd(request.Cdk, new Lock());

                lock (lockObj)
                {
                    if (data.Expiration < _server.getCurrentTime())
                        return new UseCdkResponse { Code = (int)UseCdkResponseCode.Expired };

                    if (data.MaxCount > 0 && data.Histories.Count >= data.MaxCount)
                        return new UseCdkResponse { Code = (int)UseCdkResponseCode.Used };

                    if (data.Histories.Any(x => x.RecipientId == request.MasterId))
                        return new UseCdkResponse { Code = (int)UseCdkResponseCode.Used };

                    data.Histories.Add(new CdkRecordModel { RecipientId = request.MasterId, RecipientTime = _server.GetCurrentTimeDateTimeOffset() });
                    SetDirty(new StoreUnit<CdkCodeModel>(StoreFlag.AddOrUpdate, data));

                    var res = new UseCdkResponse();
                    res.Items.AddRange(_mapper.Map<ItemProto.CdkRewordPackageDto[]>(data.Items));
                    return res;
                }

            }
            return new UseCdkResponse { Code = (int)UseCdkResponseCode.NotFound };
        }
    }
}
