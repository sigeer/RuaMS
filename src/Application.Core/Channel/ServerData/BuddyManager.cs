using Application.Core.ServerTransports;
using Application.Resources.Messages;
using Application.Shared.Constants.Buddy;
using Microsoft.Extensions.Logging;
using tools;

namespace Application.Core.Channel.ServerData
{
    public class BuddyManager
    {
        readonly ILogger<BuddyManager> _logger;
        readonly IMapper _mapper;
        readonly IChannelServerTransport _transport;
        readonly WorldChannelServer _server;

        public BuddyManager(ILogger<BuddyManager> logger, IMapper mapper, IChannelServerTransport transport, WorldChannelServer server)
        {
            _logger = logger;
            _mapper = mapper;
            _transport = transport;
            _server = server;
        }


        public async Task AddBuddy(Player player, string addName, string addGroup)
        {
            if (player.BuddyList.isFull())
            {
                await player.Popup(nameof(ClientMessage.Buddy_Full));
                return;
            }

            var ble = player.BuddyList.GetByName(addName);
            if (ble != null && addGroup == ble.Group)
            {
                await player.Popup(nameof(ClientMessage.Buddy_Exsited), ble.Name);
                return;
            }

            await _transport.SendAddBuddyRequest(new BuddyProto.AddBuddyRequest { MasterId = player.Id, TargetName = addName, GroupName = addGroup });

        }

        internal async Task AnswerInvite(Player chr, int fromId)
        {
            if (chr.BuddyList.isFull())
            {
                await chr.dropMessage(1, "好友位已满");
                return;
            }

            if (chr.BuddyList.Contains(fromId))
            {
                await chr.dropMessage(1, "已经是你的好友了");
                return;
            }

            await _transport.SendAddBuddyRequest(new BuddyProto.AddBuddyByIdRequest { MasterId = chr.Id, TargetId = fromId });
        }

        public Task DeleteBuddy(Player chr, int targetId)
        {
            return _transport.SendDeleteBuddy(new BuddyProto.DeleteBuddyRequest { MasterId = chr.Id, Buddyid = targetId });
        }

        internal Task SendWhisper(Player chr, string targetName, string message)
        {
            return _transport.SendWhisper(new MessageProto.SendWhisperMessageRequest { FromId = chr.Id, TargetName = targetName, Text = message });
        }

        internal async Task GetLocation(Player chr, string name)
        {
            var sameChannelSearch = chr.Client.CurrentServer.Players.getCharacterByName(name);
            if (sameChannelSearch != null)
            {
                await chr.SendPacket(PacketCreator.GetSameChannelFindResult(sameChannelSearch, WhisperFlag.LOCATION));
                return;
            }

            await _transport.GetLocation(new BuddyProto.GetLocationRequest { MasterId = chr.Id, TargetName = name });

        }
    }
}
