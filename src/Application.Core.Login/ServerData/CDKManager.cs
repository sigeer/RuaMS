using Application.Core.Login.Models;
using Application.Core.Login.Shared;
using Application.EF;
using Application.Shared.Items;
using Application.Utility;
using ItemProto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace Application.Core.Login.ServerData
{
    public class CDKManager : DataStorageBase<int, CdkRecordModel, CdkRecordEntity>
    {
        readonly MasterServer _server;
        IMemoryCache _cache;

        public CDKManager(MasterServer server, IMapper mapper, IDbContextFactory<DBContext> dbContextFactory, IMemoryCache cache, ILogger<CDKManager> logger) 
            : base(StorageCategory.CDK, dbContextFactory, mapper, logger)
        {
            _server = server;
            _cache = cache;
        }

        protected override int GetKey(CdkRecordModel model) => model.Id;


        CdkCodeModel? GetCdkData(string cdk)
        {
            return _cache.GetOrCreate($"CDK:{cdk}", e =>
            {
                using var dbContext = _dbContextFactory.CreateDbContext();
                var model = dbContext.CdkCodes.FirstOrDefault(x => x.Code == cdk);
                if (model != null)
                {
                    var items = dbContext.CdkItems.Where(x => x.CodeId == model.Id).ToList();
                    var dto = _mapper.Map<CdkCodeModel>(model);
                    dto.Items = _mapper.Map<List<CdkItemModel>>(items);
                    return dto;
                }
                return null;
            });
        }

        ConcurrentDictionary<string, Lock> _cdkLocks = new ConcurrentDictionary<string, Lock>();

        public ItemProto.UseCdkResponse UseCdk(ItemProto.UseCdkRequest request)
        {
            var chr = _server.CharacterManager.FindPlayerById(request.MasterId);
            if (chr == null || chr.Channel != -1)
                return new UseCdkResponse { Code = (int)UseCdkResponseCode.FetalError };

            var lockObj = _cdkLocks.GetOrAdd(request.Cdk, new Lock());

            lock (lockObj)
            {
                var data = GetCdkData(request.Cdk);

                if (data != null)
                {

                    if (data.Expiration < _server.getCurrentTime())
                        return new UseCdkResponse { Code = (int)UseCdkResponseCode.Expired };

                    var histories = Query(x => x.CodeId == data.Id, x => x.CodeId == data.Id).ToList();

                    if (data.MaxCount > 0 && histories.Count >= data.MaxCount)
                        return new UseCdkResponse { Code = (int)UseCdkResponseCode.Used };

                    if (histories.Any(x => x.RecipientId == request.MasterId))
                        return new UseCdkResponse { Code = (int)UseCdkResponseCode.Used };

                    SetDirty(new CdkRecordModel { Id = Interlocked.Increment(ref _localId), CodeId = data.Id, RecipientId = request.MasterId, RecipientTime = _server.GetCurrentTimeDateTimeOffset() });

                    var res = new UseCdkResponse();
                    res.Items.AddRange(_mapper.Map<ItemProto.CdkRewordPackageDto[]>(data.Items));
                    return res;
                }

            }
            return new UseCdkResponse { Code = (int)UseCdkResponseCode.NotFound };
        }
    }
}
