using Application.Core.Login.Datas;
using Application.Core.Login.Models;
using Application.Core.Login.Shared;
using Application.EF;
using Application.EF.Entities;
using Application.Shared.Items;
using Application.Shared.Message;
using Application.Utility;
using AutoMapper;
using AutoMapper.Extensions.ExpressionMapping;
using DueyDto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Application.Core.Login.ServerData
{
    public class DueyManager : StorageBase<int, DueyPackageModel>
    {
        private int _currentId = 0;

        readonly ILogger<DueyManager> _logger;
        readonly IDbContextFactory<DBContext> _dbContextFactory;
        readonly IMapper _mapper;
        readonly MasterServer _server;

        public DueyManager(ILogger<DueyManager> logger, IDbContextFactory<DBContext> dbContextFactory, IMapper maper, MasterServer server)
        {
            _logger = logger;
            _dbContextFactory = dbContextFactory;
            _mapper = maper;

            _server = server;
        }

        public override async Task InitializeAsync(DBContext dbContext)
        {
            _currentId = await dbContext.Dueypackages.MaxAsync(x => (int?)x.PackageId) ?? 0;
        }

        public override List<DueyPackageModel> Query(Expression<Func<DueyPackageModel, bool>> expression)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();

            var entityExpression = _mapper.MapExpression<Expression<Func<DueyPackageEntity, bool>>>(expression).Compile();
            var dbList = dbContext.Dueypackages.AsNoTracking().Where(entityExpression).ToList();

            var allPackageItems = _server.InventoryManager.LoadItems(dbContext, false, dbList.Select(x => x.PackageId).ToArray(), ItemType.Duey);

            List<DueyPackageModel> dataFromDB = [];
            foreach (var item in dbList)
            {
                var package = _mapper.Map<DueyPackageModel>(item);
                package.Item = allPackageItems.FirstOrDefault(x => x.Characterid == package.Id);
                dataFromDB.Add(package);
            }

            return QueryWithDirty(dataFromDB, expression.Compile());
        }

        public async Task TakeDueyPackage(DueyDto.TakeDueyPackageRequest request)
        {
            var res  = new DueyDto.TakeDueyPackageResponse { Request = request };
            var package = Query(x => x.Id == request.PackageId).FirstOrDefault();
            if (package == null)
            {
                res.Code = 1;

                await _server.Transport.SendMessageN(ChannelRecvCode.TakeDueyPackage, res, [request.MasterId]);
                return;
            }

            if (package.ReceiverId != request.MasterId)
            {
                res.Code = 2;

                await _server.Transport.SendMessageN(ChannelRecvCode.TakeDueyPackage, res, [request.MasterId]);
                return;
            }

            if (package.TimeStamp.ToUnixTimeMilliseconds() > _server.getCurrentTime())
            {
                res.Code = 3;

                await _server.Transport.SendMessageN(ChannelRecvCode.TakeDueyPackage, res, [request.MasterId]);
                return;
            }

            if (package.IsFrozen)
            {
                res.Code = 1;

                await _server.Transport.SendMessageN(ChannelRecvCode.TakeDueyPackage, res, [request.MasterId]);
                return;
            }

            package.IsFrozen.Set(true);
            res.Package = _mapper.Map<DueyDto.DueyPackageDto>(package);
            await _server.Transport.SendMessageN(ChannelRecvCode.TakeDueyPackage, res, [request.MasterId]);
        }

        public void PackageUnfreeze(int chrId)
        {
            var packages = Query(x => x.ReceiverId == chrId).Where(x => x.IsFrozen);
            foreach (var package in packages)
            {
                package.IsFrozen.Set(false);
                _logger.LogInformation($"Package {package.Id} automatically unfrozen due to player disconnect.");
            }
        }

        public async Task TakeDueyPackageCommit(DueyDto.TakeDueyPackageCommit request)
        {
            if (request.Success)
            {
                await RemovePackage(new DueyDto.RemovePackageRequest { MasterId = request.MasterId, PackageId = request.PackageId, ByReceived = true });
            }
            else
            {
                var package = Query(x => x.Id == request.PackageId).FirstOrDefault();
                if (package != null)
                {
                    // 领取失败、解冻
                    package.IsFrozen.Set(false);
                }
            }
        }


        public async Task CreateDueyPackage(DueyDto.CreatePackageRequest request)
        {
            var res = new CreatePackageResponse() { Request = request };
            var target = _server.CharacterManager.FindPlayerByName(request.ReceiverName);
            var sender = _server.CharacterManager.FindPlayerById(request.SenderId);
            if (target == null || sender == null)
            {
                res.Code = (int)SendDueyItemResponseCode.CharacterNotExisted;
                await _server.Transport.SendMessageN(ChannelRecvCode.CreateDueyPackage, res, [request.SenderId]);
            }

            else if (target.Character.AccountId == sender.Character.AccountId)
            {
                res.Code = (int)SendDueyItemResponseCode.SameAccount;
                await _server.Transport.SendMessageN(ChannelRecvCode.CreateDueyPackage, res, [request.SenderId]);
            }

            else
            {
                // Q.为什么特快是提前一天？而不是让普通包裹推迟一天？
                var time = DateTimeOffset.FromUnixTimeMilliseconds(_server.getCurrentTime());
                if (request.Quick)
                    time = time.AddDays(-1);
                var model = new DueyPackageModel()
                {
                    Id = Interlocked.Increment(ref _currentId),
                    ReceiverId = target.Character.Id,
                    SenderId = sender.Character.Id,
                    Mesos = request.SendMeso,
                    Message = request.SendMessage,
                    Type = request.Quick,
                    Checked = true,
                    TimeStamp = time,
                    Item = _mapper.Map<ItemModel>(request.Item, ctx =>
                    {
                        ctx.Items["Type"] = (int)ItemType.Duey;
                    })
                };

                SetDirty(model.Id, new StoreUnit<DueyPackageModel>(StoreFlag.AddOrUpdate, model));
                res.Package = _mapper.Map<DueyDto.DueyPackageDto>(model);

                await _server.Transport.SendMessageN(ChannelRecvCode.CreateDueyPackage, res, [request.SenderId, model.ReceiverId]);
            }
        }

        public async Task RemovePackage(DueyDto.RemovePackageRequest request)
        {
            var res = new DueyDto.RemovePackageResponse { Code = 0, Request = request };
            var package = Query(x => x.Id == request.PackageId).FirstOrDefault();
            if (package == null || package.ReceiverId != request.MasterId)
            {
                res.Code = 1;
                return;
            }

            SetRemoved(package.Id);
            await _server.Transport.SendMessageN(ChannelRecvCode.DeleteDueyPackage, res, [request.MasterId]);
        }

        public async Task GetPlayerDueyPackages(GetPlayerDueyPackageRequest request)
        {
            var res = new GetPlayerDueyPackageResponse();
            res.List.AddRange(_mapper.Map<DueyDto.DueyPackageDto[]>(Query(x => x.ReceiverId == request.ReceiverId)));
            res.ReceiverId = request.ReceiverId;

            await _server.Transport.SendMessageN(ChannelRecvCode.LoadDueyPackage, res, [request.ReceiverId]);
        }

        public void RunDueyExpireSchedule()
        {
            try
            {
                var dayBefore30 = DateTimeOffset.UtcNow.AddDays(-30);
                var toRemove = Query(x => x.TimeStamp < dayBefore30).Select(X => X.Id).ToList();

                foreach (int pid in toRemove)
                {
                    SetRemoved(pid);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
            }
        }

        internal async Task SendDueyNotifyOnLogin(int id)
        {
            var allUnreadData = Query(x => x.ReceiverId == id && x.Checked).OrderByDescending(x => x.Type);
            var data = allUnreadData.FirstOrDefault();
            if (data != null)
            {
                foreach (var item in allUnreadData)
                {
                    item.Checked = false;
                    SetDirty(item.Id, new StoreUnit<DueyPackageModel>(StoreFlag.AddOrUpdate, item));
                }
                await _server.Transport.SendMessageN(ChannelRecvCode.LoginNotifyDueyPackage, new DueyDto.DueyNotifyDto { Type = data.Type, ReceiverId = data.ReceiverId }, [data.ReceiverId]);
            }
        }

        protected override async Task CommitInternal(DBContext dbContext, Dictionary<int, StoreUnit<DueyPackageModel>> updateData)
        {
            var updatePackages = updateData.Keys.ToArray();
            var dbList = await dbContext.Dueypackages.Where(x => updatePackages.Contains(x.PackageId)).ExecuteDeleteAsync();
            foreach (var kv in updateData)
            {
                var obj = kv.Value.Data;
                if (kv.Value.Flag == StoreFlag.AddOrUpdate && obj != null)
                {
                    var dbData = new DueyPackageEntity(obj.Id, obj.ReceiverId, obj.SenderId, obj.Mesos, obj.Message, obj.Checked, obj.Type, obj.TimeStamp);
                    dbContext.Dueypackages.Add(dbData);
                }
                await InventoryManager.CommitInventoryByTypeAsync(dbContext, kv.Key, obj?.Item == null ? [] : [obj.Item], ItemFactory.DUEY);

            }
            await dbContext.SaveChangesAsync();
        }
    }
}
