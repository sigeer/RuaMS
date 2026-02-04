using Application.Core.Game.Players;
using Application.Core.Game.Relation;
using Application.Core.ServerTransports;
using Application.Resources.Messages;
using Application.Shared.Constants.Buddy;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System.Numerics;
using System.Threading.Tasks;
using System.Xml.Linq;
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


        public void AddBuddy(Player player, string addName, string addGroup)
        {
            if (player.BuddyList.isFull())
            {
                player.Popup(nameof(ClientMessage.Buddy_Full));
                return;
            }

            var ble = player.BuddyList.GetByName(addName);
            if (ble != null && addGroup == ble.Group)
            {
                player.Popup(nameof(ClientMessage.Buddy_Exsited), ble.Name);
                return;
            }

            _ = _transport.SendAddBuddyRequest(new BuddyProto.AddBuddyRequest { MasterId = player.Id, TargetName = addName, GroupName = addGroup });

        }

        internal void AnswerInvite(Player chr, int fromId)
        {
            if (chr.BuddyList.isFull())
            {
                chr.dropMessage(1, "好友位已满");
                return;
            }

            if (chr.BuddyList.Contains(fromId))
            {
                chr.dropMessage(1, "已经是你的好友了");
                return;
            }

            _ = _transport.SendAddBuddyRequest(new BuddyProto.AddBuddyByIdRequest { MasterId = chr.Id, TargetId = fromId });
        }

        public void DeleteBuddy(Player chr, int targetId)
        {
            _ = _transport.SendDeleteBuddy(new BuddyProto.DeleteBuddyRequest { MasterId = chr.Id, Buddyid = targetId });
        }

        internal void SendWhisper(Player chr, string targetName, string message)
        {
            _ = _transport.SendWhisper(new MessageProto.SendWhisperMessageRequest { FromId = chr.Id, TargetName = targetName, Text = message });
        }

        internal void GetLocation(Player chr, string name)
        {
            var sameChannelSearch = chr.Client.CurrentServer.Players.getCharacterByName(name);
            if (sameChannelSearch != null)
            {
                chr.sendPacket(PacketCreator.GetSameChannelFindResult(sameChannelSearch, WhisperFlag.LOCATION));
                return;
            }

            _ = _transport.GetLocation(new BuddyProto.GetLocationRequest { MasterId = chr.Id, TargetName = name });

        }
    }
}
