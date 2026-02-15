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
using System.Reflection;
using System.Threading.Tasks;

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

        public static BuddyProto.BuddyDto GetChrBuddyDto(int chrId, CharacterLiveObject buddyChr, string group = StringConstants.Buddy_DefaultGroup)
        {
            return new BuddyProto.BuddyDto
            {
                Channel = buddyChr.BuddyList.ContainsKey(chrId) ? buddyChr.Channel : -1,
                ActualChannel = buddyChr.BuddyList.ContainsKey(chrId) ? buddyChr.ActualChannel : -1,
                Group = group,
                Id = buddyChr.Character.Id,
                Name = buddyChr.Character.Name,
                MapId = buddyChr.Character.Map
            };
        }

        async Task AddBuddy(CharacterLiveObject masterChr, CharacterLiveObject targetChr, string groupName = StringConstants.Buddy_DefaultGroup)
        {
            masterChr.BuddyList[targetChr.Character.Id] = new BuddyModel() { Id = targetChr.Character.Id, CharacterId = masterChr.Character.Id, Group = groupName };
            _server.CharacterManager.SetState(masterChr.Character.Id);

            var data = new BuddyProto.AddBuddyResponse()
            {
                Code = 1,
                TargetId = targetChr.Character.Id,
                MasterId = masterChr.Character.Id,
                Buddy = GetChrBuddyDto(masterChr.Character.Id, targetChr)
            };
            await _server.Transport.BroadcastMessageN(ChannelRecvCode.OnBuddyAdd, data);
        }

        public async Task AddBuddyByName(BuddyProto.AddBuddyRequest request)
        {
            var res = new BuddyProto.AddBuddyResponse() { MasterId  = request.MasterId, TargetName = request.TargetName };
            var masterChr = _server.CharacterManager.FindPlayerById(request.MasterId);
            if (masterChr == null)
            {
                res.Code = 1;
                await _server.Transport.BroadcastMessageN(ChannelRecvCode.OnBuddyAdd, res);
                return;
            }
            

            var targetChr = _server.CharacterManager.FindPlayerByName(request.TargetName);
            if (targetChr == null)
            {
                res.Code = 1;
                await _server.Transport.BroadcastMessageN(ChannelRecvCode.OnBuddyAdd, res);
                return;
            }

            masterChr.BuddyList[targetChr.Character.Id] = new BuddyModel() { Id = targetChr.Character.Id, CharacterId = masterChr.Character.Id, Group = request.GroupName };
            _server.CharacterManager.SetState(masterChr.Character.Id);

            res.TargetId = targetChr.Character.Id;
            res.Buddy = GetChrBuddyDto(res.MasterId, targetChr);
            await _server.Transport.BroadcastMessageN(ChannelRecvCode.OnBuddyAdd, res);
        }

        public async Task AddBuddyById(BuddyProto.AddBuddyByIdRequest request)
        {
            var targetChr = _server.CharacterManager.FindPlayerById(request.TargetId);
            if (targetChr == null)
                return;

            var masterChr = _server.CharacterManager.FindPlayerById(request.MasterId);
            if (masterChr == null)
                return;

            await AddBuddy(masterChr, targetChr);
        }

        public async Task SendBuddyChatAsync(string fromName, string text, int[] to)
        {
            var targetChr = _server.CharacterManager.FindPlayerByName(fromName);
            if (targetChr == null)
                return;

            var tos = to.Select(x => _server.CharacterManager.FindPlayerById(x))
                .Where(y => y != null && y.Channel > 0).ToList();
            await _server.Transport.SendMultiChatAsync(3, fromName, tos, text);
        }

        public async Task BroadcastNoticeMessage(BuddyProto.SendBuddyNoticeMessageDto data)
        {
            var chr = _server.CharacterManager.FindPlayerById(data.MasterId)!;
            await _server.DropWorldMessage(data.Type, data.Message, chr.BuddyList.Keys.ToArray());
        }

        public async Task DeleteBuddy(BuddyProto.DeleteBuddyRequest request)
        {
            var res = new BuddyProto.DeleteBuddyResponse { MasterId = request.MasterId, Buddyid = request.Buddyid };
            var masterChr = _server.CharacterManager.FindPlayerById(request.MasterId);
            if (masterChr == null)
            {
                res.Code = 1;
            }
            else
            {
                masterChr.BuddyList.Remove(request.Buddyid);
                _server.CharacterManager.SetState(masterChr.Character.Id);
            }

            await _server.Transport.SendMessageN(ChannelRecvCode.OnBuddyRemove, res, [request.MasterId, request.Buddyid]);
        }

        public async Task SendWhisper(MessageProto.SendWhisperMessageRequest request)
        {
            var res = new MessageProto.SendWhisperMessageResponse() { Request = request  } ;
            var senderChr = _server.CharacterManager.FindPlayerById(request.FromId);
            if (senderChr == null || senderChr.Channel <= 0)
            {
                res.Code = 1;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnWhisper, res, [res.Request.FromId]);
                return;
            }

            var receiverChr = _server.CharacterManager.FindPlayerByName(request.TargetName);
            if (receiverChr == null || senderChr.Channel <= 0)
            {
                res.Code = 1;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnWhisper, res, [res.Request.FromId]);
                return;
            }

            res.FromChannel = senderChr.Channel;
            res.FromName = senderChr.Character.Name;
            res.ReceiverId = receiverChr.Character.Id;

            await _server.Transport.SendMessageN(ChannelRecvCode.OnWhisper, res, [res.ReceiverId]);
        }

        public async Task GetLocation(BuddyProto.GetLocationRequest request)
        {
            var res = new BuddyProto.GetLocationResponse() { MasterId = request.MasterId, TargetName = request.TargetName };
            var targetChr = _server.CharacterManager.FindPlayerByName(request.TargetName);
            if (targetChr == null)
                res.Code = (int)WhisperLocationResponseCode.NotFound;
            else if (targetChr.Channel == 0)
                res.Code = (int)WhisperLocationResponseCode.NotOnlined;
            else if (targetChr.Channel < 0)
                res.Code = (int)WhisperLocationResponseCode.AwayWorld;

            res.Code = (int)WhisperLocationResponseCode.DiffChannel;
            await _server.Transport.SendMessageN(ChannelRecvCode.OnBuddyLocation, res, [request.MasterId]);
        }
    }
}
