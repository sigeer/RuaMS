using Application.Core.Channel;
using Application.Core.Channel.Internal;
using Application.Module.PlayerNPC.Channel.Commands;
using Google.Protobuf;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Module.PlayerNPC.Channel.Internal
{
    internal class PlayerNpcHandlers
    {
        public class Remove : InternalSessionChannelHandler<ProtoService.UpdateMapPlayerNPCResponse>
        {
            public Remove(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => 101;

            protected override Task HandleMessage(ProtoService.UpdateMapPlayerNPCResponse res)
            {
                _server.PushChannelCommand(new InvokePlayerNpcRemoveCommand(res));
                _server.Send(s =>
                {
                    s.ServiceProvider.GetRequiredService<PlayerNPCManager>().LoadAllData();
                });
            return Task.CompletedTask;
            }

            protected override ProtoService.UpdateMapPlayerNPCResponse Parse(ByteString data) => ProtoService.UpdateMapPlayerNPCResponse.Parser.ParseFrom(data);
        }

        public class Clear : InternalSessionChannelHandler<ProtoService.RemoveAllPlayerNPCResponse>
        {
            public Clear(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => 102;

            protected override Task HandleMessage(ProtoService.RemoveAllPlayerNPCResponse res)
            {
                _server.PushChannelCommand(new InvokePlayerNpcClearCommand(res));
                _server.Send(s =>
                {
                    s.ServiceProvider.GetRequiredService<PlayerNPCManager>().LoadAllData();
                });
            return Task.CompletedTask;
            }

            protected override ProtoService.RemoveAllPlayerNPCResponse Parse(ByteString data) => ProtoService.RemoveAllPlayerNPCResponse.Parser.ParseFrom(data);
        }

        public class Refresh : InternalSessionChannelHandler<ProtoService.UpdateMapPlayerNPCResponse>
        {
            public Refresh(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => 103;

            protected override Task HandleMessage(ProtoService.UpdateMapPlayerNPCResponse res)
            {
                _server.PushChannelCommand(new InvokePlayerNpcRefreshCommand(res));
                _server.Send(s =>
                {
                    s.ServiceProvider.GetRequiredService<PlayerNPCManager>().LoadAllData();
                });
            return Task.CompletedTask;
            }

            protected override ProtoService.UpdateMapPlayerNPCResponse Parse(ByteString data) => ProtoService.UpdateMapPlayerNPCResponse.Parser.ParseFrom(data);
        }
    }
}
