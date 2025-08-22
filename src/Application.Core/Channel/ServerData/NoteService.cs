using Application.Core.Models;
using net.packet.outs;

namespace Application.Core.Channel.ServerData
{
    public class NoteService
    {
        readonly IMapper _mapper;
        readonly WorldChannelServer _server;

        public NoteService(IMapper mapper, WorldChannelServer server)
        {
            _mapper = mapper;
            _server = server;
        }

        public void OnNoteReceived(Dto.SendNoteResponse data)
        {
            var chr = _server.FindPlayerById(data.ReceiverChannel, data.ReceiverId);
            if (chr != null)
            {
                chr.sendPacket(new ShowNotesPacket(_mapper.Map<List<NoteObject>>(data.List)));
            }
        }
    }
}
