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

        public override int MessageId => ChannelSendCode.CreateCharacterResponse;

        protected override Task HandleAsync(CreateCharResponseDto data, CancellationToken cancellationToken = default)
        {
            _server.HandleCreateCharacterResponse(data);
            return Task.CompletedTask;
        }

        protected override CreateCharResponseDto Parse(ByteString content) => CreateCharResponseDto.Parser.ParseFrom(content);
    }
}
