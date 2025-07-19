using Application.Core.Channel;
using Application.Core.Channel.Services;
using Application.Core.Game.Items;
using Application.Core.Game.Life;
using Application.Core.Managers.Constants;
using Application.Core.ServerTransports;
using Application.Shared.Items;
using AutoMapper;
using client.creator;
using client.inventory;
using server;

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

        public int CreatePlayer(int type, int accountId, string name, int face, int hair, int skin, int top, int bottom, int shoes, int weapon, int gender)
        {
            return CharacterFactory.GetNoviceCreator(type, this).createCharacter(accountId, name, face, hair, skin, top, bottom, shoes, weapon, gender);
        }

        public int CreatePlayer(IChannelClient client, int type, string name, int face, int hair, int skin, int gender, int improveSp)
        {
            var checkResult = _tranport.CreatePlayerCheck(new Dto.CreateCharCheckRequest { AccountId = client.AccountId, Name = name }).Code;
            if (checkResult != CreateCharResult.Success)
                return checkResult;

            return CharacterFactory.GetVeteranCreator(type, this).createCharacter(client.AccountId, name, face, hair, skin, gender, improveSp);
        }

        public int SendNewPlayer(IPlayer player)
        {
            var dto = _characteService.DeserializeNew(player);
            return _tranport.SendNewPlayer(dto).Code;
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
