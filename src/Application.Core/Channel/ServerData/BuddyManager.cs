using Application.Core.Game.Relation;
using Application.Core.ServerTransports;
using Application.Shared.Invitations;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;
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

        public void BuddyChat(IPlayer chr, int[] recipientCharacterIds, string chattext)
        {
            var request = new Dto.BuddyChatRequest { FromId = chr.Id, Text = chattext };
            request.ToIds.AddRange(recipientCharacterIds);
            _transport.SendBuddyChat(request);
        }

        public void OnBuddyChatReceived(Dto.BuddyChatBroadcast data)
        {
            foreach (var item in data.ToIds)
            {
                var chr = _server.FindPlayerById(item);
                if (chr != null && !chr.isAwayFromWorld())
                {
                    if (chr.BuddyList.Contains(data.FromId))
                    {
                        chr.sendPacket(PacketCreator.multiChat(data.FromName, data.Text, 0));
                    }
                }
            }

        }

        public void SendNotify(IPlayer chr, bool isLogin)
        {
            var request = new Dto.NotifyBuddyWhenLoginoffRequest { MasterId = chr.Id, IsLogin = isLogin };
            request.BuddyId.AddRange(chr.BuddyList.getBuddyIds());
            _transport.SendBuddyNotify(request);
        }

        public void OnBuddyNotifyChannel(Dto.NotifyBuddyWhenLoginoffBroadcast data)
        {
            foreach (int buddy in data.BuddyId)
            {
                var chr = _server.FindPlayerById(buddy);
                if (chr != null && !chr.isAwayFromWorld())
                {
                    var ble = chr.BuddyList.Get(data.MasterId);
                    if (ble != null)
                    {
                        int mcChannel = -1;
                        if (data.IsLogin)
                        {
                            mcChannel = (byte)(data.Channel - 1);
                        }
                        chr.BuddyList.Update(ble);
                        chr.sendPacket(PacketCreator.updateBuddyChannel(ble.Id, mcChannel));
                    }
                }
            }
        }

        public void SendBuddyNoticeMessage(IPlayer chr, int type, string message)
        {
            var request = new Dto.SendBuddyNoticeMessageDto { MasterId = chr.Id, Message = message, Type = type };
            request.BuddyId.AddRange(chr.BuddyList.getBuddyIds());
            _transport.SendBuddyMessage(request);
        }

        public void OnBuddyNoticeMessageReceived(Dto.SendBuddyNoticeMessageDto data)
        {
            foreach (int buddy in data.BuddyId)
            {
                var chr = _server.FindPlayerById(buddy);
                if (chr != null && !chr.isAwayFromWorld())
                {
                    var ble = chr.BuddyList.Get(data.MasterId);
                    if (ble != null)
                    {
                        chr.dropMessage(data.Type, data.Message);
                    }
                }
            }
        }

        public void AddBuddy(IPlayer player, string addName, string addGroup)
        {
            if (player.BuddyList.isFull())
            {
                player.sendPacket(PacketCreator.serverNotice(1, "你的好友位已满"));
                return;
            }

            var ble = player.BuddyList.GetByName(addName);
            if (ble != null)
            {
                if (addGroup != ble.Group)
                {
                    ble.Group = addGroup;
                    player.sendPacket(PacketCreator.updateBuddylist(player.BuddyList.getBuddies()));
                    return;
                }

                player.sendPacket(PacketCreator.serverNotice(1, "You already have \"" + ble.Name + "\" on your Buddylist"));
                return;
            }

            try
            {
                var res = _transport.SendAddBuddyRequest(new Dto.AddBuddyRequest { MasterId = player.Id, TargetName = addName });
                if (res.Code == 1)
                {
                    player.sendPacket(PacketCreator.serverNotice(1, $"玩家 {addName} 未找到"));
                    return;
                }

                if (res.Code == 0)
                {
                    player.BuddyList.Add(_mapper.Map<BuddyCharacter>(res.Buddy));
                    player.sendPacket(PacketCreator.updateBuddylist(player.BuddyList.getBuddies()));
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
            }
        }

        public void OnAddBuddyBroadcast(Dto.AddBuddyBroadcast data)
        {
            var chr = _server.FindPlayerById(data.ReceiverId);
            if (chr != null)
            {
                if (chr.BuddyList.isFull())
                {
                    chr.dropMessage(1, "好友位已满");
                    return;
                }

                if (chr.BuddyList.Contains(data.FromId))
                {
                    chr.dropMessage(1, "已经是你的好友了");
                    return;
                }

                chr.BuddyList.Add(_mapper.Map<BuddyCharacter>(data.Buddy));
                chr.sendPacket(PacketCreator.updateBuddylist(chr.BuddyList.getBuddies()));
            }
        }

        internal void AnswerInvite(IPlayer chr, int fromId)
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

            _server.Transport.AnswerInvitation(new Dto.AnswerInviteRequest { MasterId = chr.Id, Ok = true, Type = InviteTypes.Buddy, CheckKey = fromId });
        }
    }
}
