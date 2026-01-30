using Application.Shared.Message;
using Dto;
using Google.Protobuf;

namespace Application.Core.Login.Internal.Handlers
{
    internal class BuffHandlers
    {
        public class RemoveDoor : InternalSessionMasterHandler<RemoveDoorRequest>
        {
            public RemoveDoor(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.RemoveDoor;

            protected override void HandleMessage(RemoveDoorRequest res)
            {
                _ = _server.Transport.BroadcastMessageN(ChannelRecvCode.OnDoorRemoved, new RemoveDoorResponse { OwnerId = res.OwnerId });
            }

            protected override RemoveDoorRequest Parse(ByteString data) => RemoveDoorRequest.Parser.ParseFrom(data);
        }
    }
}
