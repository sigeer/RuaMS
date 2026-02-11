using Application.Core.Login.Services;
using Application.Shared.Events;
using BaseProto;
using CreatorProto;
using Dto;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using ItemProto;
using ServiceProto;
using SyncProto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Core.Login.Servers
{
    internal class SyncGrpcService : ServiceProto.SyncService.SyncServiceBase
    {
        readonly MasterServer _server;
        readonly LoginService _loginService;
        public SyncGrpcService(MasterServer server, LoginService loginService)
        {
            _server = server;
            _loginService = loginService;
        }
        public override Task<Empty> BatchSyncPlayerShop(BatchSyncPlayerShopRequest request, ServerCallContext context)
        {
            foreach (var item in request.List)
            {
                _server.PlayerShopManager.SyncPlayerStorage(item);
            }
            return Task.FromResult(new Empty());
        }

        public override Task<PlayerBuffDto> GetPlayerBuffers(GetPlayerBufferRequest request, ServerCallContext context)
        {
            return Task.FromResult(_server.BuffManager.Get(request.CharacterId));
        }

        public override Task<GetPlayerByLoginResponse> GetPlayerObject(GetPlayerByLoginRequest request, ServerCallContext context)
        {
            return Task.FromResult(new GetPlayerByLoginResponse { Data = _loginService.PlayerLogin(request.ClientSession, request.CharacterId) });
        }

        public override Task<BoolWrapper> HasCharacterInTransition(CheckCharacterInTransitionRequest request, ServerCallContext context)
        {
            return Task.FromResult(new BoolWrapper { Value = _server.HasCharacteridInTransition(request.ClientSession) });
        }

        public override Task<Empty> PushPlayerBuffers(PushPlayerBuffsRequest request, ServerCallContext context)
        {
            _server.BuffManager.SaveBuff(request.CharacterId, request.Data);
            return Task.FromResult(new Empty());
        }

        public override Task<Empty> SetCharacterTransition(SetClientCharacterTransitionRequest request, ServerCallContext context)
        {
            _server.SetCharacteridInTransition(request.ClientSession, request.CharacterId);
            return Task.FromResult(new Empty());
        }

        public override Task<Empty> SyncPlayerShop(SyncPlayerShopRequest request, ServerCallContext context)
        {
            _server.PlayerShopManager.SyncPlayerStorage(request);
            return Task.FromResult(new Empty());
        }

        public override Task<AccountLoginStateDto> UpdateAccountState(UpdateAccountStateRequest request, ServerCallContext context)
        {
            var data = _server.AccountManager.UpdateAccountState(request.AccId, (sbyte)request.State);
            return Task.FromResult(new AccountLoginStateDto { State = data.State, AccId = request.AccId, Time = Timestamp.FromDateTimeOffset(data.ProcessTime) });
        }
    }
}
