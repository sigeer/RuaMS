using Application.Core.Login.Shared;
using Application.EF;
using Application.EF.Entities;
using Application.Utility;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace Application.Core.Login.ServerData
{
    public class RingManager : StorageBase<int, ItemProto.RingDto>
    {
        readonly IDbContextFactory<DBContext> _dbContextFactory;
        readonly ILogger<RingManager> _logger;
        readonly IMapper _mapper;
        readonly MasterServer _server;


        int _localId = 0;

        public RingManager(IDbContextFactory<DBContext> dbContextFactory, ILogger<RingManager> logger, IMapper mapper, MasterServer server)
            : base(x => x.Id)
        {
            _dbContextFactory = dbContextFactory;
            _logger = logger;
            _mapper = mapper;
            _server = server;
        }

        public override async Task InitializeAsync(DBContext dbContext)
        {
            _localId = await dbContext.Rings.MaxAsync(x => (int?)x.Id) ?? 0;
        }

        public ItemProto.RingDto CreateRing(int itemId, int chr1, int chr2)
        {
            var model = new ItemProto.RingDto()
            {
                Id = Interlocked.Increment(ref _localId),
                CharacterId1 = chr1,
                CharacterId2 = chr2,
                ItemId = itemId,
                RingId1 = Yitter.IdGenerator.YitIdHelper.NextId(),
                RingId2 = Yitter.IdGenerator.YitIdHelper.NextId(),
                CharacterName1 = _server.CharacterManager.GetPlayerName(chr1),
                CharacterName2 = _server.CharacterManager.GetPlayerName(chr2)
            };

            return model;
        }


        protected override async Task CommitInternal(DBContext dbContext, Dictionary<int, StoreUnit<ItemProto.RingDto>> updateData)
        {
            var updateItems = updateData.Keys.ToArray();

            var allDbList = await dbContext.Rings.Where(x => updateItems.Contains(x.Id)).ToListAsync();
            foreach (var item in updateData)
            {
                var dbModel = allDbList.FirstOrDefault(x => x.Id == item.Key);
                if (item.Value.Flag == StoreFlag.Remove)
                {
                    if (dbModel != null)
                    {
                        dbContext.Rings.Remove(dbModel);
                    }
                    continue;
                }

                if (item.Value.Data is null)
                    continue;

                if (dbModel == null)
                {
                    dbModel = _mapper.Map<RingEntity>(item.Value.Data);
                    dbContext.Rings.Add(dbModel);
                }
                else
                {
                    _mapper.Map(item.Value.Data, dbModel);
                }
            }
            await dbContext.SaveChangesAsync();
        }

        public override List<ItemProto.RingDto> Query(Expression<Func<ItemProto.RingDto, bool>> expression)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();

            var dataFromDB = dbContext.Rings.AsNoTracking().ProjectToType<ItemProto.RingDto>().Where(expression).ToList();
            return QueryWithDirty(dataFromDB, expression.Compile());
        }
    }
}
