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
    public class DueyManager : DataStorageBase<int, DueyDto.DueyPackageDto, DueyPackageEntity>
    {
        readonly MasterServer _server;

        HashSet<int> _lockDic = new();
        public DueyManager(ILogger<DueyManager> logger, IDbContextFactory<DBContext> dbContextFactory, IMapper mapper, MasterServer server)
            : base(StorageCategory.Duey, dbContextFactory, mapper, logger)
        {
            _server = server;
        }

        protected override int GetKey(DueyPackageDto model) => model.PackageId;

        DueyDto.DueyPackageDto? FindById(int id)
        {
            var dayBefore30 = _server.GetCurrentTimeDateTimeOffset().AddDays(-30);
            var dayBefore30_l = dayBefore30.ToTimestamp();

            return Find(x => x.CreateTime > dayBefore30 && x.ClaimTime == null && x.Id == id,
                x => x.CreateTime > dayBefore30_l && x.ClaimTime == null && x.PackageId == id);
        }

        List<DueyDto.DueyPackageDto> QueryByReceiver(int receiverId)
        {
            var dayBefore30 = _server.GetCurrentTimeDateTimeOffset().AddDays(-30);
            var dayBefore30_l = dayBefore30.ToTimestamp();

            return Query(x => x.CreateTime > dayBefore30 && x.ClaimTime == null && x.ReceiverId == receiverId,
                x => x.CreateTime > dayBefore30_l && x.ClaimTime == null && x.ReceiverId == receiverId);
        }

        public async Task TakeDueyPackage(DueyDto.TakeDueyPackageRequest request)
        {
            var res = new DueyDto.TakeDueyPackageResponse { Request = request };
            var package = FindById(request.PackageId);
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
            var packages = QueryByReceiver(chrId).Where(x => _lockDic.Contains(x.PackageId));
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
                var package = FindById(request.PackageId);
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
            var package = FindById(request.PackageId);
            if (package == null || package.ReceiverId != request.MasterId)
            {
                res.Code = 1;
                return;
            }

            package.ClaimTime = _server.GetCurrentTimeDateTimeOffset().ToTimestamp();
            SetDirty(package);

            await _server.Transport.SendMessageN(ChannelRecvCode.DeleteDueyPackage, res, [request.MasterId]);
        }

        public async Task GetPlayerDueyPackages(GetPlayerDueyPackageRequest request)
        {
            var res = new GetPlayerDueyPackageResponse();
            res.List.AddRange(QueryByReceiver(request.ReceiverId));
            res.ReceiverId = request.ReceiverId;

            await _server.Transport.SendMessageN(ChannelRecvCode.LoadDueyPackage, res, [request.ReceiverId]);
        }

        internal async Task SendDueyNotifyOnLogin(int id)
        {
            var allUnreadData = QueryByReceiver(id).Where(x => !x.Notified).OrderByDescending(x => x.Type);
            var data = allUnreadData.FirstOrDefault();
            if (data != null)
            {
                foreach (var item in allUnreadData)
                {
                    item.Notified = true;

                    SetDirty(item);
                }
                await _server.Transport.SendMessageN(ChannelRecvCode.LoginNotifyDueyPackage, new DueyDto.DueyNotifyDto { Type = data.Type, ReceiverId = data.ReceiverId }, [data.ReceiverId]);
            }
        }
    }
}
