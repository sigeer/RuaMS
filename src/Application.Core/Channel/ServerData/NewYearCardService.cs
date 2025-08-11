using Application.Core.Models;
using Application.Core.ServerTransports;
using AutoMapper;
using Dto;
using tools;

namespace Application.Core.Channel.ServerData
{
    public class NewYearCardService
    {
        readonly IChannelServerTransport _transport;
        readonly WorldChannelServer _server;
        readonly IMapper _mapper;

        public NewYearCardService(IChannelServerTransport transport, WorldChannelServer server, IMapper mapper)
        {
            _transport = transport;
            _server = server;
            _mapper = mapper;
        }

        public void SendNewYearCard(IPlayer from, string toName, string message)
        {
            _transport.SendNewYearCard(new Dto.SendNewYearCardRequest { FromId = from.Id, ToName = toName, Message = message });
        }

        public void OnNewYearCardSend(Dto.SendNewYearCardResponse data)
        {
            if (data.Code == 0)
            {
                var sender = _server.FindPlayerById(data.Request.FromId);
                if (sender != null)
                {
                    sender.getAbstractPlayerInteraction().gainItem(ItemId.NEW_YEARS_CARD, -1);
                    sender.getAbstractPlayerInteraction().gainItem(ItemId.NEW_YEARS_CARD_SEND, 1);

                    var model = _mapper.Map<NewYearCardObject>(data.Model);
                    sender.addNewYearRecord(model);
                    sender.sendPacket(PacketCreator.onNewYearCardRes(sender, model, 4, 0));    // successfully sent
                }
            }
            else
            {
                var sender = _server.FindPlayerById(data.Request.FromId);
                if (sender != null)
                {
                    sender.sendPacket(PacketCreator.onNewYearCardRes(sender, null, 5, data.Code));
                }
            }
        }

        public void AcceptNewYearCard(IPlayer receiver, int cardId)
        {
            _transport.ReceiveNewYearCard(new Dto.ReceiveNewYearCardRequest { MasterId = receiver.Id, CardId = cardId });
        }

        public void OnNewYearCardReceived(Dto.ReceiveNewYearCardResponse data)
        {
            if (data.Code == 0)
            {
                var newCard = _mapper.Map<NewYearCardObject>(data.Model);

                var receiver = _server.FindPlayerById(data.Request.MasterId);
                if (receiver != null)
                {
                    receiver.getAbstractPlayerInteraction().gainItem(ItemId.NEW_YEARS_CARD_RECEIVED, 1);
                    if (!string.IsNullOrEmpty(newCard.Message))
                    {
                        receiver.dropMessage(6, "[New Year] " + newCard.SenderName + ": " + newCard.Message);
                    }
                    receiver.addNewYearRecord(newCard);
                    receiver.sendPacket(PacketCreator.onNewYearCardRes(receiver, newCard, 6, 0));    // successfully rcvd

                    receiver.getMap().broadcastMessage(PacketCreator.onNewYearCardRes(receiver, newCard, 0xD, 0));
                }


                var sender = _server.FindPlayerById(data.Model.SenderId);
                if (sender != null)
                {
                    sender.getMap().broadcastMessage(PacketCreator.onNewYearCardRes(sender, newCard, 0xD, 0));
                    sender.dropMessage(6, "[New Year] Your addressee successfully received the New Year card.");
                }
            }

            else
            {
                var receiver = _server.FindPlayerById(data.Request.MasterId);
                if (receiver != null)
                {
                    receiver.dropMessage(6, "[New Year] The sender of the New Year card already dropped it. Nothing to receive.");
                }
            }
        }

        internal void OnNewYearCardNotify(NewYearCardNotifyDto dto)
        {
            foreach (var item in dto.List)
            {
                var chr = _server.FindPlayerById(item.MasterId);
                if (chr != null)
                {
                    foreach (var obj in item.List)
                    {
                        chr.sendPacket(PacketCreator.onNewYearCardRes(chr, _mapper.Map<NewYearCardObject>(obj), 0xC, 0));
                    }
                }

            }

        }

        public void DiscardNewYearCard(IPlayer chr, bool isSender)
        {
            _transport.SendDiscardNewYearCard(new Dto.DiscardNewYearCardRequest { MasterId = chr.Id, IsSender = isSender });
        }

        public void OnNewYearCardDiscard(Dto.DiscardNewYearCardResponse data)
        {
            var cardList = _mapper.Map<NewYearCardObject[]>(data.UpdateList);
            var chr = _server.FindPlayerById(data.Request.MasterId);

            foreach (var item in cardList)
            {
                if (chr != null)
                {
                    chr.RemoveNewYearRecord(item.Id);
                    chr.getMap().broadcastMessage(PacketCreator.onNewYearCardRes(chr, item, 0xE, 0));
                }

                var other = _server.FindPlayerById(data.Request.IsSender ? item.ReceiverId : item.SenderId);
                if (other != null)
                {
                    other.RemoveNewYearRecord(item.Id);
                    other.getMap().broadcastMessage(PacketCreator.onNewYearCardRes(other, item, 0xE, 0));

                    other.dropMessage(6, "[New Year] " + (data.Request.IsSender ? item.SenderName : item.ReceiverName) + " threw away the New Year card.");
                }
            }
        }
    }
}
