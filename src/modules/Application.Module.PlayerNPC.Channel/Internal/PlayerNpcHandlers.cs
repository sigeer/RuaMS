using Application.Core.Channel;
using Application.Core.Channel.Internal;
using Application.Module.PlayerNPC.Channel.Commands;
using Google.Protobuf;
using LifeProto;

namespace Application.Module.PlayerNPC.Channel.Internal
{
    internal class PlayerNpcHandlers
    {
        public class Remove : InternalSessionChannelHandler<UpdateMapPlayerNPCResponse>
        {
            public Remove(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => 101;

            protected override void HandleMessage(UpdateMapPlayerNPCResponse res)
            {
                _server.PushChannelCommand(new InvokePlayerNpcRemoveCommand(res));
                _server.Post(new InvokeLoadAllDataCommand());
            }

            protected override UpdateMapPlayerNPCResponse Parse(ByteString data) => UpdateMapPlayerNPCResponse.Parser.ParseFrom(data);
        }

        public class Clear : InternalSessionChannelHandler<RemoveAllPlayerNPCResponse>
        {
            public Clear(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => 102;

            protected override void HandleMessage(RemoveAllPlayerNPCResponse res)
            {
                _server.PushChannelCommand(new InvokePlayerNpcClearCommand(res));
                _server.Post(new InvokeLoadAllDataCommand());
            }

            protected override RemoveAllPlayerNPCResponse Parse(ByteString data) => RemoveAllPlayerNPCResponse.Parser.ParseFrom(data);
        }

        public class Refresh : InternalSessionChannelHandler<UpdateMapPlayerNPCResponse>
        {
            public Refresh(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => 103;

            protected override void HandleMessage(UpdateMapPlayerNPCResponse res)
            {
                _server.PushChannelCommand(new InvokePlayerNpcRefreshCommand(res));
                _server.Post(new InvokeLoadAllDataCommand());
            }

            protected override UpdateMapPlayerNPCResponse Parse(ByteString data) => UpdateMapPlayerNPCResponse.Parser.ParseFrom(data);
        }
    }
}
