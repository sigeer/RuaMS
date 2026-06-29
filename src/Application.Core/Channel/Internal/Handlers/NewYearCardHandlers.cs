using Application.Core.Models;
using Application.Shared.Internal;
using Application.Shared.Message;
using AutoMapper;
using client.inventory;
using Dto;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using System.Reflection;
using tools;
using static Application.Core.Channel.Internal.Handlers.NoteHandlers;

namespace Application.Core.Channel.Internal.Handlers
{
    internal class NewYearCardHandlers
    {
        public class ReceiveCard : InternalSessionChannelHandler<ReceiveNewYearCardResponse>
        {
            public ReceiveCard(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnNewYearCardReceived;

            protected override Task HandleMessage(ReceiveNewYearCardResponse res)
            {
                if (res.Code == 0)
                {
                    var newCard = _server.Mapper.Map<NewYearCardObject>(res.Model);

                    return _server.SendToPlayersAsync([res.Request.MasterId, res.Model.SenderId], async chr =>
                    {
                        if (chr.Id == res.Request.MasterId)
                        {
                            await chr.GainItem(ItemId.NEW_YEARS_CARD_RECEIVED, 1, show: GainItemShow.ShowInChat);
                            if (!string.IsNullOrEmpty(newCard.Message))
                            {
                                await chr.dropMessage(6, "[New Year] " + newCard.SenderName + ": " + newCard.Message);
                            }
                            chr.addNewYearRecord(newCard);
                            await chr.SendPacket(PacketCreator.onNewYearCardRes(chr, newCard, 6, 0));    // successfully rcvd

                            await chr.getMap().broadcastMessage(PacketCreator.onNewYearCardRes(chr, newCard, 0xD, 0));
                        }
                        else if (chr.Id == res.Model.SenderId)
                        {
                            await chr.getMap().broadcastMessage(PacketCreator.onNewYearCardRes(chr, newCard, 0xD, 0));
                            await chr.LightBlue("[New Year] Your addressee successfully received the New Year card.");
                        }
                    });
                }
                else
                {
                    return _server.SendToPlayerAsync(res.Request.MasterId, chr =>
                    {
                        return chr.LightBlue("[New Year] The sender of the New Year card already dropped it. Nothing to receive.");
                    });
                }
            }

            protected override ReceiveNewYearCardResponse Parse(ByteString data) => ReceiveNewYearCardResponse.Parser.ParseFrom(data);
        }

        public class SendCard : InternalSessionChannelHandler<SendNewYearCardResponse>
        {
            public SendCard(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnNewYearCardSent;

            protected override Task HandleMessage(SendNewYearCardResponse data)
            {
                return _server.SendToPlayerAsync(data.Request.FromId, async chr =>
                {
                    if (data.Code == 0)
                    {
                        await chr.GainItem(ItemId.NEW_YEARS_CARD, -1, show: GainItemShow.ShowInChat);
                        await chr.GainItem(ItemId.NEW_YEARS_CARD_SEND, 1, show: GainItemShow.ShowInChat);

                        var model = _server.Mapper.Map<NewYearCardObject>(data.Model);
                        chr.addNewYearRecord(model);
                        await chr.SendPacket(PacketCreator.onNewYearCardRes(chr, model, 4, 0));    // successfully sent
                    }
                    else
                    {
                        await chr.SendPacket(PacketCreator.onNewYearCardRes(chr, null, 5, data.Code));
                    }
                });
            }

            protected override SendNewYearCardResponse Parse(ByteString data) => SendNewYearCardResponse.Parser.ParseFrom(data);
        }

        public class Notify : InternalSessionChannelHandler<NewYearCardNotifyDto>
        {
            readonly IMapper _mapper;
            public Notify(WorldChannelServer server, IMapper mapper) : base(server)
            {
                _mapper = mapper;
            }

            public override int MessageId => (int)ChannelRecvCode.OnNewYearCardNotify;

            protected override async Task HandleMessage(NewYearCardNotifyDto res)
            {
                var allMembers = res.List.Select(x => x.MasterId);
                await _server.SendToPlayersAsync(allMembers, async chr =>
                {
                    var item = res.List.FirstOrDefault(x => x.MasterId == chr.Id)!;
                    foreach (var obj in item.List)
                    {
                        await chr.SendPacket(PacketCreator.onNewYearCardRes(chr, _mapper.Map<NewYearCardObject>(obj), 0xC, 0));
                    }
                });
            }

            protected override NewYearCardNotifyDto Parse(ByteString data) => NewYearCardNotifyDto.Parser.ParseFrom(data);
        }

        public class Discard : InternalSessionHandler<WorldChannelServer, DiscardNewYearCardResponse>
        {
            public Discard(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnNewYearCardDiscard;

            protected override Task HandleMessage(DiscardNewYearCardResponse res)
            {
                _server.Broadcast(w =>
                {
                    var cardList = _server.Mapper.Map<NewYearCardObject[]>(res.UpdateList);
                    w.getPlayerStorage().GetCharacterActor(res.Request.MasterId)?.Send(async m =>
                    {
                        var chr = m.getCharacterById(res.Request.MasterId);
                        foreach (var item in cardList)
                        {
                            if (chr != null)
                            {
                                chr.RemoveNewYearRecord(item.Id);
                                await chr.getMap().broadcastMessage(PacketCreator.onNewYearCardRes(chr, item, 0xE, 0));
                            }

                            var otherId = res.Request.IsSender ? item.ReceiverId : item.SenderId;
                            w.getPlayerStorage().GetCharacterActor(otherId)?.Send(async m =>
                            {
                                var other = m.getCharacterById(otherId);
                                if (other != null)
                                {
                                    other.RemoveNewYearRecord(item.Id);
                                    await other.getMap().broadcastMessage(PacketCreator.onNewYearCardRes(other, item, 0xE, 0));

                                    await other.dropMessage(6, "[New Year] " + (res.Request.IsSender ? item.SenderName : item.ReceiverName) + " threw away the New Year card.");
                                }
                            });

                        }
                    });
                });
                return Task.CompletedTask;
            }

            protected override DiscardNewYearCardResponse Parse(ByteString data) => DiscardNewYearCardResponse.Parser.ParseFrom(data);
        }
    }
}
