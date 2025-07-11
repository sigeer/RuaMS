using Application.Core.Login.Models;
using Application.Core.Login.Shared;
using Application.EF;
using Application.EF.Entities;
using Application.Utility;
using AutoMapper;
using AutoMapper.Extensions.ExpressionMapping;
using Google.Protobuf.WellKnownTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace Application.Core.Login.ServerData
{
    public class RingManager : StorageBase<int, RingModel>
    {
        readonly IDbContextFactory<DBContext> _dbContextFactory;
        readonly ILogger<RingManager> _logger;
        readonly IMapper _mapper;


        int _localId = 0;

        public RingManager(IDbContextFactory<DBContext> dbContextFactory, ILogger<RingManager> logger, IMapper mapper)
        {
            _dbContextFactory = dbContextFactory;
            _logger = logger;
            _mapper = mapper;
        }

        public RingModel CreateRing(int itemId, int chr1, int chr2)
        {
            var model = new RingModel()
            {
                Id = Interlocked.Increment(ref _localId),
                CharacterId1 = chr1,
                CharacterId2 = chr2,
                ItemId = itemId,
                RingId1 = Yitter.IdGenerator.YitIdHelper.NextId(),
                RingId2 = Yitter.IdGenerator.YitIdHelper.NextId()
            };
            SetDirty(model.Id, new Utility.StoreUnit<RingModel>(Utility.StoreFlag.AddOrUpdate, model));
            return model;
        }


        protected override async Task CommitInternal(DBContext dbContext, Dictionary<int, StoreUnit<RingModel>> updateData)
        {
            var updateKeys = updateData.Keys.ToArray();
            await dbContext.Rings.Where(x => updateKeys.Contains(x.Id)).ExecuteDeleteAsync();

            foreach (var item in updateData.Values)
            {
                var obj = item.Data;
                if (item.Flag == StoreFlag.AddOrUpdate && obj != null)
                {
                    var dbData = new Ring_Entity(obj.Id, obj.ItemId, obj.RingId1, obj.RingId2, obj.CharacterId1, obj.CharacterId2);
                    dbContext.Rings.Add(dbData);
                }
            }

            await dbContext.SaveChangesAsync();
        }

        public override List<RingModel> Query(Expression<Func<RingModel, bool>> expression)
        {
            var entityExpression = _mapper.MapExpression<Expression<Func<Ring_Entity, bool>>>(expression);

            using var dbContext = _dbContextFactory.CreateDbContext();

            var dataFromDB = _mapper.Map<List<RingModel>>(dbContext.Rings.Where(entityExpression));
            return QueryWithDirty(dataFromDB, expression.Compile());
        }
    }
}
