using Application.Core.Game.Relation;
using Application.Core.Login.Models;
using Application.EF.Entities;
using Application.Shared.Configs;
using Application.Shared.Constants.Item;
using Application.Shared.Constants.Job;
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
            var groups = _server.GroupPlayer(teamMember);
            foreach (var server in groups)
            {
                server.Key.SendMultiChat(type, nameFrom, server.Value, chatText);
            }
        }

        public void SendUpdateCouponRates(Config.CouponConfig config)
        {
            foreach (var server in _server.ChannelServerList)
            {
                server.Value.UpdateCouponConfig(config);
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


        internal void BroadcastTeamUpdate(int teamId, PartyOperation operation, TeamMemberDto target)
        {
            foreach (var server in _server.ChannelServerList)
            {
                server.Value.SendTeamUpdate(teamId, operation, target);
            }
        }

        internal void DropMessage(IEnumerable<PlayerChannelPair> targets, int type, string message)
        {
            var groups = _server.GroupPlayer(targets);
            foreach (var server in groups)
            {
                server.Key.DropMessage(server.Value, type, message);
            }
        }

        internal void BroadcastGuildGPUpdate(UpdateGuildGPResponse response)
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastGuildGPUpdate(response);
            }
        }

        internal void BroadcastGuildRankTitleUpdate(UpdateGuildRankTitleResponse response)
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastGuildRankTitleUpdate(response);
            }
        }

        internal void BroadcastGuildNoticeUpdate(UpdateGuildNoticeResponse response)
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastGuildNoticeUpdate(response);
            }
        }

        internal void BroadcastGuildCapacityUpdate(UpdateGuildCapacityResponse response)
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastGuildCapacityUpdate(response);
            }
        }

        internal void BroadcastGuildEmblemUpdate(UpdateGuildEmblemResponse response)
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastGuildEmblemUpdate(response);
            }
        }

        internal void BroadcastGuildDisband(GuildDisbandResponse response)
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastGuildDisband(response);
            }
        }

        internal void BroadcastGuildRankChanged(UpdateGuildMemberRankResponse response)
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastGuildRankChanged(response);
            }
        }

        internal void BroadcastGuildExpelMember(ExpelFromGuildResponse response)
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastGuildExpelMember(response);
            }
        }

        internal void BroadcastPlayerJoinGuild(JoinGuildResponse response)
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastPlayerJoinGuild(response);
            }
        }

        internal void BroadcastPlayerLeaveGuild(LeaveGuildResponse response)
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastPlayerLeaveGuild(response);
            }
        }

        internal void BroadcastPlayerLevelChanged(PlayerLevelJobChange response)
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastPlayerLevelChanged(response);
            }
        }

        internal void BroadcastPlayerJobChanged(PlayerLevelJobChange response)
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastPlayerJobChanged(response);
            }
        }

        internal void BroadcastPlayerLoginOff(PlayerOnlineChange response)
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastPlayerLoginOff(response);
            }
        }

        internal void BroadcastGuildJoinAlliance(GuildJoinAllianceResponse response)
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastGuildJoinAlliance(response);
            }
        }

        internal void BroadcastGuildLeaveAlliance(GuildLeaveAllianceResponse response)
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastGuildLeaveAlliance(response);
            }
        }

        internal void BroadcastAllianceExpelGuild(AllianceExpelGuildResponse response)
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastAllianceExpelGuild(response);
            }
        }

        internal void BroadcastAllianceCapacityIncreased(IncreaseAllianceCapacityResponse response)
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastAllianceCapacityIncreased(response);
            }
        }

        internal void BroadcastAllianceRankTitleChanged(UpdateAllianceRankTitleResponse response)
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastAllianceRankTitleChanged(response);
            }
        }

        internal void BroadcastAllianceNoticeChanged(UpdateAllianceNoticeResponse response)
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastAllianceNoticeChanged(response);
            }
        }

        internal void BroadcastAllianceLeaderChanged(AllianceChangeLeaderResponse response)
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastAllianceLeaderChanged(response);
            }
        }

        internal void BroadcastAllianceMemberRankChanged(ChangePlayerAllianceRankResponse response)
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastAllianceMemberRankChanged(response);
            }
        }

        internal void BroadcastAllianceDisband(DisbandAllianceResponse response)
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastAllianceDisband(response);
            }
        }
    }
}
