using Application.Shared.Internal;
using Application.Shared.Message;
using CreatorProto;
using Google.Protobuf;

namespace Application.Core.Login.Internal.Handlers
{
    internal class CreateChacterResponseHandler : InternalSessionMasterHandler<CreateCharResponseDto>
    {
        public CreateChacterResponseHandler(MasterServer server) : base(server)
        {
        }

        public override int MessageId => (int)ChannelSendCode.CreateCharacterResponse;

        protected override void HandleMessage(CreateCharResponseDto data)
        {
            _server.HandleCreateCharacterResponse(data);
        }

        protected override CreateCharResponseDto Parse(ByteString content) => CreateCharResponseDto.Parser.ParseFrom(content);
    }
}
