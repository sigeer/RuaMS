using Application.Core.Channel.Commands.Channel;
using Application.Shared.Message;
using Google.Protobuf;
using LifeProto;
using XmlWzReader;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Application.Core.Channel.Internal.Handlers
{
    internal class PLifeHandlers
    {

        public class Create : InternalSessionChannelHandler<CreatePLifeRequest>
        {
            public Create(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnPlifeCreated;

            protected override void HandleMessage(CreatePLifeRequest res)
            {
                _server.PushChannelCommand(new InvokePLifeCreatingCommand(res));
            }

            protected override CreatePLifeRequest Parse(ByteString data) => CreatePLifeRequest.Parser.ParseFrom(data);
        }

        public class Remove : InternalSessionChannelHandler<RemovePLifeResponse>
        {
            public Remove(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnPlifeRemoved;

            protected override void HandleMessage(RemovePLifeResponse res)
            {
                _server.PushChannelCommand(new InvokePLifeRemoveCommand(res));
            }

            protected override RemovePLifeResponse Parse(ByteString data) => RemovePLifeResponse.Parser.ParseFrom(data);
        }
    }
}
