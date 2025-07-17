using Application.Core.Login.Models;
using Application.Core.Login.ServerTransports;
using Application.Shared.Message;
using Application.Shared.Servers;
using BaseProto;
using CashProto;
using Dto;
using ItemProto;
using net.server;
using tools;

namespace Application.Core.Login
{
    public class MasterServerTransport : MasterServerTransportBase, IServerTransport
    {
        public MasterServerTransport(MasterServer masterServer) : base(masterServer)
        {
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


        public void SendNotes(int channel, int id, Dto.NoteDto[] notes)
        {
            var data = new SendNoteResponse() { ReceiverChannel = channel, ReceiverId = id };
            data.List.AddRange(notes);
            SendMessage(BroadcastType.OnNoteSend, data, new PlayerChannelPair(channel, id));
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

        public void SendWorldConfig(Config.WorldConfig patch)
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastMessage(BroadcastType.OnWorldConfigUpdate, patch);
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

        /// <summary>
        ///  把<paramref name="chrId"/>传送到<paramref name="toChrId"/>
        /// </summary>
        /// <param name="chrId"></param>
        /// <param name="toChrId"></param>
        public void SummonPlayer(int chrId, int toChrId)
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
            SendMessage(BroadcastType.OnInvitationSend, response, response.SenderPlayerId, response.ReceivePlayerId);
        }

        internal void ReturnInvitationAnswer(AnswerInviteResponse response)
        {
            SendMessage(BroadcastType.OnInvitationAnswer, response, response.SenderPlayerId, response.ReceivePlayerId);
        }

        internal void BroadcastShutdown()
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastMessage(BroadcastType.OnShutdown, new Google.Protobuf.WellKnownTypes.Empty());
            }
        }

        internal void SendNewYearCardReceived(ReceiveNewYearCardResponse response)
        {
            int[] to = response.Model == null ? [response.Request.MasterId] : [response.Request.MasterId, response.Model.SenderId];
            SendMessage(BroadcastType.OnNewYearCardReceived, response, to);
        }

        internal void SendNewYearCards(SendNewYearCardResponse response)
        {
            SendMessage(BroadcastType.OnNewYearCardSend, response, response.Request.FromId);
        }

        internal void SendNewYearCardNotify(NewYearCardNotifyDto response)
        {
            SendMessage(BroadcastType.OnNewYearCardNotify, response, response.List.Select(x => x.MasterId).ToArray());
        }

        internal void SendNewYearCardDiscard(DiscardNewYearCardResponse response)
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastMessage(BroadcastType.OnNewYearCardDiscard, response);
            }
        }

        internal void SendSetFly(SetFlyResponse setFlyResponse)
        {
            SendMessage(BroadcastType.OnSetFly, setFlyResponse, setFlyResponse.Request.CId);
        }

        internal void BroadcastPLifeCreated(CreatePLifeRequest request)
        {
            BroadcastMessage(BroadcastType.OnPLifeCreated, request);
        }

        internal void BroadcastPLifeRemoved(RemovePLifeResponse request)
        {
            BroadcastMessage(BroadcastType.OnPLifeRemoved, request);
        }

        internal void ReturnBuyCashItem(BuyCashItemResponse buyCashItemResponse)
        {
            SendMessage(BroadcastType.OnCashItemPurchased, buyCashItemResponse, buyCashItemResponse.MasterId);
        }
    }
}
