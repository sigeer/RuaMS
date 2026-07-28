using Application.Core.Login.Shared;
using Application.EF;
using ItemProto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Core.Login.ServerData
{
    public class GachaponManager : ReadonlyStorageBase
    {
        readonly IMapper _mapper;

        List<ItemProto.GachaponPoolDto> _pools = new();
        List<ItemProto.GachaponPoolChanceDto> _itemChance = new();
        List<ItemProto.GachaponPoolItemDto> _item = new();
        public GachaponManager(IDbContextFactory<DBContext> dbContextFactory, ILogger<ReadonlyStorageBase> logger, MasterServer server, IMapper mapper)
            : base(logger, StorageCategory.Gachapon)
        {
            _mapper = mapper;
        }

        public override async Task InitializeAsync(DBContext dbContext)
        {
            await base.InitializeAsync(dbContext);

            _pools = _mapper.Map<List<ItemProto.GachaponPoolDto>>(await dbContext.GachaponPools.AsNoTracking().ToListAsync());
            _itemChance = _mapper.Map<List<ItemProto.GachaponPoolChanceDto>>(await dbContext.GachaponPoolLevelChances.AsNoTracking().ToListAsync());
            _item = _mapper.Map<List<ItemProto.GachaponPoolItemDto>>(await dbContext.GachaponPoolItems.AsNoTracking().ToListAsync());
        }

        public GacheponDataDto GetGachaponData()
        {
            var res = new GacheponDataDto();
            res.Pools.AddRange(_pools);
            res.Items.AddRange(_item);
            res.Chances.AddRange(_itemChance);
            return res;
        }
    }
}
