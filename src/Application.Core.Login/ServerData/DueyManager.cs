using Application.Core.Login.Shared;
using Application.EF;
using Application.EF.Entities;
using Application.Shared.Items;
using Application.Shared.Message;
using Application.Utility;
using Google.Protobuf.WellKnownTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Reflection;

namespace Application.Core.Login.ServerData
{
    public class DueyManager : DataStorageBase<int, ProtoModel.DueyPackageProto, DueyPackageEntity>
    {
        readonly MasterServer _server;

        HashSet<int> _lockDic = new();
        public DueyManager(ILogger<DueyManager> logger, IDbContextFactory<DBContext> dbContextFactory, IMapper mapper, MasterServer server)
            : base(StorageCategory.Duey, dbContextFactory, mapper, logger)
        {
            _server = server;
        }

        protected override int GetKey(ProtoModel.DueyPackageProto model) => model.PackageId;

        ProtoModel.DueyPackageProto? FindById(int id)
        {
            var dayBefore30 = _server.GetCurrentTimeDateTimeOffset().AddDays(-30);
            var dayBefore30_l = dayBefore30.ToTimestamp();

            return Find(x => x.CreateTime > dayBefore30 && x.ClaimTime == null && x.Id == id,
                x => x.CreateTime > dayBefore30_l && x.ClaimTime == null && x.PackageId == id);
        }

        List<ProtoModel.DueyPackageProto> QueryByReceiver(int receiverId)
        {
            var dayBefore30 = _server.GetCurrentTimeDateTimeOffset().AddDays(-30);
            var dayBefore30_l = dayBefore30.ToTimestamp();

            return Query(x => x.CreateTime > dayBefore30 && x.ClaimTime == null && x.ReceiverId == receiverId,
                x => x.CreateTime > dayBefore30_l && x.ClaimTime == null && x.ReceiverId == receiverId);
        }

        public async Task TakeDueyPackage(ProtoService.TakeDueyPackageRequest request)
        {
            var res = new ProtoService.TakeDueyPackageResponse { Request = request };
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

        ProtoModel.DueyPackageProto MapToDto(ProtoModel.DueyPackageProto dto)
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

        public async Task TakeDueyPackageCommit(ProtoService.TakeDueyPackageCommitRequest request)
        {
            if (request.Success)
            {
                await RemovePackage(new ProtoService.RemovePackageRequest { MasterId = request.MasterId, PackageId = request.PackageId, ByReceived = true });
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


        public async Task<ProtoService.CreatePackageResponse> CreateDueyPackage(ProtoService.CreatePackageRequest request)
        {
            var res = new ProtoService.CreatePackageResponse();
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
                await SendDueyPackage(request.SenderId, target.Character.Id, request.Item, request.SendMeso, request.Quick, request.SendMessage);
                return res;
            }
        }

        ProtoModel.DueyPackageProto CreateDueyPackageModel(int senderId, int reciverId, ProtoModel.ItemProto? item, int meso, bool quick, string? message)
        {
            // Q.为什么特快是提前一天？而不是让普通包裹推迟一天？
            var time = DateTimeOffset.FromUnixTimeMilliseconds(_server.getCurrentTime());
            if (quick)
                time = time.AddDays(-1);

            var model = new ProtoModel.DueyPackageProto()
            {
                PackageId = Interlocked.Increment(ref _localId),
                ReceiverId = reciverId,
                SenderId = senderId,
                Mesos = meso,
                Message = message ?? "",
                Type = quick,
                Notified = false,
                CreateTime = time.ToTimestamp(),
                Item = item
            };
            return model;
        }


        public async Task SendDueyPackage(int senderId, int reciverId, ProtoModel.ItemProto? item, int meso, bool quick, string? message)
        {
            var model = CreateDueyPackageModel(senderId, reciverId, item, meso, quick, message);

            SetDirty(model);

            var data = new ProtoService.CreatePackageNotifyResponse { Package = model };
            await _server.Transport.SendMessageN(ChannelRecvCode.CreateDueyPackage, data, [model.ReceiverId]);
        }

        public async Task RemovePackage(ProtoService.RemovePackageRequest request)
        {
            var res = new ProtoService.RemovePackageResponse { Code = 0, Request = request };
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

        public async Task GetPlayerDueyPackages(ProtoService.GetPlayerDueyPackageRequest request)
        {
            var res = new ProtoService.GetPlayerDueyPackageResponse();
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
                await _server.Transport.SendMessageN(ChannelRecvCode.LoginNotifyDueyPackage, new ProtoModel.DueyNotifyProto { Type = data.Type, ReceiverId = data.ReceiverId }, [data.ReceiverId]);
            }
        }
    }
}
