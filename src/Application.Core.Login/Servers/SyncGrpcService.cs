using Application.Core.Login.Services;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace Application.Core.Login.Servers
{
    internal class SyncGrpcService : ProtoService.SyncService.SyncServiceBase
    {
        readonly MasterServer _server;
        readonly LoginService _loginService;
        public SyncGrpcService(MasterServer server, LoginService loginService)
        {
            _server = server;
            _loginService = loginService;
        }
        public override Task<Empty> BatchSyncPlayerShop(ProtoService.BatchSyncPlayerShopRequest request, ServerCallContext context)
        {
            foreach (var item in request.List)
            {
                _server.PlayerShopManager.SyncPlayerStorage(item);
            }
            return Task.FromResult(new Empty());
        }

        public override Task<ProtoModel.PlayerBuffProto> GetPlayerBuffers(ProtoService.GetPlayerBufferRequest request, ServerCallContext context)
        {
            return Task.FromResult(_server.BuffManager.Get(request.CharacterId));
        }

        public override Task<ProtoService.GetPlayerByLoginResponse> GetPlayerObject(ProtoService.GetPlayerByLoginRequest request, ServerCallContext context)
        {
            return Task.FromResult(new ProtoService.GetPlayerByLoginResponse { Data = _loginService.PlayerLogin(request.ClientSession, request.CharacterId) });
        }

        public override Task<ProtoModel.BoolWrapper> HasCharacterInTransition(ProtoService.CheckCharacterInTransitionRequest request, ServerCallContext context)
        {
            return Task.FromResult(new ProtoModel.BoolWrapper { Value = _server.HasCharacteridInTransition(request.ClientSession) });
        }

        public override Task<Empty> PushPlayerBuffers(ProtoService.PushPlayerBuffsRequest request, ServerCallContext context)
        {
            _server.BuffManager.SaveBuff(request.CharacterId, request.Data);
            return Task.FromResult(new Empty());
        }

        public override Task<Empty> SetCharacterTransition(ProtoService.SetClientCharacterTransitionRequest request, ServerCallContext context)
        {
            _server.SetCharacteridInTransition(request.ClientSession, request.CharacterId);
            return Task.FromResult(new Empty());
        }

        public override Task<Empty> SyncPlayerShop(ProtoService.SyncPlayerShopRequest request, ServerCallContext context)
        {
            _server.PlayerShopManager.SyncPlayerStorage(request);
            return Task.FromResult(new Empty());
        }

        public override Task<ProtoModel.AccountLoginStateProto> UpdateAccountState(ProtoService.UpdateAccountStateRequest request, ServerCallContext context)
        {
            var data = _server.AccountManager.UpdateAccountState(request.AccId, (sbyte)request.State);
            return Task.FromResult(new ProtoModel.AccountLoginStateProto { State = data.State, AccId = request.AccId, Time = Timestamp.FromDateTimeOffset(data.ProcessTime) });
        }
    }
}
