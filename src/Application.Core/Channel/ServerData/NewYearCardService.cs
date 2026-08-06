using Application.Core.ServerTransports;

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

        public void SendNewYearCard(Player from, string toName, string message)
        {
            _ = _transport.SendNewYearCard(new ProtoService.SendNewYearCardRequest { FromId = from.Id, ToName = toName, Message = message });
        }
        public void AcceptNewYearCard(Player receiver, int cardId)
        {
            _ = _transport.ReceiveNewYearCard(new ProtoService.ReceiveNewYearCardRequest { MasterId = receiver.Id, CardId = cardId });
        }

        public void DiscardNewYearCard(Player chr, bool isSender)
        {
            _ = _transport.SendDiscardNewYearCard(new ProtoService.DiscardNewYearCardRequest { MasterId = chr.Id, IsSender = isSender });
        }

    }
}
