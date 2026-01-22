using Application.Core.Game.Players;
using Application.Shared.Internal;
using Application.Shared.Message;
using AutoMapper;
using client.inventory;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using ItemProto;
using Microsoft.AspNetCore.Hosting.Server;
using server.quest;
using SyncProto;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using tools;
using XmlWzReader;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

            protected override Task HandleAsync(UseItemMegaphoneBroadcast res, CancellationToken cancellationToken = default)
            {
                _server.BroadcastPacket(PacketCreator.itemMegaphone(res.Request.Message, res.Request.IsWishper, res.MasterChannel, _mapper.Map<Item>(res.Request.Item)));
                return Task.CompletedTask;
            }

            protected override UseItemMegaphoneBroadcast Parse(ByteString data) => UseItemMegaphoneBroadcast.Parser.ParseFrom(data);
        }

        public class TVMessageStart : InternalSessionChannelHandler<CreateTVMessageBroadcast>
        {
            public TVMessageStart(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.HandleTVMessageStart;

            protected override Task HandleAsync(CreateTVMessageBroadcast res, CancellationToken cancellationToken = default)
            {
                var noticeMsg = string.Join(" ", res.Request.MessageList);
                _server.BroadcastPacket(PacketCreator.enableTV());
                _server.BroadcastPacket(PacketCreator.sendTV(res.Master, res.Request.MessageList.ToArray(), res.Request.Type <= 2 ? res.Request.Type : res.Request.Type - 3, res.MasterPartner));

                if (res.Request.Type >= 3)
                    _server.BroadcastPacket(PacketCreator.serverNotice(3, res.Master.Channel, CharacterViewDtoUtils.GetPlayerNameWithMedal(res.Master) + " : " + noticeMsg, res.Request.ShowEar));


                return Task.CompletedTask;
            }

            protected override CreateTVMessageBroadcast Parse(ByteString data) => CreateTVMessageBroadcast.Parser.ParseFrom(data);
        }

        public class TVMessageFinish : InternalSessionChannelEmptyHandler
        {
            public TVMessageFinish(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.HandleTVMessageFinish;

            protected override Task HandleAsync(Empty res, CancellationToken cancellationToken = default)
            {
                _server.ItemService.OnBroadcastTVFinished(res);
                return Task.CompletedTask;
            }

        }
    }
}
