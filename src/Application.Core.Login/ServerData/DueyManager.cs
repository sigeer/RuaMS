using Application.Core.Login.Models;
using Application.Core.Login.Services;
using Application.EF;
using Application.Shared.Items;
using AutoMapper;
using client.inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Application.Core.Login.Datas
{
    public class DueyManager
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
        readonly DataStorage _dataStorage;

        public DueyManager(ILogger<DueyManager> logger, IDbContextFactory<DBContext> dbContextFactory, IMapper maper, MasterServer server, DataStorage dataStorage)
        {
            _logger = logger;
            _dbContextFactory = dbContextFactory;
            _mapper = maper;

            _dataSource = new();
            _server = server;
            _dataStorage = dataStorage;
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


        public Dto.CreatePackageCheckResponse CreateDueyPackageCheck(int senderId, string recipient)
        {
            var response = new Dto.CreatePackageCheckResponse();
            response.ReceiverId = -1;
            var target = _server.CharacterManager.FindPlayerByName(recipient);
            var sender = _server.CharacterManager.FindPlayerById(senderId);
            if (target == null || sender == null)
            {
                response.Code = (int)SendDueyItemResponseCode.CharacterNotExisted;
                return response;
            }

            if (target.Character.AccountId == sender.Character.AccountId)
            {
                response.Code = (int)SendDueyItemResponseCode.SameAccount;
                return response;
            }

            response.Code = (int)SendDueyItemResponseCode.Success;
            response.ReceiverId = target.Character.Id;
            return response;
        }

        public void UpdatePackageId(int localId, int packageId)
        {
            if (_dataSource.TryGetValue(localId, out var d))
                d.PackageId = packageId;
        }


        public Dto.CreatePackageResponse CreateDueyPackage(string senderName, int sendMesos, Dto.ItemDto? item, string? sendMessage, int receiverId, bool quick)
        {
            try
            {
                var model = new DueyPackageModel()
                {
                    Id = Interlocked.Increment(ref _currentId),
                    ReceiverId = receiverId,
                    SenderName = senderName,
                    Mesos = sendMesos,
                    Message = sendMessage,
                    Type = quick,
                    Checked = true,
                    TimeStamp = DateTimeOffset.UtcNow,
                    Item = _mapper.Map<ItemModel>(item, ctx =>
                    {
                        ctx.Items["Type"] = ItemFactory.DUEY.getValue();
                    })
                };
                _dataSource[model.Id] = model;
                _dataStorage.SetDueyPackageAdded(model);
                return new Dto.CreatePackageResponse { IsSuccess = true };
            }
            catch (Exception sqle)
            {
                _logger.LogError(sqle.ToString());
            }

            return new Dto.CreatePackageResponse { IsSuccess = false };
        }

        public void RemovePackageById(int localId)
        {
            if (_dataSource.TryRemove(localId, out var d))
                _dataStorage.SetDueyPackageRemoved(d);
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

        public Dto.DueyPackageDto[] GetPlayerDueyPackages(int playerId)
        {
            return _mapper.Map<Dto.DueyPackageDto[]>(_dataSource.Values.Where(x => x.ReceiverId == playerId));
        }

        public Dto.DueyPackageDto? GetDueyPackageByPackageId(int id)
        {
            if (_dataSource.TryGetValue(id, out var d))
                return _mapper.Map<Dto.DueyPackageDto>(d);
            return null;
        }

        public void RunDueyExpireSchedule()
        {
            try
            {
                var dayBefore30 = DateTimeOffset.UtcNow.AddDays(-30);
                var toRemove = _dataSource.Values.Where(x => x.TimeStamp < dayBefore30).Select(X => X.Id).ToList();

                foreach (int pid in toRemove)
                {
                    RemovePackageById(pid);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
            }
        }
    }
}
