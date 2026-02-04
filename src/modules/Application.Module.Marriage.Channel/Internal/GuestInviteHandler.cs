using Application.Core.Channel;
using Application.Core.Channel.Internal;
using Application.Module.Marriage.Channel.Commands;
using Application.Module.Marriage.Common;
using Google.Protobuf;
using MarriageProto;

namespace Application.Module.Marriage.Channel.Internal
{
    internal class GuestInviteHandler : InternalSessionChannelHandler<MarriageProto.InviteGuestResponse>
    {
        readonly WeddingManager _weddingManager;
        public GuestInviteHandler(WorldChannelServer server, WeddingManager weddingManager) : base(server)
        {
            _weddingManager = weddingManager;
        }

        public override int MessageId => MasterSend.WeddingInviteGuest;

        protected override Task HandleMessage(InviteGuestResponse res, CancellationToken cancellationToken = default)
        {
            _server.PushChannelCommand(new InvokeGuestInvitationCommand(res));
            return Task.CompletedTask;
        }

        protected override InviteGuestResponse Parse(ByteString data) => InviteGuestResponse.Parser.ParseFrom(data);
    }
}
