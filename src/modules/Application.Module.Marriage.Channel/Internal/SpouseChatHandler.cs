using Application.Core.Channel;
using Application.Core.Channel.Internal;
using Application.Module.Marriage.Channel.Commands;
using Application.Module.Marriage.Common;
using Google.Protobuf;
using MarriageProto;

namespace Application.Module.Marriage.Channel.Internal
{
    internal class SpouseChatHandler : InternalSessionChannelHandler<MarriageProto.SendSpouseChatResponse>
    {
        readonly MarriageManager _marriageManager;
        public SpouseChatHandler(WorldChannelServer server, MarriageManager marriageManager) : base(server)
        {
            _marriageManager = marriageManager;
        }

        public override int MessageId => MasterSend.SpouseChat;

        protected override Task HandleMessage(SendSpouseChatResponse res, CancellationToken cancellationToken = default)
        {
            _server.PushChannelCommand(new InvokeSpouseChatCommand(res));
            return Task.CompletedTask;
        }

        protected override SendSpouseChatResponse Parse(ByteString data) => SendSpouseChatResponse.Parser.ParseFrom(data);
    }
}
