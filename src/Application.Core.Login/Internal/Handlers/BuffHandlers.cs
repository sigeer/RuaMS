using Application.Shared.Message;
using Google.Protobuf;

namespace Application.Core.Login.Internal.Handlers
{
    internal class BuffHandlers
    {
        public class RemoveDoor : InternalSessionMasterHandler<ProtoService.RemoveDoorRequest>
        {
            public RemoveDoor(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.RemoveDoor;

            protected override Task HandleMessage(ProtoService.RemoveDoorRequest res)
            {
                return _server.Transport.BroadcastMessageN(ChannelRecvCode.OnDoorRemoved, new ProtoService.RemoveDoorResponse { OwnerId = res.OwnerId });
            }

            protected override ProtoService.RemoveDoorRequest Parse(ByteString data) => ProtoService.RemoveDoorRequest.Parser.ParseFrom(data);
        }
    }
}
