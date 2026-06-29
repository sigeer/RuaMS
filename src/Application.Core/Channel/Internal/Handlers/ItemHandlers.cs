using Application.Core.Channel.Commands;
using Application.Shared.Message;
using AutoMapper;
using client.inventory;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using ItemProto;
using tools;

namespace Application.Core.Channel.Internal.Handlers
{
    internal class ItemHandlers
    {
        public class Megaphone : InternalSessionChannelHandler<UseItemMegaphoneBroadcast>
        {
            readonly IMapper _mapper;
            public Megaphone(WorldChannelServer server, IMapper mapper) : base(server)
            {
                _mapper = mapper;
            }

            public override int MessageId => (int)ChannelRecvCode.HandleItemMegaphone;

            protected override Task HandleMessage(UseItemMegaphoneBroadcast res)
            {
                return _server.BroadcastAsync(async w =>
                {
                    var p = PacketCreator.itemMegaphone(res.Request.Message, res.Request.IsWishper, res.MasterChannel, _mapper.Map<Item>(res.Request.Item));
                    await w.broadcastPacket(p);
                });
            }

            protected override UseItemMegaphoneBroadcast Parse(ByteString data) => UseItemMegaphoneBroadcast.Parser.ParseFrom(data);
        }

        public class TVMessageStart : InternalSessionChannelHandler<CreateTVMessageBroadcast>
        {
            public TVMessageStart(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.HandleTVMessageStart;

            protected override Task HandleMessage(CreateTVMessageBroadcast res)
            {
                return _server.PushChannelCommandAsync(new InvokeTVCommand(res));
            }

            protected override CreateTVMessageBroadcast Parse(ByteString data) => CreateTVMessageBroadcast.Parser.ParseFrom(data);
        }

        public class TVMessageFinish : InternalSessionChannelEmptyHandler
        {
            public TVMessageFinish(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.HandleTVMessageFinish;

            protected override Task HandleMessage(Empty res)
            {
                return _server.PushChannelCommandAsync(new InvokeTVFinishCommand());
            }

        }
    }
}
