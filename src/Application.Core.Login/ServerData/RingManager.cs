using Application.Core.Login.Models;
using Application.Core.Login.Shared;
using Application.EF;
using Application.EF.Entities;
using Application.Utility;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace Application.Core.Login.ServerData
{
    public class RingManager : StorageBase<int, RingSourceModel>
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

        public override async Task InitializeAsync(DBContext dbContext)
        {
            _localId = await dbContext.Rings.MaxAsync(x => (int?)x.Id) ?? 0;
        }

        public RingSourceModel CreateRing(int itemId, int chr1, int chr2)
        {
            var model = new RingSourceModel()
            {
                Id = Interlocked.Increment(ref _localId),
                CharacterId1 = chr1,
                CharacterId2 = chr2,
                ItemId = itemId,
                RingId1 = Yitter.IdGenerator.YitIdHelper.NextId(),
                RingId2 = Yitter.IdGenerator.YitIdHelper.NextId()
            };
            SetDirty(model.Id, new Utility.StoreUnit<RingSourceModel>(Utility.StoreFlag.AddOrUpdate, model));
            return model;
        }

        public List<RingSourceModel> LoadRings(long[] ringId)
        {
            return Query(x => ringId.Contains(x.RingId1) || ringId.Contains(x.RingId2));
        }


        protected override async Task CommitInternal(DBContext dbContext, Dictionary<int, StoreUnit<RingSourceModel>> updateData)
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

        public override List<RingSourceModel> Query(Expression<Func<RingSourceModel, bool>> expression)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();

            var dataFromDB = dbContext.Rings.AsNoTracking().ProjectToType<RingSourceModel>().Where(expression).ToList();
            return QueryWithDirty(dataFromDB, expression.Compile());
        }
    }
}
