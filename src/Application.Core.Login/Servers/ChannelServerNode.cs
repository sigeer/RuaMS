using Application.Shared.Message;
using Application.Shared.Servers;
using BaseProto;
using Config;
using CreatorProto;
using Dto;
using ExpeditionProto;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using MessageProto;
using Microsoft.AspNetCore.Hosting.Server;
using SyncProto;
using System.Threading.Tasks;
using SystemProto;

namespace Application.Core.Login.Servers
{
    public abstract class ChannelServerNode
    {
        public string ServerHost { get; protected set; } = null!;
        public string ServerName { get; protected set; } = null!;
        public List<ChannelConfig> ServerConfigs { get; protected set; } = null!;

        public ServerProto.MonitorData? MonitorData { get; protected set; }
        public DateTimeOffset LastPingTime { get; set; }

        public void HealthCheck(ServerProto.MonitorData data)
        {
            LastPingTime = DateTimeOffset.Now;
            MonitorData = data;
        }


        public abstract void BroadcastMessage<TMessage>(string type, TMessage message) where TMessage : IMessage;
        public abstract CreatorProto.CreateCharResponseDto CreateCharacterFromChannel(CreatorProto.CreateCharRequestDto request);
        public abstract ExpeditionProto.QueryChannelExpedtionResponse GetExpeditionInfo();

        public abstract Task SendMessage<TMessage>(int type, TMessage message, CancellationToken cancellationToken = default) where TMessage : IMessage;
        public abstract Task SendMessage(int type, CancellationToken cancellationToken = default);
    }


    public class RemoteChannelServerNode : ChannelServerNode
    {
        private readonly IServerStreamWriter<BaseProto.PacketWrapper> _writer;
        ServiceProto.Master2ChannelService.Master2ChannelServiceClient _client;
        readonly MasterServer _server;
        public RemoteChannelServerNode(
            MasterServer server, 
            IServerStreamWriter<BaseProto.PacketWrapper> writer, 
            RegisterServerRequest request)
        {
            _server = server;
            _writer = writer;

            ServerHost = request.ServerHost;
            ServerName = request.ServerName;
            ServerConfigs = request.Channels.Select(x => new ChannelConfig { Port = x.Port, MaxSize = x.MaxSize }).ToList();
            _client = new ServiceProto.Master2ChannelService.Master2ChannelServiceClient(GrpcChannel.ForAddress(request.GrpcUrl));
        }


        public async Task HandleAsync(PacketWrapper packet)
        {
            if (packet.EventId == ChannelSendCode.DisconnectAll)
            {
                await _server.Transport.BroadcastMessageN(ChannelRecvCode.DisconnectAll);
            }

            if (packet.EventId == ChannelSendCode.SaveAll)
            {
                await _server.Transport.BroadcastMessageN(ChannelRecvCode.SaveAll);
            }

            if (packet.EventId == ChannelSendCode.SyncMap)
            {
                _server.CharacterManager.BatchUpdateMap(MapBatchSyncDto.Parser.ParseFrom(packet.Data).List.ToList());
            }

            if (packet.EventId == ChannelSendCode.MultiChat)
            {
                var data = MultiChatMessage.Parser.ParseFrom(packet.Data);
                if (data.Type == 0)
                    await _server.BuddyManager.SendBuddyChatAsync(data.FromName, data.Text, data.Receivers.ToArray());
                else if (data.Type == 1)
                    await _server.TeamManager.SendTeamChatAsync(data.FromName, data.Text);
                else if (data.Type == 2)
                    await _server.GuildManager.SendGuildChatAsync(data.FromName, data.Text);
                else if (data.Type == 3)
                    await _server.GuildManager.SendAllianceChatAsync(data.FromName, data.Text);
            }
        }

        async Task SendAsync(int type, IMessage realMessage, CancellationToken cancellationToken = default)
        {
            await _writer.WriteAsync(new PacketWrapper
            {
                EventId = type,
                Data = realMessage.ToByteString()
            }, cancellationToken);
        }

        async Task SendAsync(int type, CancellationToken cancellationToken = default)
        {
            await _writer.WriteAsync(new PacketWrapper
            {
                EventId = type
            }, cancellationToken);
        }

        public override void BroadcastMessage<TMessage>(string type, TMessage message)
        {
            _client.BroadcastMesssage(new BaseProto.MessageWrapper { Type = type, Content = message.ToByteString() });
        }

        public override async Task SendMessage<TMessage>(int type, TMessage message, CancellationToken cancellationToken = default)
        {
            await SendAsync(type, message, cancellationToken);
        }

        public override async Task SendMessage(int type, CancellationToken cancellationToken = default)
        {
            await SendAsync(type, cancellationToken);
        }

        public override CreateCharResponseDto CreateCharacterFromChannel(CreateCharRequestDto request)
        {
            return _client.CreateCharacterFromChannel(request);
        }

        public override QueryChannelExpedtionResponse GetExpeditionInfo()
        {
            return _client.GetExpeditionInfo(new Google.Protobuf.WellKnownTypes.Empty());
        }
    }
}
