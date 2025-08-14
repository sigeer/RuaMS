using Application.Core.Login.Services;
using Application.Shared.Message;
using BaseProto;
using Config;
using ConfigProto;
using Dto;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MessageProto;
using Microsoft.EntityFrameworkCore;
using SystemProto;

namespace Application.Core.Login.Servers
{
    internal class SystemGrpcService : ServiceProto.SystemService.SystemServiceBase
    {
        readonly MasterServer _server;
        readonly MessageService _msgService;

        public SystemGrpcService(MasterServer masterServer, MessageService messageService)
        {
            _server = masterServer;
            _msgService = messageService;
        }

        public override async Task<Empty> ShutdownMaster(ShutdownMasterRequest request, ServerCallContext context)
        {
            await _server.Shutdown(request.DelaySeconds);
            return new Empty();
        }

        public override Task<Empty> CompleteChannelShutdown(ChannelShutdownCallback request, ServerCallContext context)
        {
            _server.CompleteChannelShutdown(request.ServerName);
            return Task.FromResult(new Empty());
        }

        public override Task<BanResponse> Ban(BanRequest request, ServerCallContext context)
        {
            return Task.FromResult(_server.AccountBanManager.Ban(request));
        }

        public override Task<Empty> BroadcastMessage(PacketRequest request, ServerCallContext context)
        {
            _server.BroadcastPacket(request);
            return Task.FromResult(new Empty());
        }


        public override Task<Empty> DisconnectAll(DisconnectAllRequest request, ServerCallContext context)
        {
            _server.DisconnectAll(request);
            return Task.FromResult(new Empty());
        }

        public override Task<DisconnectPlayerByNameResponse> DisconnectPlayer(DisconnectPlayerByNameRequest request, ServerCallContext context)
        {
            return Task.FromResult(_server.CrossServerService.DisconnectPlayerByName(request));
        }

        public override Task<Empty> DropMessage(DropMessageRequest request, ServerCallContext context)
        {
            _server.DropWorldMessage(request.Type, request.Message, request.OnlyGM);
            return Task.FromResult(new Empty());
        }

        public override Task<AutoBanIgnoredWrapper> GetAutobanIgnores(Empty request, ServerCallContext context)
        {
            return Task.FromResult(_server.SystemManager.LoadAutobanIgnoreData());
        }

        public override Task<IPEndPointDto> GetChannelEndPoint(GetChannelEndPointRequest request, ServerCallContext context)
        {
            var ipep = _server.GetChannelIPEndPoint(request.Channel);
            return Task.FromResult(new IPEndPointDto { Address = ByteString.CopyFrom(ipep.Address.GetAddressBytes()), Port = ipep.Port });
        }

        public override Task<TimeWrapper> GetCurrentTime(Empty request, ServerCallContext context)
        {
            return Task.FromResult(new TimeWrapper { Value = _server.getCurrentTime() });
        }

        public override Task<TimeWrapper> GetCurrentTimestamp(Empty request, ServerCallContext context)
        {
            return Task.FromResult(new TimeWrapper { Value = _server.getCurrentTimestamp() });
        }

        public override Task<GetAllClientInfo> GetOnlinedClients(Empty request, ServerCallContext context)
        {
            return Task.FromResult(_server.AccountManager.GetOnliendClientInfo());
        }

        public override Task<ShowOnlinePlayerResponse> GetOnlinedPlayers(Empty request, ServerCallContext context)
        {
            return Task.FromResult(_server.CharacterManager.GetOnlinedPlayers());
        }

        public override Task<ServerStateDto> GetServerState(Empty request, ServerCallContext context)
        {
            return Task.FromResult(_server.GetServerStats());
        }

        public override Task<RegisterServerResult> RegisterServer(RegisterServerRequest request, ServerCallContext context)
        {
            var channelGrpcHost = context.GetHttpContext().Connection.RemoteIpAddress!.ToString();
            var channelId = _server.AddChannel(new RemoteWorldChannel(request.ServerName, request.ServerHost,
                channelGrpcHost, request.GrpcPort,
                request.Channels.Select(x => new Application.Shared.Servers.ChannelConfig { MaxSize = x.MaxSize, Port = x.Port }).ToList()));
            return Task.FromResult(new RegisterServerResult
            {
                StartChannel = channelId,
                Coupon = _server.CouponManager.GetConfig(),
                Config = _server.GetWorldConfig()
            });
        }

        public override Task<Empty> RemoveTimer(Empty request, ServerCallContext context)
        {
            _server.Transport.BroadcastMessage(BroadcastType.Broadcast_RemoveTimer, new MessageProto.RemoveTimer());
            return Task.FromResult(new Empty());
        }

        public override Task<Empty> SaveAll(Empty request, ServerCallContext context)
        {
            _server.Transport.BroadcastMessage(BroadcastType.SaveAll, new Empty());
            return Task.FromResult(new Empty());
        }

        public override Task<SendReportResponse> SendReport(SendReportRequest request, ServerCallContext context)
        {
            return Task.FromResult(_msgService.AddReport(request));
        }

        public override Task<Empty> SendTimer(SetTimer request, ServerCallContext context)
        {
            _server.Transport.BroadcastMessage(BroadcastType.Broadcast_SetTimer, new MessageProto.SetTimer { Seconds = request.Seconds });
            return Task.FromResult(new Empty());
        }

        public override Task<Empty> SendWorldConfig(WorldConfig request, ServerCallContext context)
        {
            _server.UpdateWorldConfig(request);
            return Task.FromResult(new Empty());
        }

        public override Task<SetFlyResponse> SetAccountFly(SetFlyRequest request, ServerCallContext context)
        {
            return Task.FromResult(_server.AccountManager.SetFly(request));
        }

        public override Task<ToggleAutoBanIgnoreResponse> SetAutobanIgnore(ToggleAutoBanIgnoreRequest request, ServerCallContext context)
        {
            return Task.FromResult(_server.SystemManager.ToggleAutoBanIgnored(request));
        }

        public override Task<SetGmLevelResponse> SetGmLevel(SetGmLevelRequest request, ServerCallContext context)
        {
            return Task.FromResult(_server.AccountManager.SetGmLevel(request));
        }

        public override Task<SummonPlayerByNameResponse> SummonPlayer(SummonPlayerByNameRequest request, ServerCallContext context)
        {
            return Task.FromResult(_server.CrossServerService.SummonPlayerByName(request));
        }

        public override Task<UnbanResponse> Unban(UnbanRequest request, ServerCallContext context)
        {
            return Task.FromResult(_server.AccountBanManager.Unban(request));
        }

        public override Task<WrapPlayerByNameResponse> WrapPlayer(WrapPlayerByNameRequest request, ServerCallContext context)
        {
            return base.WrapPlayer(request, context);
        }
    }
}
