using Application.Core.Channel.Commands;
using Application.Shared.Constants.Buddy;
using Application.Shared.Message;
using BuddyProto;
using Google.Protobuf;
using MessageProto;
using tools;

namespace Application.Core.Channel.Internal.Handlers
{
    internal class BuddyHandlers
    {
        internal class InvokeBuddyAddHandler : InternalSessionChannelHandler<BuddyProto.AddBuddyResponse>
        {
            public InvokeBuddyAddHandler(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnBuddyAdd;

            protected override void HandleMessage(AddBuddyResponse res)
            {
                _server.PushChannelCommand(new InvokeAddBuddyCallbackCommand(res));
            }

            protected override AddBuddyResponse Parse(ByteString data) => AddBuddyResponse.Parser.ParseFrom(data);
        }

        internal class InvokeBuddyDeleteHandler : InternalSessionChannelHandler<BuddyProto.DeleteBuddyResponse>
        {
            public InvokeBuddyDeleteHandler(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnBuddyRemove;

            protected override void HandleMessage(DeleteBuddyResponse res)
            {
                _server.PushChannelCommand(new InvokeRemoveBuddyCallbackCommand(res));
            }

            protected override DeleteBuddyResponse Parse(ByteString data) => DeleteBuddyResponse.Parser.ParseFrom(data);
        }

        internal class GetLocation : InternalSessionChannelHandler<BuddyProto.GetLocationResponse>
        {
            public GetLocation(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnBuddyLocation;

            protected override void HandleMessage(GetLocationResponse res)
            {
                _server.PushChannelCommand(new InvokeBuddyGetLocationCommand(res));
            }

            protected override GetLocationResponse Parse(ByteString data) => GetLocationResponse.Parser.ParseFrom(data);
        }

        internal class Whisper : InternalSessionChannelHandler<MessageProto.SendWhisperMessageResponse>
        {
            public Whisper(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnWhisper;

            protected override void HandleMessage(SendWhisperMessageResponse res)
            {
                _server.PushChannelCommand(new InvokeWhisperCommand(res));
            }

            protected override SendWhisperMessageResponse Parse(ByteString data) => SendWhisperMessageResponse.Parser.ParseFrom(data);
        }
    }
}
