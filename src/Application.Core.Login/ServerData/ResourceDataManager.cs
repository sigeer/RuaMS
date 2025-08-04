using Application.Core.Login.Models;
using Application.Core.Login.Shared;
using Application.EF;
using Application.EF.Entities;
using Application.Utility;
using AutoMapper;
using AutoMapper.Extensions.ExpressionMapping;
using BaseProto;
using ItemProto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using ZLinq;

namespace Application.Core.Login.ServerData
{
    public class ResourceDataManager : StorageBase<int, PLifeModel>
    {
        readonly ILogger<ResourceDataManager> _logger;
        readonly IMapper _mapper;
        readonly MasterServer _server;

        readonly IDbContextFactory<DBContext> _dbContextFactory;

        int _localPLifeId = 0;

        public ResourceDataManager(ILogger<ResourceDataManager> logger, IMapper mapper, MasterServer server, IDbContextFactory<DBContext> dbContextFactory)
        {
            _logger = logger;
            _mapper = mapper;
            _server = server;
            _dbContextFactory = dbContextFactory;
        }

        public override async Task InitializeAsync(DBContext dbContext)
        {
            _localPLifeId = await dbContext.Plives.MaxAsync(x => (int?)x.Id) ?? 0;
        }

        public override List<PLifeModel> Query(Expression<Func<PLifeModel, bool>> expression)
        {
            var entityExpression = _mapper.MapExpression<Expression<Func<PlifeEntity, bool>>>(expression).Compile();
            using var dbContext = _dbContextFactory.CreateDbContext();
            var dataFromDB = _mapper.Map<List<PLifeModel>>(dbContext.Plives.AsNoTracking().Where(entityExpression).ToList());

            return QueryWithDirty(dataFromDB, expression.Compile());
        }

        public GetPLifeByMapIdResponse LoadMapPLife(GetPLifeByMapIdRequest request)
        {
            var res = new GetPLifeByMapIdResponse();
            res.List.AddRange(_mapper.Map<PLifeDto[]>(Query(x => x.Map == request.MapId)));
            return res;
        }

        public void CreatePLife(CreatePLifeRequest request)
        {
            var newKey = Interlocked.Increment(ref _localPLifeId);
            var newModel = _mapper.Map<PLifeModel>(request.Data);
            SetDirty(newKey, new StoreUnit<PLifeModel>(StoreFlag.AddOrUpdate, newModel));

            _server.Transport.BroadcastPLifeCreated(request);
        }

        public void RemovePLife(RemovePLifeRequest request)
        {
            List<PLifeModel> toRemove = [];
            if (request.LifeId > 0)
            {
                toRemove = Query(x => x.Type == request.LifeType && x.Map == request.MapId && x.Life == request.LifeId).ToList();

            }
            else
            {
                toRemove = Query(x => x.Type == request.LifeType && x.Map == request.MapId && x.Life == request.LifeId)
                    .Where(x => x.X >= request.PosX - 50 && x.X <= request.PosX + 50 && x.Y >= request.PosY - 50 && x.Y <= request.PosY + 50).ToList();
            }

            foreach (var item in toRemove)
            {
                SetRemoved(item.Id);
            }

            var res = new RemovePLifeResponse { MasterId = request.MasterId };
            res.RemovedItems.AddRange(_mapper.Map<PLifeDto[]>(toRemove));
            _server.Transport.BroadcastPLifeRemoved(res);
        }

        protected override async Task CommitInternal(DBContext dbContext, Dictionary<int, StoreUnit<PLifeModel>> updateData)
        {
            var usedData = updateData.Keys.ToArray();
            await dbContext.Plives.Where(x => usedData.Contains(x.Id)).ExecuteDeleteAsync();
            foreach (var item in updateData.Values)
            {
                var obj = item.Data;
                if (item.Flag == StoreFlag.AddOrUpdate && obj != null)
                {
                    var model = new PlifeEntity(obj.Id, obj.Map, obj.Life, obj.Mobtime, obj.X, obj.Y, obj.Fh, obj.Type);
                    dbContext.Plives.Add(model);
                    await dbContext.SaveChangesAsync();
                    obj.Id = model.Id;
                }
            }
            await dbContext.SaveChangesAsync();
        }
    }
}
