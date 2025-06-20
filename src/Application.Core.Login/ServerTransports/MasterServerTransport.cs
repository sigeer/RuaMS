using Application.Core.Game.Relation;
using Application.Core.Login.Models;
using Application.EF.Entities;
using Application.Shared.Configs;
using Application.Shared.Constants.Item;
using Application.Shared.Constants.Job;
using Application.Shared.Message;
using Application.Shared.Servers;
using Application.Shared.Team;
using client.inventory;
using Dto;
using net.server;
using Org.BouncyCastle.Asn1.X509;
using System.Xml.Linq;
using tools;

namespace Application.Core.Login
{
    public class MasterServerTransport : IServerTransport
    {
        readonly MasterServer _server;
        public MasterServerTransport(MasterServer masterServer)
        {
            this._server = masterServer;
        }

        public void BroadcastMessage(Packet p)
        {
            var world = Server.getInstance().getWorld(0);
            foreach (var ch in world.Channels)
            {
                ch.broadcastPacket(p);
            }
        }

        public void BroadcastWorldGMPacket(Packet packet)
        {
            var world = Server.getInstance().getWorld(0);
            foreach (var ch in world.Channels)
            {
                ch.broadcastGMPacket(packet);
            }
        }

        public Dto.CreateCharResponseDto CreatePlayer(Dto.CreateCharRequestDto request)
        {
            var world = Server.getInstance().getWorld(0);
            var channel = world.Channels[0];
            var statusCode = channel.Service.CreatePlayer(
                request.Type, request.AccountId,
                request.Name, request.Face, request.Hair, request.SkinColor, request.Top, request.Bottom, request.Shoes, request.Weapon, request.Gender);
            return new Dto.CreateCharResponseDto() { Code = statusCode };
        }

        public CoupleIdPair? GetAllWeddingCoupleForGuest(int guestId, bool cathedral)
        {
            var world = Server.getInstance().getWorld(0);
            foreach (var ch in world.Channels)
            {
                var p = ch.WeddingInstance.GetWeddingCoupleForGuest(guestId, cathedral);
                if (p != null)
                {
                    return p;
                }
            }
            return null;
        }

        public int GetAllWeddingReservationStatus(IEnumerable<int> possibleWeddings, bool cathedral)
        {
            var world = Server.getInstance().getWorld(0);
            int selectedPw = -1;
            int selectedPos = int.MaxValue;

            foreach (int pw in possibleWeddings)
            {
                foreach (var ch in world.Channels)
                {
                    int pos = ch.WeddingInstance.GetWeddingReservationStatus(pw, cathedral);
                    if (pos != -1)
                    {
                        if (pos < selectedPos)
                        {
                            selectedPos = pos;
                            selectedPw = pw;
                            break;
                        }
                    }
                }
            }

            return selectedPw;
        }

        public void SendChannelPlayerPacket(int channel, int playerId, Packet packet)
        {
            var world = Server.getInstance().getWorld(0);
            var chr = world.Channels[channel - 1].getPlayerStorage().getCharacterById(playerId);
            if (chr != null)
            {
                chr.Client.sendPacket(packet);
            }
        }

        public void SendDueyNotification(int channel, int id, string senderName, bool dueyType)
        {
            var world = Server.getInstance().getWorld(0);
            var chr = world.Channels[channel - 1].getPlayerStorage().getCharacterById(id);
            if (chr != null)
            {
                chr.Client.sendPacket(PacketCreator.sendDueyParcelReceived(senderName, dueyType));
            }
        }

        public void SendNotes(int channel, int id, Dto.NoteDto[] notes)
        {
            var world = Server.getInstance().getWorld(0);
            world.Channels[channel - 1].Service.SendNoteMessage(id, notes);
        }

        public void SendServerMessage(IEnumerable<int> playerIdList)
        {
            if (playerIdList.Count() > 0)
            {
                var world = Server.getInstance().getWorld(0);
                foreach (int chrid in playerIdList)
                {
                    var chr = world.Players.getCharacterById(chrid);

                    if (chr != null && chr.isLoggedinWorld())
                    {
                        chr.sendPacket(PacketCommon.serverMessage(_server.ServerMessage));
                    }
                }
            }
        }


        public void SendMultiChat(int type, string nameFrom, PlayerChannelPair[] teamMember, string chatText)
        {
            if (teamMember.Length == 0)
                return;

            var groups = _server.GroupPlayer(teamMember);
            foreach (var server in groups)
            {
                var res = new MultiChatMessage { Type = type, FromName = nameFrom, Text = chatText };
                res.Receivers.AddRange(server.Value);
                server.Key.BroadcastMessage(BroadcastType.OnMultiChat, res);
            }
        }

        public void SendUpdateCouponRates(Config.CouponConfig config)
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastMessage(BroadcastType.OnCouponConfigUpdate, config);
            }
        }

        public void SendWorldConfig(WorldConfigPatch patch)
        {
            var world = Server.getInstance().getWorld(0);
            foreach (var ch in world.Channels)
            {
                ch.UpdateWorldConfig(patch);
            }
        }

        public bool WrapPlayer(string name, int? channel, int mapId, int? portal)
        {
            var world = Server.getInstance().getWorld(0);
            foreach (var ch in world.getChannels())
            {
                var target = ch.Players.getCharacterByName(name);
                if (target != null)
                {
                    if (portal != null)
                        target.changeMap(mapId, portal.Value);
                    else
                        target.changeMap(mapId);

                    if (channel != null && target.Channel != channel)
                    {
                        target.Client.ChangeChannel(channel.Value);
                    }
                    return true;
                }
            }
            return false;
        }

        public void FamilyReuinion(int chrId, int toChrId)
        {

        }


        internal void BroadcastTeamUpdate(UpdateTeamResponse response)
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastMessage(BroadcastType.OnTeamUpdate, response);
            }
        }

        internal void DropMessage(IEnumerable<PlayerChannelPair> targets, int type, string message)
        {
            if (targets.Count() == 0)
                return;

            var groups = _server.GroupPlayer(targets);
            foreach (var server in groups)
            {
                var msg = new DropMessageDto { Type = type, Message = message };
                msg.PlayerId.AddRange(server.Value);
                server.Key.BroadcastMessage(BroadcastType.OnDropMessage, msg);
            }
        }

        internal void BroadcastGuildGPUpdate(UpdateGuildGPResponse response)
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastMessage(BroadcastType.OnGuildGpUpdate, response);
            }
        }

        internal void BroadcastGuildRankTitleUpdate(UpdateGuildRankTitleResponse response)
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastMessage(BroadcastType.OnGuildRankTitleUpdate, response);
            }
        }

        internal void BroadcastGuildNoticeUpdate(UpdateGuildNoticeResponse response)
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastMessage(BroadcastType.OnGuildNoticeUpdate, response);
            }
        }

        internal void BroadcastGuildCapacityUpdate(UpdateGuildCapacityResponse response)
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastMessage(BroadcastType.OnGuildCapacityUpdate, response);
            }
        }

        internal void BroadcastGuildEmblemUpdate(UpdateGuildEmblemResponse response)
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastMessage(BroadcastType.OnGuildEmblemUpdate, response);
            }
        }

        internal void BroadcastGuildDisband(GuildDisbandResponse response)
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastMessage(BroadcastType.OnGuildDisband, response);
            }
        }

        internal void BroadcastGuildRankChanged(UpdateGuildMemberRankResponse response)
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastMessage(BroadcastType.OnGuildRankChanged, response);
            }
        }

        internal void BroadcastGuildExpelMember(ExpelFromGuildResponse response)
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastMessage(BroadcastType.OnGuildExpelMember, response);
            }
        }

        internal void BroadcastPlayerJoinGuild(JoinGuildResponse response)
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastMessage(BroadcastType.OnPlayerJoinGuild, response);
            }
        }

        internal void BroadcastPlayerLeaveGuild(LeaveGuildResponse response)
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastMessage(BroadcastType.OnPlayerLeaveGuild, response);
            }
        }

        internal void BroadcastPlayerLevelChanged(PlayerLevelJobChange response)
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastMessage(BroadcastType.OnPlayerLevelChanged, response);
            }
        }

        internal void BroadcastPlayerJobChanged(PlayerLevelJobChange response)
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastMessage(BroadcastType.OnPlayerJobChanged, response);
            }
        }

        internal void BroadcastPlayerLoginOff(PlayerOnlineChange response)
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastMessage(BroadcastType.OnPlayerLoginOff, response);
            }
        }

        internal void BroadcastGuildJoinAlliance(GuildJoinAllianceResponse response)
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastMessage(BroadcastType.OnGuildJoinAlliance, response);
            }
        }

        internal void BroadcastGuildLeaveAlliance(GuildLeaveAllianceResponse response)
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastMessage(BroadcastType.OnGuildLeaveAlliance, response);
            }
        }

        internal void BroadcastAllianceExpelGuild(AllianceExpelGuildResponse response)
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastMessage(BroadcastType.OnAllianceExpelGuild, response);
            }
        }

        internal void BroadcastAllianceCapacityIncreased(IncreaseAllianceCapacityResponse response)
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastMessage(BroadcastType.OnAllianceCapacityUpdate, response);
            }
        }

        internal void BroadcastAllianceRankTitleChanged(UpdateAllianceRankTitleResponse response)
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastMessage(BroadcastType.OnAllianceRankTitleUpdate, response);
            }
        }

        internal void BroadcastAllianceNoticeChanged(UpdateAllianceNoticeResponse response)
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastMessage(BroadcastType.OnAllianceNoticeUpdate, response);
            }
        }

        internal void BroadcastAllianceLeaderChanged(AllianceChangeLeaderResponse response)
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastMessage(BroadcastType.OnAllianceChangeLeader, response);
            }
        }

        internal void BroadcastAllianceMemberRankChanged(ChangePlayerAllianceRankResponse response)
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastMessage(BroadcastType.OnAllianceRankChange, response);
            }
        }

        internal void BroadcastAllianceDisband(DisbandAllianceResponse response)
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastMessage(BroadcastType.OnAllianceDisband, response);
            }
        }

        internal void BroadcastJoinChatRoom(Dto.JoinChatRoomResponse response)
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastMessage(BroadcastType.OnJoinChatRoom, response);
            }
        }

        internal void BroadcastLeaveChatRoom(Dto.LeaveChatRoomResponse response)
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastMessage(BroadcastType.OnLeaveChatRoom, response);
            }
        }

        internal void BroadcastChatRoomMessage(SendChatRoomMessageResponse res)
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastMessage(BroadcastType.OnChatRoomMessageSend, res);
            }
        }

        internal void ReturnInvitationCreated(CreateInviteResponse response)
        {
            var sender = _server.CharacterManager.FindPlayerById(response.SenderPlayerId)!;
            var server1 = _server.GetChannelServer(sender.Channel);
            server1.BroadcastMessage(BroadcastType.OnInvitationSend, response);

            var receiver = _server.CharacterManager.FindPlayerById(response.ReceivePlayerId);
            if (receiver != null)
            {
                var server2 = _server.GetChannelServer(receiver.Channel);
                if (server1 != server2)
                    server2.BroadcastMessage(BroadcastType.OnInvitationSend, response);
            }

        }

        internal void ReturnInvitationAnswer(AnswerInviteResponse response)
        {
            var sender = _server.CharacterManager.FindPlayerById(response.SenderPlayerId)!;
            var server1 = _server.GetChannelServer(sender.Channel);
            server1.BroadcastMessage(BroadcastType.OnInvitationAnswer, response);

            var receiver = _server.CharacterManager.FindPlayerById(response.ReceivePlayerId);
            if (receiver != null)
            {
                var server2 = _server.GetChannelServer(receiver.Channel);
                if (server1 != server2)
                    server2.BroadcastMessage(BroadcastType.OnInvitationAnswer, response);
            }
        }
    }
}
