using Application.Core.Login.Models.Gachpons;
using Application.EF;
using AutoMapper;
using ItemProto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Core.Login.ServerData
{
    public class GachaponManager
    {
        readonly IDbContextFactory<DBContext> _dbContextFactory;
        readonly ILogger<GachaponManager> _logger;
        readonly MasterServer _server;
        readonly IMapper _mapper;

        List<GachaponPoolModel> _pools = new();
        List<GachaponPoolLevelChanceModel> _itemChance = new();
        List<GachaponPoolItemModel> _item = new();
        public GachaponManager(IDbContextFactory<DBContext> dbContextFactory, ILogger<GachaponManager> logger, MasterServer server, IMapper mapper)
        {
            _dbContextFactory = dbContextFactory;
            _logger = logger;
            _server = server;
            _mapper = mapper;
        }

        public async Task InitializeAsync(DBContext dbContext)
        {
            _pools = _mapper.Map<List<GachaponPoolModel>>(await dbContext.GachaponPools.AsNoTracking().ToListAsync());
            _itemChance = _mapper.Map<List<GachaponPoolLevelChanceModel>>(await dbContext.GachaponPoolLevelChances.AsNoTracking().ToListAsync());
            _item = _mapper.Map<List<GachaponPoolItemModel>>(await dbContext.GachaponPoolItems.AsNoTracking().ToListAsync());
        }

        public GacheponDataDto GetGachaponData()
        {
            var res = new GacheponDataDto();
            res.Pools.AddRange(_mapper.Map<ItemProto.GachaponPoolDto[]>(_pools));
            res.Items.AddRange(_mapper.Map<ItemProto.GachaponPoolItemDto[]>(_item));
            res.Chances.AddRange(_mapper.Map<ItemProto.GachaponPoolChanceDto[]>(_itemChance));
            return res;
        }
    }
}
