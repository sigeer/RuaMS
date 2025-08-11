using Application.Core.Channel.Services;
using Application.Core.Game.Life;
using Application.Core.ServerTransports;
using AutoMapper;

namespace Application.Core.Channel
{
    /// <summary>
    /// 暂时放在Application.Core，之后会把Application.Core改成Application.Core.Channel
    /// </summary>
    public class ChannelService
    {
        readonly IChannelServerTransport _tranport;
        readonly DataService _characteService;
        readonly WorldChannel _server;
        readonly IMapper _mapper;

        public ChannelService(IChannelServerTransport tranport, DataService characteService, WorldChannel server, IMapper mapper)
        {
            _tranport = tranport;
            _characteService = characteService;
            _server = server;
            _mapper = mapper;
        }

        public void RemovePlayerIncomingInvites(int id)
        {
            _tranport.SendRemovePlayerIncomingInvites(id);
        }


        public Dictionary<int, List<DropEntry>> RequestAllReactorDrops()
        {
            var allItems = _tranport.RequestAllReactorDrops();
            return allItems.Items.GroupBy(x => x.DropperId).ToDictionary(x => x.Key, x => _mapper.Map<List<DropEntry>>(x.ToArray()));
        }


        internal int[] GetCardTierSize()
        {
            return _tranport.GetCardTierSize();
        }

        internal List<List<int>> GetMostSellerCashItems()
        {
            return _mapper.Map<List<List<int>>>(_tranport.GetMostSellerCashItems());
        }

        internal ItemProto.OwlSearchRecordDto[] GetOwlSearchedItems()
        {
            return _tranport.GetOwlSearchedItems().Items.ToArray();
        }

        internal void SendTeamChat(string name, string chattext)
        {
            _tranport.SendTeamChat(name, chattext);
        }
    }
}
