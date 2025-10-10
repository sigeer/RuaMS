using Application.Core.Game.Relation;
using Application.Core.ServerTransports;
using Application.Resources.Messages;
using Application.Shared.Constants.Buddy;
using AutoMapper;
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
                        ble.Channel = data.Channel;
                        ble.ActualChannel = data.ActualChannel;
                        chr.BuddyList.Update(ble);
                        chr.sendPacket(PacketCreator.updateBuddyChannel(ble.Id, ble.ActualChannel - 1));
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
                player.Popup(nameof(ClientMessage.Buddy_Full));
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

                player.Popup(nameof(ClientMessage.Buddy_Exsited), ble.Name);
                return;
            }

            var res = _transport.SendAddBuddyRequest(new Dto.AddBuddyRequest { MasterId = player.Id, TargetName = addName, GroupName = addGroup });
            if (res.Code == 1)
            {
                player.Popup(nameof(ClientMessage.PlayerNotFound), addName);
                return;
            }

            if (res.Code == 0)
            {
                player.BuddyList.Add(_mapper.Map<BuddyCharacter>(res.Buddy));
                player.sendPacket(PacketCreator.updateBuddylist(player.BuddyList.getBuddies()));
            }
        }

        public void OnAddBuddyBroadcast(Dto.AddBuddyBroadcast data)
        {
            var chr = _server.FindPlayerById(data.ReceiverId);
            if (chr != null)
            {
                if (chr.BuddyList.Contains(data.Buddy.Id))
                {
                    chr.BuddyList.Update(_mapper.Map<BuddyCharacter>(data.Buddy));
                    chr.sendPacket(PacketCreator.updateBuddylist(chr.BuddyList.getBuddies()));
                }
                else
                {
                    chr.sendPacket(PacketCreator.requestBuddylistAdd(data.Buddy.Id, data.ReceiverId, data.Buddy.Name));
                }
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

            var res = _transport.SendAddBuddyRequest(new Dto.AddBuddyByIdRequest { MasterId = chr.Id, TargetId = fromId });
            if (res.Code == 0)
            {
                chr.BuddyList.Add(_mapper.Map<BuddyCharacter>(res.Buddy));
                chr.sendPacket(PacketCreator.updateBuddylist(chr.BuddyList.getBuddies()));
            }
            else
            {
                chr.dropMessage(1, "未知错误：" + res.Code);
            }
        }

        public void DeleteBuddy(IPlayer chr, int targetId)
        {
            var res = _transport.SendDeleteBuddy(new Dto.DeleteBuddyRequest { MasterId = chr.Id, Buddyid = targetId });
            if (res.Code == 0)
            {
                chr.BuddyList.Remove(targetId);
                chr.sendPacket(PacketCreator.updateBuddylist(chr.BuddyList.getBuddies()));
            }

        }

        public void OnBuddyDeleted(Dto.DeleteBuddyBroadcast data)
        {
            var chr = _server.FindPlayerById(data.MasterId);
            if (chr != null)
            {
                if (chr.BuddyList.Contains(data.Buddyid))
                {
                    chr.sendPacket(PacketCreator.updateBuddyChannel(data.Buddyid, -1));
                }
            }
        }

        internal void SendWhisper(IPlayer chr, string targetName, string message)
        {
            var res = _transport.SendWhisper(new Dto.SendWhisperMessageRequest { FromId = chr.Id, TargetName = targetName, Text = message });
            chr.sendPacket(PacketCreator.getWhisperResult(targetName, res.Code == 0));
        }

        internal void OnWhisperReceived(Dto.SendWhisperMessageBroadcast data)
        {
            var chr = _server.FindPlayerById(data.ReceiverId);
            if (chr != null)
            {
                chr.sendPacket(PacketCreator.getWhisperReceive(data.FromName, data.FromChannel - 1, data.IsFromGM, data.Text));
            }
        }

        internal void GetLocation(IPlayer chr, string name)
        {
            var sameChannelSearch = chr.Client.CurrentServer.Players.getCharacterByName(name);
            if (sameChannelSearch != null)
            {
                chr.sendPacket(PacketCreator.GetSameChannelFindResult(sameChannelSearch, WhisperFlag.LOCATION));
                return;
            }

            var res = _transport.GetLocation(new Dto.GetLocationRequest { MasterId = chr.Id, TargetName = name });
            var code = (WhisperLocationResponseCode)res.Code;
            switch (code)
            {
                case WhisperLocationResponseCode.NotFound:
                case WhisperLocationResponseCode.NotOnlined:
                case WhisperLocationResponseCode.NoAccess:
                    chr.sendPacket(PacketCreator.getWhisperResult(name, false));
                    break;
                case WhisperLocationResponseCode.AwayWorld:
                    chr.sendPacket(PacketCreator.GetFindResult(name, WhisperType.RT_CASH_SHOP, -1, WhisperFlag.LOCATION));
                    break;
                case WhisperLocationResponseCode.DiffChannel:
                    chr.sendPacket(PacketCreator.GetFindResult(name, WhisperType.RT_DIFFERENT_CHANNEL, res.Field, WhisperFlag.LOCATION));
                    break;
                default:
                    break;
            }
        }
    }
}
