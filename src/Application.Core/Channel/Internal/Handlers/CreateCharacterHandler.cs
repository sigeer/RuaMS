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

        public override int MessageId => (int)ChannelRecvCode.CreateCharacter;

        protected override void HandleMessage(CreateCharRequestDto data)
        {
            var res = _server.DataService.CreatePlayer(data);
            _ = _server.Transport.CreatePlayerResponseAsync(res);
        }

        protected override CreateCharRequestDto Parse(ByteString content) => CreateCharRequestDto.Parser.ParseFrom(content);
    }
}
