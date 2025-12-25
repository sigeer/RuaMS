using Application.Core.Login.Models;
using Application.Core.Login.ServerTransports;
using Application.Shared.Message;
using Application.Shared.Servers;
using BaseProto;
using CashProto;
using Config;
using Dto;
using MessageProto;
using net.server;
using System.Resources;
using System.Threading.Tasks;

namespace Application.Core.Login
{
    public class MasterServerTransport : MasterServerTransportBase, IServerTransport
    {
        public MasterServerTransport(MasterServer masterServer) : base(masterServer)
        {
        }

        public CreatorProto.CreateCharResponseDto CreatePlayer(CreatorProto.CreateCharRequestDto request)
        {
            var defaultServer = _server.ChannelServerList.FirstOrDefault().Value;
            if (defaultServer == null)
                return new CreatorProto.CreateCharResponseDto { Code = -2 };

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
            SendMessage(BroadcastType.OnNoteSend, data, [data.ReceiverId]);
        }


        public async Task SendMultiChatAsync(int type, string nameFrom, IEnumerable<CharacterLiveObject> teamMember, string chatText)
        {
            var res = new MultiChatMessage { Type = type, FromName = nameFrom, Text = chatText };
            res.Receivers.AddRange(teamMember.Select(x => x.Character.Id));

            await BroadcastMessageN(ChannelRecvCode.MultiChat, res);
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

        internal void BroadcastTeamUpdate(TeamProto.UpdateTeamResponse response)
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastMessage(BroadcastType.OnTeamUpdate, response);
            }
        }

        internal void BroadcastGuildGPUpdate(GuildProto.UpdateGuildGPResponse response)
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastMessage(BroadcastType.OnGuildGpUpdate, response);
            }
        }

        internal void BroadcastGuildRankTitleUpdate(GuildProto.UpdateGuildRankTitleResponse response)
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastMessage(BroadcastType.OnGuildRankTitleUpdate, response);
            }
        }

        internal void BroadcastGuildNoticeUpdate(GuildProto.UpdateGuildNoticeResponse response)
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastMessage(BroadcastType.OnGuildNoticeUpdate, response);
            }
        }

        internal void BroadcastGuildCapacityUpdate(GuildProto.UpdateGuildCapacityResponse response)
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastMessage(BroadcastType.OnGuildCapacityUpdate, response);
            }
        }

        internal void BroadcastGuildEmblemUpdate(GuildProto.UpdateGuildEmblemResponse response)
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastMessage(BroadcastType.OnGuildEmblemUpdate, response);
            }
        }

        internal void BroadcastGuildDisband(GuildProto.GuildDisbandResponse response)
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastMessage(BroadcastType.OnGuildDisband, response);
            }
        }

        internal void BroadcastGuildRankChanged(GuildProto.UpdateGuildMemberRankResponse response)
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastMessage(BroadcastType.OnGuildRankChanged, response);
            }
        }

        internal void BroadcastGuildExpelMember(GuildProto.ExpelFromGuildResponse response)
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastMessage(BroadcastType.OnGuildExpelMember, response);
            }
        }

        internal void BroadcastPlayerJoinGuild(GuildProto.JoinGuildResponse response)
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastMessage(BroadcastType.OnPlayerJoinGuild, response);
            }
        }

        internal void BroadcastPlayerLeaveGuild(GuildProto.LeaveGuildResponse response)
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastMessage(BroadcastType.OnPlayerLeaveGuild, response);
            }
        }

        internal void BroadcastPlayerLevelChanged(CharacterLiveObject obj)
        {
            BroadcastPlayerFieldChange(BroadcastType.OnPlayerLevelChanged, obj);
        }

        internal void BroadcastPlayerJobChanged(CharacterLiveObject obj)
        {
            BroadcastPlayerFieldChange(BroadcastType.OnPlayerJobChanged, obj);
        }

        internal void BroadcastPlayerLoginOff(PlayerOnlineChange response)
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastMessage(BroadcastType.OnPlayerLoginOff, response);
            }
        }

        void BroadcastPlayerFieldChange(string evt, CharacterLiveObject obj)
        {
            SyncProto.PlayerFieldChange response = new SyncProto.PlayerFieldChange
            {
                Channel = obj.Channel,
                FamilyId = obj.Character.FamilyId,
                GuildId = obj.Character.GuildId,
                TeamId = obj.Character.Party,
                Id = obj.Character.Id,
                JobId = obj.Character.JobId,
                Level = obj.Character.Level,
                MapId = obj.Character.Map,
                Name = obj.Character.Name,
                MedalItemId = obj.InventoryItems.FirstOrDefault(x => x.InventoryType == (int)InventoryType.EQUIPPED && x.Position == EquipSlot.Medal)?.Itemid ?? 0
            };
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastMessage(evt, response);
            }
        }

        internal void BroadcastGuildJoinAlliance(AllianceProto.GuildJoinAllianceResponse response)
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastMessage(BroadcastType.OnGuildJoinAlliance, response);
            }
        }

        internal void BroadcastGuildLeaveAlliance(AllianceProto.GuildLeaveAllianceResponse response)
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastMessage(BroadcastType.OnGuildLeaveAlliance, response);
            }
        }

        internal void BroadcastAllianceExpelGuild(AllianceProto.AllianceExpelGuildResponse response)
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastMessage(BroadcastType.OnAllianceExpelGuild, response);
            }
        }

        internal void BroadcastAllianceCapacityIncreased(AllianceProto.IncreaseAllianceCapacityResponse response)
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastMessage(BroadcastType.OnAllianceCapacityUpdate, response);
            }
        }

        internal void BroadcastAllianceRankTitleChanged(AllianceProto.UpdateAllianceRankTitleResponse response)
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastMessage(BroadcastType.OnAllianceRankTitleUpdate, response);
            }
        }

        internal void BroadcastAllianceNoticeChanged(AllianceProto.UpdateAllianceNoticeResponse response)
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastMessage(BroadcastType.OnAllianceNoticeUpdate, response);
            }
        }

        internal void BroadcastAllianceLeaderChanged(AllianceProto.AllianceChangeLeaderResponse response)
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastMessage(BroadcastType.OnAllianceChangeLeader, response);
            }
        }

        internal void BroadcastAllianceMemberRankChanged(AllianceProto.ChangePlayerAllianceRankResponse response)
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastMessage(BroadcastType.OnAllianceRankChange, response);
            }
        }

        internal void BroadcastAllianceDisband(AllianceProto.DisbandAllianceResponse response)
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

        internal void ReturnInvitationCreated(InvitationProto.CreateInviteResponse response)
        {
            SendMessage(BroadcastType.OnInvitationSend, response, [response.SenderPlayerId, response.ReceivePlayerId]);
        }

        internal void ReturnInvitationAnswer(InvitationProto.AnswerInviteResponse response)
        {
            SendMessage(BroadcastType.OnInvitationAnswer, response, [response.SenderPlayerId, response.ReceivePlayerId]);
        }

        internal async Task BroadcastShutdown()
        {
            await BroadcastMessageN(ChannelRecvCode.UnregisterChannel, new Google.Protobuf.WellKnownTypes.Empty());
        }

        internal void SendNewYearCardReceived(ReceiveNewYearCardResponse response)
        {
            int[] to = response.Model == null ? [response.Request.MasterId] : [response.Request.MasterId, response.Model.SenderId];
            SendMessage(BroadcastType.OnNewYearCardReceived, response, to);
        }

        internal void SendNewYearCards(SendNewYearCardResponse response)
        {
            SendMessage(BroadcastType.OnNewYearCardSend, response, [response.Request.FromId]);
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


        internal void BroadcastPLifeCreated(LifeProto.CreatePLifeRequest request)
        {
            BroadcastMessage(BroadcastType.OnPLifeCreated, request);
        }

        internal void BroadcastPLifeRemoved(LifeProto.RemovePLifeResponse request)
        {
            BroadcastMessage(BroadcastType.OnPLifeRemoved, request);
        }

        internal void ReturnBuyCashItem(BuyCashItemResponse buyCashItemResponse)
        {
            SendMessage(BroadcastType.OnCashItemPurchased, buyCashItemResponse, [buyCashItemResponse.MasterId]);
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

        internal void BroadcastBanned(SystemProto.BanBroadcast data)
        {
            BroadcastMessage(BroadcastType.BroadcastBan, data);
        }

        internal void BroadcastGmLevelChanged(SystemProto.SetGmLevelBroadcast data)
        {
            SendMessage(BroadcastType.OnGmLevelSet, data, [data.TargetId]);
        }

        internal void SendWrapPlayerByName(SystemProto.SummonPlayerByNameBroadcast data)
        {
            SendMessage(BroadcastType.SendWrapPlayerByName, data, [data.MasterId]);
        }
    }
}
