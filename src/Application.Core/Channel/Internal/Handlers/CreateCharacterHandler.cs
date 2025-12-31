using Application.Shared.Internal;
using Application.Shared.Message;
using CreatorProto;
using Google.Protobuf;

namespace Application.Core.Channel.Internal.Handlers
{
    internal class CreateCharacterHandler : InternalSessionChannelHandler<CreateCharRequestDto>
    {
        public CreateCharacterHandler(WorldChannelServer server) : base(server)
        {
        }

        public override int MessageId => ChannelRecvCode.CreateCharacter;

        protected override async Task HandleAsync(CreateCharRequestDto data, CancellationToken cancellationToken = default)
        {
            var res = _server.DataService.CreatePlayer(data);
            await _server.Transport.CreatePlayerResponseAsync(res, cancellationToken);
        }

        protected override CreateCharRequestDto Parse(ByteString content) => CreateCharRequestDto.Parser.ParseFrom(content);
    }
}
