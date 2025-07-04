using Application.EF;
using Application.Shared.Constants.Item;
using AutoMapper;
using MakerProto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Module.Maker.Master
{
    public class MakerManager
    {
        readonly ILogger<MakerManager> _logger;
        readonly IDbContextFactory<DBContext> _dbContextFactory;
        readonly IMapper _mapper;

        public MakerManager(ILogger<MakerManager> logger, IDbContextFactory<DBContext> dbContextFactory, IMapper mapper)
        {
            _logger = logger;
            _dbContextFactory = dbContextFactory;
            _mapper = mapper;
        }

        public MakerProto.QueryMakerCraftTableResponse GetMakerCraftTable(MakerProto.ItemIdRequest request)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var dbModel = dbContext.Makercreatedata.Where(x => x.Itemid == request.ItemId).Select(x => new { x.ReqLevel, x.ReqMakerLevel, x.ReqMeso, x.Quantity }).FirstOrDefault();
            if (dbModel == null)
                return new MakerProto.QueryMakerCraftTableResponse();

            int reqLevel = dbModel?.ReqLevel ?? -1;
            int reqMakerLevel = dbModel?.ReqMakerLevel ?? -1;
            int cost = dbModel?.ReqMeso ?? -1;
            int toGive = dbModel?.Quantity ?? -1;

            var model = new MakerProto.MakerCraftTable();
            _mapper.Map(dbModel, model);

            var dataList = dbContext.Makerrecipedata.Where(x => x.Itemid == request.ItemId).ToList();
            model.ReqItems = new MakerProto.MakerRequiredItems();
            model.ReqItems.List.AddRange(_mapper.Map<MakerProto.MakerRequiredItem[]>(dataList));
            return new QueryMakerCraftTableResponse() { Data = _mapper.Map<MakerProto.MakerCraftTable>(model) };
        }

        public MakerProto.QueryMakerItemStatResponse GetMakerReagentStatUpgrade(MakerProto.ItemIdRequest request)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var data = _mapper.Map<MakerProto.MakerItemStat>(
                dbContext.Makerreagentdata.Where(x => x.Itemid == request.ItemId).Select(x => new { x.Stat, x.Value }
                ).FirstOrDefault());
            return new MakerProto.QueryMakerItemStatResponse { Data = data };
        }

        public MakerProto.MakerRequiredItems GetMakerDisassembledItems(MakerProto.ItemIdRequest request)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var dataList = dbContext.Makerrecipedata.Where(x => x.Itemid == request.ItemId && x.ReqItem >= ItemId.BASIC_MONSTER_CRYSTAL_1 && x.Itemid < 4270000)
                .ToList()
                .Select(x => new MakerProto.MakerRequiredItem() { ItemId = x.ReqItem, Count = x.Count })
                .ToList();
            var res = new MakerProto.MakerRequiredItems();
            res.List.AddRange(dataList);
            return res;
        }
    }
}
