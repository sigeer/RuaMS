using Application.EF;
using Application.EF.Entities;
using Application.Host.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using server;
using server.life;

namespace Application.Host.Services
{
    public class DropdataFilter : Pagination
    {
        public string? Mob { get; set; }
        public string? Item { get; set; }
    }
    public class DropdataService
    {
        readonly DBContext _dbContext;
        readonly IMapper _mapper;

        public DropdataService(DBContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<int> AddOrUpdate(DropDataDto model)
        {
            var dbModel = await _dbContext.DropData.FirstOrDefaultAsync(x => x.Id == model.Id);
            if (dbModel == null)
            {
                dbModel = _mapper.Map<DropDatum>(model);

                await _dbContext.DropData.AddAsync(dbModel);
            }
            else
            {
                _mapper.Map(model, dbModel);
            }

            await _dbContext.SaveChangesAsync();
            return 1;
        }

        public async Task<PagedData<DropDataDto>> GetPagedData(DropdataFilter filter)
        {
            var dbSet = _dbContext.DropData.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(filter.Mob))
            {
                List<int> filteredMobId = [];
                if (int.TryParse(filter.Mob, out var d))
                    filteredMobId.Add(d);
                filteredMobId.AddRange(MonsterInformationProvider.getInstance().getMobsIDsFromName(filter.Mob).Select(x => x.Key));
                dbSet = dbSet.Where(x => filteredMobId.Contains(x.Dropperid));
            }
            if (!string.IsNullOrWhiteSpace(filter.Item))
            {
                List<int> filterItemId = [];
                if (int.TryParse(filter.Item, out var d))
                    filterItemId.Add(d);
                filterItemId.AddRange(ItemInformationProvider.getInstance().getItemDataByName(filter.Item).Select(x => x.Id));
                dbSet = dbSet.Where(x => filterItemId.Contains(x.Itemid));
            }

            return _mapper.Map<PagedData<DropDataDto>>(await dbSet.OrderBy(x => x.Id).ToPageAsync(filter.PageIndex, filter.PageSize));
        }
    }
}
