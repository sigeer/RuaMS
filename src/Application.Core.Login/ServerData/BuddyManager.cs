using Application.Core.Login.Models;
using Application.Core.Login.Services;
using Application.EF;
using Application.Shared.Constants;
using Application.Shared.Invitations;
using Application.Shared.Message;
using AutoMapper;
using Dto;
using Microsoft.Extensions.Logging;
using XmlWzReader;

namespace Application.Core.Login.ServerData
{
    public class BuddyManager
    {
        readonly MasterServer _server;
        readonly IMapper _mapper;
        readonly ILogger<BuddyManager> _logger;
        readonly InvitationService _invitationService;

        public BuddyManager(MasterServer server, IMapper mapper, ILogger<BuddyManager> logger, InvitationService invitationService)
        {
            _server = server;
            _mapper = mapper;
            _logger = logger;
            _invitationService = invitationService;
        }

        private Dto.BuddyDto GetDtoByChr(CharacterLiveObject chr, string group = StringConstants.Buddy_DefaultGroup)
        {
            return new Dto.BuddyDto
            {
                Channel = chr.Channel,
                Group = group,
                Id = chr.Character.Id,
                Name = chr.Character.Name,
                MapId = chr.Character.Map
            };
        }

        public Dto.AddBuddyResponse AddBuddy(Dto.AddBuddyRequest request)
        {
            var targetChr = _server.CharacterManager.FindPlayerByName(request.TargetName);
            if (targetChr == null)
                return new Dto.AddBuddyResponse() { Code = 1 };

            _invitationService.AddInvitation(new Dto.CreateInviteRequest { FromId = request.MasterId, ToName = request.TargetName, Type = InviteTypes.Buddy });

            return new Dto.AddBuddyResponse
            {
                Buddy = GetDtoByChr(targetChr, request.GroupName)
            };
        }

        public void PushAcceptBuddy(int masterId, int senderId)
        {
            var chr = _server.CharacterManager.FindPlayerById(senderId);
            if (chr == null)
                return;

            var data = new Dto.AddBuddyBroadcast()
            {
                FromId = senderId,
                ReceiverId = masterId,
                Buddy = GetDtoByChr(chr)
            };
            _server.Transport.SendMessage(BroadcastType.Buddy_AcceptInvite, data, data.ReceiverId);
        }

        public void BuddyChat(BuddyChatRequest request)
        {
            var data = new Dto.BuddyChatBroadcast();
            data.ToIds.AddRange(request.ToIds);
            data.FromName = _server.CharacterManager.GetPlayerName(request.FromId);
            data.FromId = request.FromId;
            data.Text = request.Text;
            _server.Transport.SendMessage(BroadcastType.Buddy_Chat, request, request.ToIds.ToArray());
        }

        public void BroadcastNotify(NotifyBuddyWhenLoginoffRequest request)
        {
            var data = new Dto.NotifyBuddyWhenLoginoffBroadcast();
            data.BuddyId.AddRange(request.BuddyId);
            data.Channel = _server.CharacterManager.FindPlayerById(request.MasterId)?.ActualChannel ?? 0;
            data.IsLogin = request.IsLogin;
            data.MasterId = request.MasterId;
            _server.Transport.SendMessage(BroadcastType.Buddy_NotifyChannel, data, data.BuddyId.ToArray());
        }

        public void BroadcastNoticeMessage(SendBuddyNoticeMessageDto data)
        {
            _server.Transport.SendMessage(BroadcastType.Buddy_NoticeMessage, data, data.BuddyId.ToArray());
        }

        public void DeleteBuddy(Dto.DeleteBuddyBroadcast request)
        {
            _server.Transport.SendMessage(BroadcastType.Buddy_Delete, new Dto.DeleteBuddyBroadcast { MasterId = request.Buddyid, Buddyid = request.MasterId }, request.Buddyid);
        }
    }
}
