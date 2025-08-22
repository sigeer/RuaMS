using Application.EF;
using Application.Shared.Constants.Item;
using MakerProto;
using MapsterMapper;
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

            var model = new MakerProto.MakerCraftTable()
            {
                ReqMakerLevel = dbModel.ReqMakerLevel,
                ItemId = request.ItemId,
                Quantity = dbModel.Quantity,
                ReqLevel = dbModel.ReqLevel,
                ReqMeso = dbModel.ReqMeso,
            };

            var dataList = dbContext.Makerrecipedata.Where(x => x.Itemid == request.ItemId).ToList();
            model.ReqItems = new MakerProto.MakerRequiredItems();
            model.ReqItems.List.AddRange(_mapper.Map<MakerProto.MakerRequiredItem[]>(dataList));
            return new QueryMakerCraftTableResponse() { Data = _mapper.Map<MakerProto.MakerCraftTable>(model) };
        }

        public MakerProto.QueryMakerItemStatResponse GetMakerReagentStatUpgrade(MakerProto.ItemIdRequest request)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();

            var dbModel = dbContext.Makerreagentdata.Where(x => x.Itemid == request.ItemId).FirstOrDefault();
            var data = _mapper.Map<MakerProto.MakerItemStat>(dbModel);
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
