using Application.Core.Login;
using Application.Core.Login.Datas;
using Application.Core.Login.Models;
using Application.Core.Login.Shared;
using Application.EF;
using Application.EF.Entities;
using Application.Module.Duey.Master.Models;
using Application.Shared.Items;
using Application.Utility;
using AutoMapper;
using AutoMapper.Extensions.ExpressionMapping;
using client.inventory;
using DueyDto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace Application.Module.Duey.Master
{
    public class DueyManager : StorageBase<int, DueyPackageModel>
    {
        private int _currentId = 0;

        readonly ILogger<DueyManager> _logger;
        readonly IDbContextFactory<DBContext> _dbContextFactory;
        readonly IMapper _mapper;
        readonly MasterServer _server;
        readonly DueyMasterTransport _transport;

        public DueyManager(ILogger<DueyManager> logger, IDbContextFactory<DBContext> dbContextFactory, IMapper maper, MasterServer server, DueyMasterTransport transport)
        {
            _logger = logger;
            _dbContextFactory = dbContextFactory;
            _mapper = maper;

            _server = server;
            _transport = transport;
        }

        public async Task Initialize(DBContext dbContext)
        {
            _currentId = await dbContext.Dueypackages.MaxAsync(x => (int?)x.PackageId) ?? 0;
        }

        protected override List<DueyPackageModel> Query(Expression<Func<DueyPackageModel, bool>> expression)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();

            var entityExpression = _mapper.MapExpression<Expression<Func<DueyPackageEntity, bool>>>(expression).Compile();
            var dbList = (from a in dbContext.Dueypackages.AsNoTracking().Where(entityExpression)
                          join b in dbContext.Characters on a.SenderId equals b.Id
                          select new { Package = a, SenderName = b.Name }).ToList();

            var allPackageItems = InventoryManager.LoadDueyItems(dbContext, dbList.Select(x => x.Package.PackageId).ToArray());

            List<DueyPackageModel> dataFromDB = [];
            foreach (var item in dbList)
            {
                var package = _mapper.Map<DueyPackageModel>(item.Package);
                package.Item = _mapper.Map<ItemModel>(allPackageItems.FirstOrDefault(x => x.Item.Characterid == package.Id));
                package.SenderName = item.SenderName;

                dataFromDB.Add(package);
            }

            return QueryWithDirty(dataFromDB, expression.Compile());
        }

        public void TakeDueyPackage(DueyDto.TakeDueyPackageRequest request)
        {
            var package = Query(x => x.Id == request.PackageId).FirstOrDefault();
            if (package == null)
            {
                _transport.SendTakeDueyPackage(new DueyDto.TakeDueyPackageResponse { Code = 1, Request = request });
                return;
            }

            if (package.ReceiverId != request.MasterId)
            {
                _transport.SendTakeDueyPackage(new DueyDto.TakeDueyPackageResponse { Code = 2, Request = request });
                return;
            }

            if (package.TimeStamp.ToUnixTimeMilliseconds() > _server.getCurrentTime())
            {
                _transport.SendTakeDueyPackage(new DueyDto.TakeDueyPackageResponse { Code = 3, Request = request });
                return;
            }

            if (!package.TryFreeze())
            {
                _transport.SendTakeDueyPackage(new DueyDto.TakeDueyPackageResponse { Code = 1, Request = request });
                return;
            }

            _transport.SendTakeDueyPackage(new DueyDto.TakeDueyPackageResponse { Request = request, Package = _mapper.Map<DueyDto.DueyPackageDto>(package) });
        }

        public void PackageUnfreeze(int chrId)
        {
            var packages = Query(x => x.ReceiverId == chrId && x.IsFrozen);
            foreach (var package in packages)
            {
                package.ForceUnfreeze();
                _logger.LogInformation($"Package {package.Id} automatically unfrozen due to player disconnect.");
            }
        }

        public void TakeDueyPackageCommit(DueyDto.TakeDueyPackageCommit request)
        {
            if (request.Success)
            {
                RemovePackage(new DueyDto.RemovePackageRequest { MasterId = request.MasterId, PackageId = request.PackageId, ByReceived = true });
            }
            else
            {
                var package = Query(x => x.Id == request.PackageId).FirstOrDefault();
                if (package != null)
                {
                    // 领取失败、解冻
                    package.ForceUnfreeze();
                }
            }
        }


        public void CreateDueyPackage(DueyDto.CreatePackageRequest request)
        {
            try
            {
                var target = _server.CharacterManager.FindPlayerByName(request.ReceiverName);
                var sender = _server.CharacterManager.FindPlayerById(request.SenderId);
                if (target == null || sender == null)
                {
                    _transport.SendCreatePackage(new DueyDto.CreatePackageResponse
                    {
                        Code = (int)SendDueyItemResponseCode.CharacterNotExisted,
                        Transaction = _server.ItemTransactionManager.CreateTransaction(request.Transaction, ItemTransactionStatus.PendingForRollback)
                    });
                    return;
                }

                if (target.Character.AccountId == sender.Character.AccountId)
                {
                    _transport.SendCreatePackage(new DueyDto.CreatePackageResponse
                    {
                        Code = (int)SendDueyItemResponseCode.SameAccount,
                        Transaction = _server.ItemTransactionManager.CreateTransaction(request.Transaction, ItemTransactionStatus.PendingForRollback)
                    });
                    return;
                }

                // Q.为什么特快是提前一天？而不是让普通包裹推迟一天？
                var time = DateTimeOffset.FromUnixTimeMilliseconds(_server.getCurrentTime());
                if (request.Quick)
                    time = time.AddDays(-1);
                var model = new DueyPackageModel()
                {
                    Id = Interlocked.Increment(ref _currentId),
                    ReceiverId = target.Character.Id,
                    SenderId = sender.Character.Id,
                    SenderName = sender.Character.Name,
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

                _transport.SendCreatePackage(new DueyDto.CreatePackageResponse
                {
                    Package = _mapper.Map<DueyDto.DueyPackageDto>(model),
                    Transaction = _server.ItemTransactionManager.CreateTransaction(request.Transaction, ItemTransactionStatus.PendingForCommit)
                });
            }
            catch (Exception sqle)
            {
                _logger.LogError(sqle.ToString());
            }
        }

        public void RemovePackage(DueyDto.RemovePackageRequest request)
        {
            var package = Query(x => x.Id == request.PackageId).FirstOrDefault();
            if (package == null || package.ReceiverId != request.MasterId)
            {
                return;
            }

            SetRemoved(package.Id);
            _transport.SendDueyPackageRemoved(new DueyDto.RemovePackageResponse { Code = 0, Request = request });
        }

        public DueyDto.GetPlayerDueyPackageResponse GetPlayerDueyPackages(GetPlayerDueyPackageRequest request)
        {
            var res = new GetPlayerDueyPackageResponse();
            res.List.AddRange(_mapper.Map<DueyDto.DueyPackageDto[]>(Query(x => x.ReceiverId == request.ReceiverId)));
            res.ReceiverId = request.ReceiverId;
            return res;
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

        internal void SendDueyNotifyOnLogin(int id)
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
                _transport.SendDueyNotifyOnLogin(id, new DueyDto.DueyNotifyDto { Type = data.Type, ReceiverId = data.ReceiverId });
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
