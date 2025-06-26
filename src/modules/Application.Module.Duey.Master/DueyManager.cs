using Application.Core.Login;
using Application.Core.Login.Datas;
using Application.Core.Login.Models;
using Application.Core.Login.Shared;
using Application.EF;
using Application.Module.Duey.Master.Models;
using Application.Shared.Items;
using Application.Utility;
using AutoMapper;
using DueyDto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Application.Module.Duey.Master
{
    public class DueyManager : StorageBase<int, UpdateField<DueyPackageModel>>
    {
        /// <summary>
        /// Key: localId(不是packageId,packageId只用来保存数据库)
        /// </summary>
        ConcurrentDictionary<int, DueyPackageModel> _dataSource;

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

            _dataSource = new();
            _server = server;
            _transport = transport;
        }

        public async Task Initialize(DBContext dbContext)
        {
            var allPackages = _mapper.Map<List<DueyPackageModel>>(await dbContext.Dueypackages.AsNoTracking().ToListAsync());

            var allPackageItems = InventoryManager.LoadDueyItems(dbContext, allPackages.Select(x => x.PackageId).ToArray());
            foreach (var package in allPackages)
            {
                package.Item = _mapper.Map<ItemModel>(allPackageItems.FirstOrDefault(x => x.Item.Characterid == package.PackageId));
                package.Id = Interlocked.Increment(ref _currentId);
                _dataSource[package.Id] = package;
            }
        }


        public void UpdatePackageId(int localId, int packageId)
        {
            if (_dataSource.TryGetValue(localId, out var d))
                d.PackageId = packageId;
        }

        public void TakeDueyPackage(DueyDto.TakeDueyPackageRequest request)
        {
            if (!_dataSource.TryGetValue(request.PackageId, out var package) || package.Fronzen)
            {
                _transport.SendTakeDueyPackage(new DueyDto.TakeDueyPackageResponse { Code = 1, Request = request });
                return;
            }

            if (package.ReceiverId != request.MasterId)
            {
                _transport.SendTakeDueyPackage(new DueyDto.TakeDueyPackageResponse {  Code = 2, Request = request });
                return;
            }

            package.Fronzen = true;
            _transport.SendTakeDueyPackage(new DueyDto.TakeDueyPackageResponse { Request = request, Package = _mapper.Map<DueyDto.DueyPackageDto>(package) });
        }

        public void TakeDueyPackageCommit(DueyDto.TakeDueyPackageCommit request)
        {
            if (request.Success)
            {
                if (_dataSource.TryRemove(request.PackageId, out var package))
                {
                    RemovePackageById(new DueyDto.RemovePackageRequest { MasterId = request.MasterId, PackageId = request.PackageId });
                }
            }
            else
            {
                if (_dataSource.TryGetValue(request.PackageId, out var package))
                {
                    package.Fronzen = false;
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
                    _transport.SendCreatePackage(new DueyDto.CreatePackageResponse { Code = (int)SendDueyItemResponseCode.CharacterNotExisted, CostMeso = request.SendMeso });
                    return;
                }

                if (target.Character.AccountId == sender.Character.AccountId)
                {
                    _transport.SendCreatePackage(new DueyDto.CreatePackageResponse { Code = (int)SendDueyItemResponseCode.SameAccount, CostMeso = request.SendMeso });
                    return;
                }

                var model = new DueyPackageModel()
                {
                    Id = Interlocked.Increment(ref _currentId),
                    ReceiverId = target.Character.Id,
                    SenderId = sender.Character.Id,
                    Mesos = request.SendMeso,
                    Message = request.SendMessage,
                    Type = request.Quick,
                    Checked = true,
                    TimeStamp = DateTimeOffset.UtcNow,
                    Item = _mapper.Map<ItemModel>(request.Item, ctx =>
                    {
                        ctx.Items["Type"] = (int)ItemType.Duey;
                    })
                };
                _dataSource[model.Id] = model;

                SetDirty(model.Id, new UpdateField<DueyPackageModel>(UpdateMethod.AddOrUpdate, model));

                _transport.SendCreatePackage(new DueyDto.CreatePackageResponse { Package = _mapper.Map<DueyDto.DueyPackageDto>(model) });
                _server.Transport.SendDueyNotification(target.Channel, target.Character.Id, target.Character.Name, model.Type);
            }
            catch (Exception sqle)
            {
                _logger.LogError(sqle.ToString());
            }
        }

        public void RemovePackageById(DueyDto.RemovePackageRequest request)
        {
            if (!_dataSource.TryRemove(request.PackageId, out var d))
            {
                return;
            }

            SetDirty(d.Id, new UpdateField<DueyPackageModel>(UpdateMethod.Remove, d));
            _transport.SendDueyPackageRemoved(new DueyDto.RemovePackageResponse { Code = 0, Request = request });
        }

        public void SendDueyNotification(string characterName)
        {
            var target = _server.CharacterManager.FindPlayerByName(characterName);
            if (target != null && target.Channel > 0)
            {
                using var dbContext = _dbContextFactory.CreateDbContext();
                var sender = dbContext.Dueypackages.Where(x => x.ReceiverId == target.Character.Id && x.Checked)
                    .OrderByDescending(x => x.Type).Select(x => new { x.SenderName, x.Type }).FirstOrDefault();

                if (sender != null)
                {
                    dbContext.Dueypackages.Where(x => x.ReceiverId == target.Character.Id).ExecuteUpdate(x => x.SetProperty(y => y.Checked, false));
                    _server.Transport.SendDueyNotification(target.Channel, target.Character.Id, sender.SenderName, sender.Type);
                }
            }
        }

        public DueyDto.DueyPackageDto[] GetPlayerDueyPackages(GetPlayerDueyPackageRequest request)
        {
            return _mapper.Map<DueyDto.DueyPackageDto[]>(_dataSource.Values.Where(x => x.ReceiverId == request.ReceiverId));
        }

        public void RunDueyExpireSchedule()
        {
            try
            {
                var dayBefore30 = DateTimeOffset.UtcNow.AddDays(-30);
                var toRemove = _dataSource.Values.Where(x => x.TimeStamp < dayBefore30).Select(X => X.Id).ToList();

                foreach (int pid in toRemove)
                {
                    if (_dataSource.TryRemove(pid, out var d))
                    {
                        SetDirty(d.Id, new UpdateField<DueyPackageModel>(UpdateMethod.Remove, d));
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
            }
        }
    }
}
