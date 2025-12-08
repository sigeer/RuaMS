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

        public abstract Task BroadcastMessageN<TMessage>(int type, TMessage message) where TMessage : IMessage;
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


        public async Task Handle(PacketWrapper packet)
        {
            if (packet.EventId == ChannelSendCode.DisconnectAll)
                await SendAsync(ChannelRecvCode.DisconnectAll);

            if (packet.EventId == ChannelSendCode.SaveAll)
                await SendAsync(ChannelRecvCode.SaveAll);


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

        public async Task SendAsync(int type, IMessage realMessage)
        {
            await _writer.WriteAsync(new PacketWrapper
            {
                EventId = type,
                Data = realMessage.ToByteString()
            });
        }

        public async Task SendAsync(int type)
        {
            await _writer.WriteAsync(new PacketWrapper
            {
                EventId = type
            });
        }

        public override void BroadcastMessage<TMessage>(string type, TMessage message)
        {
            _client.BroadcastMesssage(new BaseProto.MessageWrapper { Type = type, Content = message.ToByteString() });
        }

        public override async Task BroadcastMessageN<TMessage>(int type, TMessage message)
        {
            await SendAsync(type, message);
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
