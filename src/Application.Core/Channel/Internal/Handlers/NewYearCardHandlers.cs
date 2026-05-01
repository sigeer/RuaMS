using Application.Core.Models;
using Application.Shared.Internal;
using Application.Shared.Message;
using AutoMapper;
using Dto;
using Google.Protobuf;
using tools;

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

            protected override void HandleMessage(ReceiveNewYearCardResponse res)
            {
                _server.Broadcast(w =>
                {
                    if (res.Code == 0)
                    {
                        var newCard = w.Mapper.Map<NewYearCardObject>(res.Model);

                        w.getPlayerStorage().GetCharacterActor(res.Request.MasterId)?.Send(m =>
                        {
                            var receiver = m.getCharacterById(res.Request.MasterId);

                            if (receiver != null)
                            {
                                receiver.GainItem(ItemId.NEW_YEARS_CARD_RECEIVED, 1, show: GainItemShow.ShowInChat);
                                if (!string.IsNullOrEmpty(newCard.Message))
                                {
                                    receiver.dropMessage(6, "[New Year] " + newCard.SenderName + ": " + newCard.Message);
                                }
                                receiver.addNewYearRecord(newCard);
                                receiver.sendPacket(PacketCreator.onNewYearCardRes(receiver, newCard, 6, 0));    // successfully rcvd

                                receiver.getMap().broadcastMessage(PacketCreator.onNewYearCardRes(receiver, newCard, 0xD, 0));
                            }
                        });



                        w.getPlayerStorage().GetCharacterActor(res.Model.SenderId)?.Send(m =>
                        {
                            var sender = m.getCharacterById(res.Model.SenderId);

                            if (sender != null)
                            {
                                sender.getMap().broadcastMessage(PacketCreator.onNewYearCardRes(sender, newCard, 0xD, 0));
                                sender.dropMessage(6, "[New Year] Your addressee successfully received the New Year card.");
                            }
                        });

                    }

                    else
                    {
                        var receiver = w.getPlayerStorage().GetCharacterClientById(res.Request.MasterId);
                        if (receiver != null)
                        {
                            receiver.LightBlue("[New Year] The sender of the New Year card already dropped it. Nothing to receive.");
                        }
                    }
                });
            }

            protected override ReceiveNewYearCardResponse Parse(ByteString data) => ReceiveNewYearCardResponse.Parser.ParseFrom(data);
        }

        public class SendCard : InternalSessionChannelHandler<SendNewYearCardResponse>
        {
            public SendCard(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnNewYearCardSent;

            protected override void HandleMessage(SendNewYearCardResponse data)
            {
                _server.Broadcast(w =>
                {
                    w.getPlayerStorage().GetCharacterActor(data.Request.FromId)?.Send(m =>
                    {
                        var sender = m.getCharacterById(data.Request.FromId);
                        if (sender != null)
                        {
                            if (data.Code == 0)
                            {
                                sender.GainItem(ItemId.NEW_YEARS_CARD, -1, show: GainItemShow.ShowInChat);
                                sender.GainItem(ItemId.NEW_YEARS_CARD_SEND, 1, show: GainItemShow.ShowInChat);

                                var model = w.Mapper.Map<NewYearCardObject>(data.Model);
                                sender.addNewYearRecord(model);
                                sender.sendPacket(PacketCreator.onNewYearCardRes(sender, model, 4, 0));    // successfully sent
                            }
                            else
                            {
                                sender.sendPacket(PacketCreator.onNewYearCardRes(sender, null, 5, data.Code));
                            }

                        }
                    });
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

            protected override void HandleMessage(NewYearCardNotifyDto res)
            {
                _server.Broadcast(w =>
                {
                    foreach (var item in res.List)
                    {
                        w.getPlayerStorage().GetCharacterActor(item.MasterId)?.Send(m =>
                        {
                            var chr = m.getCharacterById(item.MasterId);
                            if (chr != null)
                            {
                                foreach (var obj in item.List)
                                {
                                    chr.sendPacket(PacketCreator.onNewYearCardRes(chr, _mapper.Map<NewYearCardObject>(obj), 0xC, 0));
                                }
                            }
                        });
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

            protected override void HandleMessage(DiscardNewYearCardResponse res)
            {
                _server.Broadcast(w =>
                {
                    var cardList = w.Mapper.Map<NewYearCardObject[]>(res.UpdateList);
                    w.getPlayerStorage().GetCharacterActor(res.Request.MasterId)?.Send(m =>
                    {
                        var chr = m.getCharacterById(res.Request.MasterId);
                        foreach (var item in cardList)
                        {
                            if (chr != null)
                            {
                                chr.RemoveNewYearRecord(item.Id);
                                chr.getMap().broadcastMessage(PacketCreator.onNewYearCardRes(chr, item, 0xE, 0));
                            }

                            var otherId = res.Request.IsSender ? item.ReceiverId : item.SenderId;
                            w.getPlayerStorage().GetCharacterActor(otherId)?.Send(m =>
                            {
                                var other = m.getCharacterById(otherId);
                                if (other != null)
                                {
                                    other.RemoveNewYearRecord(item.Id);
                                    other.getMap().broadcastMessage(PacketCreator.onNewYearCardRes(other, item, 0xE, 0));

                                    other.dropMessage(6, "[New Year] " + (res.Request.IsSender ? item.SenderName : item.ReceiverName) + " threw away the New Year card.");
                                }
                            });

                        }
                    });
                });
            }

            protected override DiscardNewYearCardResponse Parse(ByteString data) => DiscardNewYearCardResponse.Parser.ParseFrom(data);
        }
    }
}
