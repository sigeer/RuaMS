using Application.Core.Login.Models;
using Application.Core.Login.Services;
using Application.EF;
using Application.Shared.Constants;
using Application.Shared.Constants.Buddy;
using Application.Shared.Message;
using AutoMapper;
using Dto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Core.Login.ServerData
{
    public class BuddyManager
    {
        readonly MasterServer _server;
        readonly IMapper _mapper;
        readonly ILogger<BuddyManager> _logger;
        readonly InvitationService _invitationService;
        readonly IDbContextFactory<DBContext> _dbContextFactory;


        public BuddyManager(MasterServer server, IMapper mapper, ILogger<BuddyManager> logger, InvitationService invitationService, IDbContextFactory<DBContext> dbContextFactory)
        {
            _server = server;
            _mapper = mapper;
            _logger = logger;
            _invitationService = invitationService;
            _dbContextFactory = dbContextFactory;
        }

        public static Dto.BuddyDto GetChrBuddyDto(int chrId, CharacterLiveObject buddyChr, string group = StringConstants.Buddy_DefaultGroup)
        {
            return new Dto.BuddyDto
            {
                Channel = buddyChr.BuddyList.ContainsKey(chrId) ? buddyChr.Channel : -1,
                ActualChannel = buddyChr.BuddyList.ContainsKey(chrId) ? buddyChr.ActualChannel : -1,
                Group = group,
                Id = buddyChr.Character.Id,
                Name = buddyChr.Character.Name,
                MapId = buddyChr.Character.Map
            };
        }

        Dto.AddBuddyResponse AddBuddy(CharacterLiveObject masterChr, CharacterLiveObject targetChr, string groupName = StringConstants.Buddy_DefaultGroup)
        {
            masterChr.BuddyList[targetChr.Character.Id] = new BuddyModel() { Id = targetChr.Character.Id, CharacterId = masterChr.Character.Id, Group = groupName };

            var data = new Dto.AddBuddyBroadcast()
            {
                ReceiverId = targetChr.Character.Id,
                Buddy = GetChrBuddyDto(targetChr.Character.Id, masterChr)
            };
            _server.Transport.SendMessage(BroadcastType.Buddy_Added, data, data.ReceiverId);

            return new Dto.AddBuddyResponse
            {
                Buddy = GetChrBuddyDto(masterChr.Character.Id, targetChr, groupName)
            };
        }

        public Dto.AddBuddyResponse AddBuddyByName(Dto.AddBuddyRequest request)
        {
            var targetChr = _server.CharacterManager.FindPlayerByName(request.TargetName);
            if (targetChr == null)
                return new Dto.AddBuddyResponse() { Code = 1 };

            var masterChr = _server.CharacterManager.FindPlayerById(request.MasterId);
            if (masterChr == null)
                return new Dto.AddBuddyResponse() { Code = 1 };

            return AddBuddy(masterChr, targetChr, request.GroupName);
        }

        public Dto.AddBuddyResponse AddBuddyById(Dto.AddBuddyByIdRequest request)
        {
            var targetChr = _server.CharacterManager.FindPlayerById(request.TargetId);
            if (targetChr == null)
                return new Dto.AddBuddyResponse() { Code = 1 };

            var masterChr = _server.CharacterManager.FindPlayerById(request.MasterId);
            if (masterChr == null)
                return new Dto.AddBuddyResponse() { Code = 1 };

            return AddBuddy(masterChr, targetChr);
        }

        public void BuddyChat(BuddyChatRequest request)
        {
            var data = new Dto.BuddyChatBroadcast();
            data.ToIds.AddRange(request.ToIds);
            data.FromName = _server.CharacterManager.GetPlayerName(request.FromId);
            data.FromId = request.FromId;
            data.Text = request.Text;
            _server.Transport.SendMessage(BroadcastType.Buddy_Chat, data, data.ToIds.ToArray());
        }

        public void BroadcastNotify(CharacterLiveObject obj)
        {
            var data = new Dto.NotifyBuddyWhenLoginoffBroadcast();
            data.BuddyId.AddRange(obj.BuddyList.Keys);
            data.Channel = obj.Channel;
            data.ActualChannel = obj.ActualChannel;
            data.MasterId = obj.Character.Id;
            _server.Transport.SendMessage(BroadcastType.Buddy_NotifyChannel, data, data.BuddyId.ToArray());
        }

        public void BroadcastNoticeMessage(SendBuddyNoticeMessageDto data)
        {
            _server.Transport.SendMessage(BroadcastType.Buddy_NoticeMessage, data, data.BuddyId.ToArray());
        }

        public Dto.DeleteBuddyResponse DeleteBuddy(Dto.DeleteBuddyRequest request)
        {
            var masterChr = _server.CharacterManager.FindPlayerById(request.MasterId);
            if (masterChr == null)
                return new DeleteBuddyResponse() { Code = 1 };

            masterChr.BuddyList.Remove(request.Buddyid);
            _server.Transport.SendMessage(BroadcastType.Buddy_Delete, new Dto.DeleteBuddyBroadcast { MasterId = request.Buddyid, Buddyid = request.MasterId }, request.Buddyid);
            return new DeleteBuddyResponse();
        }

        public SendWhisperMessageResponse SendWhisper(SendWhisperMessageRequest request)
        {
            var senderChr = _server.CharacterManager.FindPlayerById(request.FromId);
            if (senderChr == null || senderChr.Channel < 0)
                return new SendWhisperMessageResponse { Code = 1 };

            var receiverChr = _server.CharacterManager.FindPlayerByName(request.TargetName);
            if (receiverChr == null || senderChr.Channel < 0)
                return new SendWhisperMessageResponse { Code = 1 };

            var data = new Dto.SendWhisperMessageBroadcast
            {
                FromChannel = senderChr.Channel,
                FromName = senderChr.Character.Name,
                ReceiverId = receiverChr.Character.Id,
                Text = request.Text
            };
            _server.Transport.SendMessage(BroadcastType.Whisper_Chat, data, data.ReceiverId);
            return new SendWhisperMessageResponse();
        }

        public GetLocationResponse GetLocation(GetLocationRequest request)
        {
            var targetChr = _server.CharacterManager.FindPlayerByName(request.TargetName);
            if (targetChr == null)
                return new GetLocationResponse { Code = (int)WhisperLocationResponseCode.NotFound };
            if (targetChr.Channel == 0)
                return new GetLocationResponse { Code = (int)WhisperLocationResponseCode.NotOnlined };
            if (targetChr.Channel < 0)
                return new GetLocationResponse { Code = (int)WhisperLocationResponseCode.AwayWorld };

            return new GetLocationResponse { Code = (int)WhisperLocationResponseCode.DiffChannel, Field = targetChr.Channel };
        }
    }
}
