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
using Microsoft.Extensions.Logging;
using ServerProto;
using SystemProto;

namespace Application.Core.Login.Servers
{
    internal class SystemGrpcService : ServiceProto.SystemService.SystemServiceBase
    {
        readonly MasterServer _server;
        readonly MessageService _msgService;
        readonly ILogger<SystemGrpcService> _logger;

        public SystemGrpcService(MasterServer masterServer, MessageService messageService, ILogger<SystemGrpcService> logger)
        {
            _server = masterServer;
            _msgService = messageService;
            _logger = logger;
        }


        public override async Task Connect(IAsyncStreamReader<PacketWrapper> requestStream, IServerStreamWriter<PacketWrapper> responseStream, ServerCallContext context)
        {
            RemoteChannelServerNode? serverNode = null;
            try
            {
                var lastHeartbeat = DateTime.UtcNow;
                _ = Task.Run(async () =>
                {
                    while (!context.CancellationToken.IsCancellationRequested)
                    {
                        if (DateTime.UtcNow - lastHeartbeat > TimeSpan.FromSeconds(10))
                        {
                            // 超时，主动关闭
                            throw new RpcException(
                                new Status(StatusCode.Cancelled, "Heartbeat timeout"));
                        }

                        await Task.Delay(1000);
                    }
                });

                await foreach (var msg in requestStream.ReadAllAsync(context.CancellationToken))
                {
                    lastHeartbeat = DateTime.UtcNow;

                    if (msg.EventId == ChannelSendCode.RegisterChannel)
                    {
                        serverNode = new RemoteChannelServerNode(_server, responseStream, RegisterServerRequest.Parser.ParseFrom(msg.Data));
                        var channelId = _server.AddChannel(serverNode);
                        if (channelId > 0)
                        {
                            await serverNode.SendMessage(ChannelRecvCode.RegisterChannel, new RegisterServerResult
                            {
                                StartChannel = channelId,
                                Coupon = _server.CouponManager.GetConfig(),
                                Config = _server.GetWorldConfig()
                            });
                        }
                        else
                        {
                            await serverNode.SendMessage(ChannelRecvCode.RegisterChannel, new RegisterServerResult
                            {
                                StartChannel = channelId,
                            });
                            serverNode = null;
                        }
                    }
                    else if (serverNode != null)
                    {
                        await serverNode.HandleAsync(msg);
                    }
                }
                _server.RemoveChanelServerNode(serverNode);
            }
            catch (System.IO.IOException io) when (io.Message.Contains("The client reset the request stream."))
            {
                _server.RemoveChanelServerNode(serverNode, false);
            }
            catch (RpcException rpc) when (rpc.StatusCode == StatusCode.Cancelled)
            {
                _server.RemoveChanelServerNode(serverNode, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
        }

        public override async Task<Empty> ShutdownMaster(ShutdownMasterRequest request, ServerCallContext context)
        {
            await _server.Shutdown(request.DelaySeconds);
            return new Empty();
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


        public override Task<Empty> RemoveTimer(Empty request, ServerCallContext context)
        {
            _server.Transport.BroadcastMessage(BroadcastType.Broadcast_RemoveTimer, new MessageProto.RemoveTimer());
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
            return Task.FromResult(_server.CrossServerService.WarpPlayerByName(request));
        }

        public override Task<Empty> HealthCheck(MonitorData request, ServerCallContext context)
        {
            var serverName = context.RequestHeaders.Get("x-server-name")?.Value;
            if (serverName != null && _server.ChannelServerList.TryGetValue(serverName, out var node))
            {
                node.HealthCheck(request);
            }
            return Task.FromResult(new Empty());
        }
    }
}
