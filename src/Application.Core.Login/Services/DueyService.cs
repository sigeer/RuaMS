using Application.Core.EF.Entities.Items;
using Application.Core.Login.Datas;
using Application.Core.Login.Models;
using Application.EF;
using AutoMapper;
using client.inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Core.Login.Services
{
    public class DueyService
    {
        readonly ILogger<DueyService> _logger;
        readonly IDbContextFactory<DBContext> _dbContextFactory;
        readonly IMapper _mapper;

        public DueyService(ILogger<DueyService> logger, IDbContextFactory<DBContext> dbContextFactory, IMapper mapper)
        {
            _logger = logger;
            _dbContextFactory = dbContextFactory;
            _mapper = mapper;
        }

        private static List<ItemEntityPair> LoadDueyItems(DBContext dbContext, int[] packageIdArray)
        {
            var items = (from a in dbContext.Inventoryitems.AsNoTracking()
                .Where(x => x.Characterid != null && packageIdArray.Contains(x.Characterid.Value) && x.Type == ItemFactory.DUEY.getValue())
                         join c in dbContext.Pets.AsNoTracking() on a.Petid equals c.Petid into css
                         from cs in css.DefaultIfEmpty()
                         select new { Item = a, Pet = cs }).ToList();

            var invItemId = items.Select(x => x.Item.Inventoryitemid).ToList();
            var equips = (from a in dbContext.Inventoryequipments.AsNoTracking().Where(x => invItemId.Contains(x.Inventoryitemid))
                          join e in dbContext.Rings.AsNoTracking() on a.RingId equals e.Id into ess
                          from es in ess.DefaultIfEmpty()
                          select new EquipEntityPair(a, es)).ToList();

            return (from a in items
                    join b in equips on a.Item.Inventoryitemid equals b.Equip.Inventoryitemid into bss
                    from bs in bss.DefaultIfEmpty()
                    select new ItemEntityPair(a.Item, bs, a.Pet)).ToList();
        }

        public Dto.DueyPackageDto[] GetPlayerDueyPackages(int id)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();

            var dataList = dbContext.Dueypackages.Where(x => x.ReceiverId == id).ToArray();
            var allDueyItems = LoadDueyItems(dbContext, dataList.Select(x => x.PackageId).ToArray());
            return dataList.Select(x =>
            {
                var m = _mapper.Map<Dto.DueyPackageDto>(x);
                m.Item.AddRange(_mapper.Map<Dto.ItemDto[]>(allDueyItems.Where(y => y.Item.Characterid == x.PackageId)));
                return m;
            }).ToArray();
        }

        public Dto.DueyPackageDto? GetDueyPackageByPackageId(int id)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();

            var dbModel = dbContext.Dueypackages.FirstOrDefault(x => x.PackageId == id);
            if (dbModel == null)
                return null;

            var allDueyItems = LoadDueyItems(dbContext, [id]);
            var m = _mapper.Map<Dto.DueyPackageDto>(dbModel);
            m.Item.AddRange(_mapper.Map<Dto.ItemDto[]>(allDueyItems.Where(y => y.Item.Characterid == id)));
            return m;
        }

        public void runDueyExpireSchedule()
        {
            try
            {
                var dayBefore30 = DateTimeOffset.UtcNow.AddDays(-30);
                using var dbContext = _dbContextFactory.CreateDbContext();

                var toRemove = dbContext.Dueypackages.Where(x => x.TimeStamp < dayBefore30).Select(X => X.PackageId).ToList();


                foreach (int pid in toRemove)
                {
                    RemovePackageFromDB(pid);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
            }
        }

        public void RemovePackageFromDB(int packageId)
        {
            try
            {
                using var dbContext = new DBContext();
                using var dbTrans = dbContext.Database.BeginTransaction();
                dbContext.Dueypackages.Where(x => x.PackageId == packageId).ExecuteDelete();

                InventoryManager.CommitInventoryByType(dbContext, packageId, [], ItemFactory.DUEY);

                dbTrans.Commit();
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
            }
        }

        public void SubmitDueyPackage(Dto.DueyPackageDto package)
        {
            try
            {
                using var dbContext = new DBContext();
                using var dbTrans = dbContext.Database.BeginTransaction();

                var dbModel = dbContext.Dueypackages.FirstOrDefault(x => x.PackageId == package.PackageId);
                if (dbModel == null)
                    return;

                _mapper.Map(package, dbModel);

                InventoryManager.CommitInventoryByType(dbContext, package.PackageId, _mapper.Map<ItemModel[]>(package.Item), ItemFactory.DUEY);

                dbContext.SaveChanges();
                dbTrans.Commit();
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
            }
        }
    }
}
