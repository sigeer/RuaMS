using Application.Core.Login.Shared;
using Application.EF;
using Application.EF.Entities;
using Application.Shared.Items;
using Application.Shared.Message;
using Application.Utility;
using DueyDto;
using Google.Protobuf.WellKnownTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Application.Core.Login.ServerData
{
    public class DueyManager : LocalStorageBase<int, DueyDto.DueyPackageDto>
    {
        readonly ILogger<DueyManager> _logger;
        readonly IDbContextFactory<DBContext> _dbContextFactory;
        readonly IMapper _mapper;
        readonly MasterServer _server;

        HashSet<int> _lockDic = new();
        public DueyManager(ILogger<DueyManager> logger, IDbContextFactory<DBContext> dbContextFactory, IMapper maper, MasterServer server)
            : base(x => x.PackageId)
        {
            _logger = logger;
            _dbContextFactory = dbContextFactory;
            _mapper = maper;

            _server = server;
        }

        protected override Task<int> SetLocalId(DBContext dbContext)
        {
            return dbContext.Dueypackages.Select(x => x.PackageId).DefaultIfEmpty().MaxAsync();
        }

        protected override async Task<ConcurrentDictionary<int, StoreUnit<DueyPackageDto>>> SetLocalData(DBContext dbContext)
        {
            var dayBefore30 = _server.GetCurrentTimeDateTimeOffset().AddDays(-30);
            return new System.Collections.Concurrent.ConcurrentDictionary<int, StoreUnit<DueyPackageDto>>(
                await dbContext.Dueypackages.Where(x => x.ClaimTime == null && x.CreateTime > dayBefore30)
                    .ProjectToType<DueyDto.DueyPackageDto>().ToDictionaryAsync(x => x.PackageId, x => new StoreUnit<DueyDto.DueyPackageDto>(StoreFlag.Cached, x)));
        }

        public override List<DueyDto.DueyPackageDto> Query(Func<DueyDto.DueyPackageDto, bool> expression)
        {
            var dayBefore30 = _server.GetCurrentTimeDateTimeOffset().AddDays(-30).ToTimestamp();

            return _localData.Values.Where(x => x.Flag != StoreFlag.Remove)
                .Select(x => x.Data!)
                .Where(x => x.CreateTime > dayBefore30 && x.ClaimTime == null)
                .Where(expression).ToList();
        }

        public async Task TakeDueyPackage(DueyDto.TakeDueyPackageRequest request)
        {
            var res = new DueyDto.TakeDueyPackageResponse { Request = request };
            var package = Query(x => x.PackageId == request.PackageId).FirstOrDefault();
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

            if (package.CreateTime.ToDateTimeOffset().AddDays(1).ToUnixTimeMilliseconds() > _server.getCurrentTime())
            {
                res.Code = 3;

                await _server.Transport.SendMessageN(ChannelRecvCode.TakeDueyPackage, res, [request.MasterId]);
                return;
            }

            if (_lockDic.Contains(package.PackageId))
            {
                res.Code = 1;

                await _server.Transport.SendMessageN(ChannelRecvCode.TakeDueyPackage, res, [request.MasterId]);
                return;
            }

            _lockDic.Add(package.PackageId);
            res.Package = MapToDto(package);
            await _server.Transport.SendMessageN(ChannelRecvCode.TakeDueyPackage, res, [request.MasterId]);
        }

        DueyDto.DueyPackageDto MapToDto(DueyDto.DueyPackageDto dto)
        {
            dto.SenderName = _server.CharacterManager.GetPlayerName(dto.SenderId);
            return dto;
        }

        public void PackageUnfreeze(int chrId)
        {
            var packages = Query(x => x.ReceiverId == chrId).Where(x => _lockDic.Contains(x.PackageId));
            foreach (var package in packages)
            {
                _lockDic.Remove(package.PackageId);
                _logger.LogInformation($"Package {package.PackageId} automatically unfrozen due to player disconnect.");
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
                var package = Query(x => x.PackageId == request.PackageId).FirstOrDefault();
                if (package != null)
                {
                    // 领取失败、解冻
                    _lockDic.Remove(package.PackageId);
                }
            }
        }


        public async Task<CreatePackageResponse> CreateDueyPackage(DueyDto.CreatePackageRequest request)
        {
            var res = new CreatePackageResponse();
            var target = _server.CharacterManager.FindPlayerByName(request.ReceiverName);
            var sender = _server.CharacterManager.FindPlayerById(request.SenderId);
            if (target == null || sender == null)
            {
                res.Code = (int)SendDueyItemResponseCode.CharacterNotExisted;
                return res;
            }

            else if (target.Character.AccountId == sender.Character.AccountId)
            {
                res.Code = (int)SendDueyItemResponseCode.SameAccount;
                return res;
            }

            else
            {
                // Q.为什么特快是提前一天？而不是让普通包裹推迟一天？
                var time = DateTimeOffset.FromUnixTimeMilliseconds(_server.getCurrentTime());
                if (request.Quick)
                    time = time.AddDays(-1);
                var model = new DueyDto.DueyPackageDto()
                {
                    PackageId = Interlocked.Increment(ref _localId),
                    ReceiverId = target.Character.Id,
                    SenderId = sender.Character.Id,
                    Mesos = request.SendMeso,
                    Message = request.SendMessage,
                    Type = request.Quick,
                    Notified = false,
                    CreateTime = time.ToTimestamp(),
                    Item = request.Item
                };

                SetDirty(model);

                var data = new CreatePackageBroadcast { Package = model };
                await _server.Transport.SendMessageN(ChannelRecvCode.CreateDueyPackage, data, [model.ReceiverId]);
                return res;
            }
        }

        public async Task RemovePackage(DueyDto.RemovePackageRequest request)
        {
            var res = new DueyDto.RemovePackageResponse { Code = 0, Request = request };
            var package = Query(x => x.PackageId == request.PackageId).FirstOrDefault();
            if (package == null || package.ReceiverId != request.MasterId)
            {
                res.Code = 1;
                return;
            }

            package.ClaimTime = _server.GetCurrentTimeDateTimeOffset().ToTimestamp();
            SetDirty(package.PackageId);

            await _server.Transport.SendMessageN(ChannelRecvCode.DeleteDueyPackage, res, [request.MasterId]);
        }

        public async Task GetPlayerDueyPackages(GetPlayerDueyPackageRequest request)
        {
            var res = new GetPlayerDueyPackageResponse();
            res.List.AddRange(Query(x => x.ReceiverId == request.ReceiverId));
            res.ReceiverId = request.ReceiverId;

            await _server.Transport.SendMessageN(ChannelRecvCode.LoadDueyPackage, res, [request.ReceiverId]);
        }

        internal async Task SendDueyNotifyOnLogin(int id)
        {
            var allUnreadData = Query(x => x.ReceiverId == id && x.Notified).OrderByDescending(x => x.Type);
            var data = allUnreadData.FirstOrDefault();
            if (data != null)
            {
                foreach (var item in allUnreadData)
                {
                    item.Notified = false;

                    SetDirty(item.PackageId);
                }
                await _server.Transport.SendMessageN(ChannelRecvCode.LoginNotifyDueyPackage, new DueyDto.DueyNotifyDto { Type = data.Type, ReceiverId = data.ReceiverId }, [data.ReceiverId]);
            }
        }

        protected override async Task CommitInternal(DBContext dbContext, Dictionary<int, StoreUnit<DueyPackageDto>> updateData)
        {
            var updatePackages = updateData.Keys.ToArray();

            var allDbList = await dbContext.Dueypackages.Where(x => updatePackages.Contains(x.PackageId)).ToListAsync();
            foreach (var item in updateData)
            {
                if (item.Value.Data == null)
                    continue;

                var dbModel = allDbList.FirstOrDefault(x => x.PackageId == item.Key);
                if (dbModel == null)
                {
                    dbModel = _mapper.Map<DueyPackageEntity>(item.Value.Data);
                    dbContext.Dueypackages.Add(dbModel);
                }
                else
                {
                    _mapper.Map(item.Value.Data, dbModel);
                }
            }
            await dbContext.SaveChangesAsync();
        }
    }
}
