using Application.Core.Login.Services;
using Application.Shared.Message;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace Application.Core.Login.Servers
{
    internal class SystemGrpcService : ProtoService.SystemService.SystemServiceBase
    {
        readonly MasterServer _server;
        readonly ReportService _msgService;
        readonly ILogger<SystemGrpcService> _logger;

        public SystemGrpcService(MasterServer masterServer, ReportService messageService, ILogger<SystemGrpcService> logger)
        {
            _server = masterServer;
            _msgService = messageService;
            _logger = logger;
        }


        public override async Task Connect(IAsyncStreamReader<ProtoModel.PacketWrapper> requestStream, IServerStreamWriter<ProtoModel.PacketWrapper> responseStream, ServerCallContext context)
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

                    if (msg.EventId == (int)ChannelSendCode.RegisterChannel)
                    {
                        serverNode = new RemoteChannelServerNode(_server, responseStream, ProtoService.RegisterServerRequest.Parser.ParseFrom(msg.Data));
                        var channelId = _server.AddChannel(serverNode);
                        if (channelId > 0)
                        {
                            await serverNode.SendMessage(msg.EventId, new ProtoModel.RegisterServerResultProto
                            {
                                StartChannel = channelId,
                                Config = _server.GetWorldConfig()
                            });
                        }
                        else
                        {
                            await serverNode.SendMessage(msg.EventId, new ProtoModel.RegisterServerResultProto
                            {
                                StartChannel = channelId,
                            });
                            serverNode = null;
                        }
                    }
                    else if (serverNode != null)
                    {
                        serverNode.HandleAsync(msg);
                    }
                }
                _server.RemoveChanelServerNode(serverNode);
            }
            catch (System.IO.IOException io) when (io.Message.Contains("The client reset the request stream.") || io.Message.Contains("The request stream was aborted."))
            {
                _server.RemoveChanelServerNode(serverNode, false);
            }
            catch (RpcException rpc) when (rpc.StatusCode == StatusCode.Cancelled)
            {
                _server.RemoveChanelServerNode(serverNode, false);
            }
            catch (Exception ex)
            {
                _server.RemoveChanelServerNode(serverNode, false);
                _logger.LogError(ex.ToString());
            }
        }

        public override async Task<Empty> ShutdownMaster(ProtoService.ShutdownMasterRequest request, ServerCallContext context)
        {
            await _server.Shutdown(request.DelaySeconds);
            return new Empty();
        }


        public override Task<ProtoModel.AutoBanIgnoredWrapperProto> GetAutobanIgnores(Empty request, ServerCallContext context)
        {
            return Task.FromResult(_server.SystemManager.LoadAutobanIgnoreData());
        }

        public override Task<ProtoModel.IPEndPointProto> GetChannelEndPoint(ProtoService.GetChannelEndPointRequest request, ServerCallContext context)
        {
            var ipep = _server.GetChannelIPEndPoint(request.Channel);
            return Task.FromResult(new ProtoModel.IPEndPointProto { Address = ByteString.CopyFrom(ipep.Address.GetAddressBytes()), Port = ipep.Port });
        }

        public override Task<ProtoModel.TimeWrapper> GetCurrentTime(Empty request, ServerCallContext context)
        {
            return Task.FromResult(new ProtoModel.TimeWrapper { Value = _server.getCurrentTime() });
        }

        public override Task<ProtoModel.TimeWrapper> GetCurrentTimestamp(Empty request, ServerCallContext context)
        {
            return Task.FromResult(new ProtoModel.TimeWrapper { Value = _server.getCurrentTimestamp() });
        }

        public override Task<ProtoModel.GetAllClientInfo> GetOnlinedClients(Empty request, ServerCallContext context)
        {
            return Task.FromResult(_server.AccountManager.GetOnliendClientInfo());
        }

        public override Task<ProtoService.ShowOnlinePlayerResponse> GetOnlinedPlayers(Empty request, ServerCallContext context)
        {
            return Task.FromResult(_server.CharacterManager.GetOnlinedPlayers());
        }

        public override Task<ProtoModel.ServerStateProto> GetServerState(Empty request, ServerCallContext context)
        {
            return Task.FromResult(_server.GetServerStats());
        }


        public override Task<ProtoService.SetFlyResponse> SetAccountFly(ProtoService.SetFlyRequest request, ServerCallContext context)
        {
            return Task.FromResult(_server.AccountManager.SetFly(request));
        }
        public override Task<Empty> HealthCheck(ProtoModel.MonitorData request, ServerCallContext context)
        {
            var serverName = context.RequestHeaders.Get("x-server-name")?.Value;
            if (serverName != null && _server.ChannelServerList.TryGetValue(serverName, out var node))
            {
                node.HealthCheck(request);
            }
            return Task.FromResult(new Empty());
        }

        public override Task<ProtoService.GainAccountCharacterSlotResponse> GainCharacterSlot(ProtoService.GainAccountCharacterSlotRequest request, ServerCallContext context)
        {
            _server.AccountManager.GainCharacterSlot(request.AccId);
            return base.GainCharacterSlot(request, context);
        }
    }
}
