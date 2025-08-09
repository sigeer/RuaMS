using Application.Core.Login.Models;
using Application.Core.Login.ServerTransports;
using Application.Shared.Message;
using Application.Shared.Servers;
using BaseProto;
using CashProto;
using Config;
using Dto;
using net.server;
using System.Resources;

namespace Application.Core.Login
{
    public class MasterServerTransport : MasterServerTransportBase, IServerTransport
    {
        public MasterServerTransport(MasterServer masterServer) : base(masterServer)
        {
        }

        public Dto.CreateCharResponseDto CreatePlayer(Dto.CreateCharRequestDto request)
        {
            var defaultServer = _server.ChannelServerList.FirstOrDefault().Value;
            if (defaultServer == null)
                return new CreateCharResponseDto { Code = -2 };

            return defaultServer.CreateCharacterFromChannel(request);
        }

        public ExpeditionProto.QueryChannelExpedtionResponse QueryExpeditionInfo(ExpeditionProto.QueryChannelExpedtionRequest request)
        {
            if (request.Channel <= 0)
            {
                var res = new ExpeditionProto.QueryChannelExpedtionResponse();
                List<ExpeditionProto.ChannelExpeditionDto> all = [];
                foreach (var item in _server.ChannelServerList)
                {
                    var data = item.Value.GetExpeditionInfo();
                    all.AddRange(data.List);
                }
                res.List.AddRange(all.OrderBy(x => x.Channel));
                return res;
            }
            else
            {
                return _server.ChannelServerList[_server.Channels[request.Channel].ServerName].GetExpeditionInfo();
            }
        }


        public void SendNotes(int channel, int id, Dto.NoteDto[] notes)
        {
            var data = new SendNoteResponse() { ReceiverChannel = channel, ReceiverId = id };
            data.List.AddRange(notes);
            SendMessage(BroadcastType.OnNoteSend, data, new PlayerChannelPair(channel, id));
            SendMessage(BroadcastType.OnNoteSend, data, data.ReceiverId);
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

        internal void BroadcastTeamUpdate(UpdateTeamResponse response)
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastMessage(BroadcastType.OnTeamUpdate, response);
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

        internal void BroadcastReportNotify(SendReportBroadcast data)
        {
            SendMessage(BroadcastType.OnReportReceived, data, data.GmId.ToArray());
        }

        internal void MonitorChangedNotify(MonitorDataChangedNotifyDto data)
        {
            SendMessage(BroadcastType.OnMonitorChangedNotify, data, data.GmId.ToArray());
        }

        internal void AutobanIgnoresChangedNotify(AutoBanIgnoredChangedNotifyDto data)
        {
            SendMessage(BroadcastType.OnAutoBanIgnoreChangedNotify, data, data.GmId.ToArray());
        }

        internal void BroadcastBanned(BanBroadcast data)
        {
            BroadcastMessage(BroadcastType.BroadcastBan, data);
        }

        internal void BroadcastGmLevelChanged(SetGmLevelBroadcast data)
        {
            SendMessage(BroadcastType.OnGmLevelSet, data, data.TargetId);
        }

        internal void SendWrapPlayerByName(SummonPlayerByNameBroadcast data)
        {
            SendMessage(BroadcastType.SendWrapPlayerByName, data, data.MasterId);
        }
    }
}
