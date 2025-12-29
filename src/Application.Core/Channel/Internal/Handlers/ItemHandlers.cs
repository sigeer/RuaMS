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
        public class Megaphone : InternalSessionChannelHandler<UseItemMegaphoneResponse>
        {
            readonly IMapper _mapper;
            public Megaphone(WorldChannelServer server, IMapper mapper) : base(server)
            {
                _mapper = mapper;
            }

            public override int MessageId => ChannelRecvCode.HandleItemMegaphone;

            protected override Task HandleAsync(UseItemMegaphoneResponse res, CancellationToken cancellationToken = default)
            {
                var masterChr = _server.FindPlayerById(res.Request.MasterId);
                if (masterChr?.TscRequest != null)
                {
                    masterChr.TscRequest.Execute(masterChr, ct =>
                    {
                        return res.Code == 0;
                    });
                }

                if (res.Code == 0)
                {
                    _server.BroadcastPacket(PacketCreator.itemMegaphone(res.Request.Message, res.Request.IsWishper, res.MasterChannel, _mapper.Map<Item>(res.Request.Item)));
                }
                return Task.CompletedTask;
            }

            protected override UseItemMegaphoneResponse Parse(ByteString data) => UseItemMegaphoneResponse.Parser.ParseFrom(data);
        }

        public class TVMessageStart : InternalSessionChannelHandler<CreateTVMessageResponse>
        {
            public TVMessageStart(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => ChannelRecvCode.HandleTVMessageStart;

            protected override Task HandleAsync(CreateTVMessageResponse res, CancellationToken cancellationToken = default)
            {
                var masterChr = _server.FindPlayerById(res.Request.MasterId);
                if (masterChr?.TscRequest != null)
                {
                    masterChr.TscRequest.Execute(masterChr, ct =>
                    {
                        if (res.Code == 0)
                        {
                            return true;
                        }
                        masterChr.Popup("MapleTV is already in use.");
                        return false;
                    });
                }

                if (res.Code == 0)
                {
                    var noticeMsg = string.Join(" ", res.Request.MessageList);
                    _server.BroadcastPacket(PacketCreator.enableTV());
                    _server.BroadcastPacket(PacketCreator.sendTV(res.Master, res.Request.MessageList.ToArray(), res.Request.Type <= 2 ? res.Request.Type : res.Request.Type - 3, res.MasterPartner));

                    if (res.Request.Type >= 3)
                        _server.BroadcastPacket(PacketCreator.serverNotice(3, res.Master.Channel, CharacterViewDtoUtils.GetPlayerNameWithMedal(res.Master) + " : " + noticeMsg, res.Request.ShowEar));
                }


                return Task.CompletedTask;
            }

            protected override CreateTVMessageResponse Parse(ByteString data) => CreateTVMessageResponse.Parser.ParseFrom(data);
        }

        public class TVMessageFinish : InternalSessionEmptyHandler<WorldChannelServer>
        {
            public TVMessageFinish(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => ChannelRecvCode.HandleTVMessageFinish;

            protected override Task HandleAsync(Empty res, CancellationToken cancellationToken = default)
            {
                _server.ItemService.OnBroadcastTVFinished(res);
                return Task.CompletedTask;
            }

        }
    }
}
