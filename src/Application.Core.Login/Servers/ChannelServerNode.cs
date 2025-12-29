using Application.EF.Entities;
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
using System.Collections.Concurrent;
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
        public abstract Task SendMessage<TMessage>(int type, TMessage message, CancellationToken cancellationToken = default) where TMessage : IMessage;
        public abstract Task SendMessage(int type, CancellationToken cancellationToken = default);
    }


    public class RemoteChannelServerNode : ChannelServerNode
    {
        private readonly IServerStreamWriter<BaseProto.PacketWrapper> _writer;
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
        }


        public async Task HandleAsync(PacketWrapper packet, CancellationToken cancellationToken = default)
        {
            await _server.MessageDispatcherV.DispatchAsync(packet.EventId, packet.Data, cancellationToken);
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

        public override async Task SendMessage<TMessage>(int type, TMessage message, CancellationToken cancellationToken = default)
        {
            await SendAsync(type, message, cancellationToken);
        }

        public override async Task SendMessage(int type, CancellationToken cancellationToken = default)
        {
            await SendAsync(type, cancellationToken);
        }
    }
}
