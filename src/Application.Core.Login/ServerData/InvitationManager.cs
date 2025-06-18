using Application.Core.Game.Controllers;
using Application.Core.Game.Players;
using Application.Core.Login.Models;
using Application.Core.Login.Models.ChatRoom;
using Application.Core.Login.Models.Invitations;
using Application.Shared.Invitations;
using Application.Utility;
using Application.Utility.Configs;
using Application.Utility.Exceptions;
using Dto;
using Microsoft.Extensions.Logging;
using MySqlX.XDevAPI.Common;
using net.server.guild;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using tools;
using static Google.Protobuf.Reflection.FeatureSet.Types;

namespace Application.Core.Login.ServerData
{
    /// <summary>
    /// 邀请过期检查
    /// </summary>
    public class InvitationManager : TimelyControllerBase
    {
        const long Expired = 3 * 60 * 1000;

        readonly MasterServer _server;
        readonly ILogger<InvitationManager> _logger;
        ConcurrentDictionary<string, ConcurrentDictionary<int, InviteRequest>> _allRequests = new();
        readonly InviteMasterHandlerRegistry _inviteMasterHandlerRegistry;
        public InvitationManager(MasterServer server, ILogger<InvitationManager> logger, InviteMasterHandlerRegistry inviteMasterHandlerRegistry)
            : base("InvitationExpireCheckTask", TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30))
        {
            _server = server;
            _logger = logger;
            _inviteMasterHandlerRegistry = inviteMasterHandlerRegistry;
        }

        protected override void HandleRun()
        {
            var now = _server.getCurrentTime();

            foreach (var typeEntry in _allRequests)
            {
                var typeKey = typeEntry.Key;
                var inviteDict = typeEntry.Value;

                // 找出过期的 Key（避免在遍历时修改字典）
                var expiredKeys = inviteDict
                    .Where(kv => now - kv.Value.CreationTime > Expired)
                    .Select(kv => kv.Key)
                    .ToList();

                // 删除过期邀请
                var handler = _inviteMasterHandlerRegistry.GetHandler(typeKey);
                foreach (var key in expiredKeys)
                {
                    if (inviteDict.TryRemove(key, out var request)) 
                        handler?.OnInvitationExpired(request);
                }
            }
        }

        public override async ValueTask DisposeAsync()
        {
            await base.DisposeAsync();
            foreach (var item in _allRequests.Values)
            {
                _allRequests.Clear();
            }
        }

        internal void RemovePlayerInvitation(int masterId, string enumType)
        {
            if (_allRequests.TryGetValue(enumType, out var type))
            {
                if (type.TryRemove(masterId, out var d)) 
                    _inviteMasterHandlerRegistry.GetHandler(enumType)?.OnInvitationExpired(d);
            }
        }

        internal void RemovePlayerInvitation(int masterId)
        {
            foreach (var it in _allRequests)
            {
                if (it.Value.TryRemove(masterId, out var d))
                    _inviteMasterHandlerRegistry.GetHandler(it.Key)?.OnInvitationExpired(d);
            }
        }

        internal bool TryAddInvitation(string type, int toId, InviteRequest request)
        {
            if (!_allRequests.ContainsKey(type))
                _allRequests[type] = new();
            return _allRequests[type].TryAdd(toId, request);
        }

        private bool IsExpired(InviteRequest request) => _server.getCurrentTime() - request.CreationTime > Expired;

        public InviteResult AnswerInvite(string type, int responseId, int checkKey, bool answer)
        {
            InviteResultType result = InviteResultType.NOT_FOUND;
            if (_allRequests.TryGetValue(type, out var data))
            {
                if (data.TryRemove(responseId, out var d) && (checkKey == -1 || d.Key == checkKey))
                {
                    if (!IsExpired(d))
                    {
                        result = answer ? InviteResultType.ACCEPTED : InviteResultType.DENIED;
                        return new InviteResult(result, d);
                    }
                    _inviteMasterHandlerRegistry.GetHandler(type)?.OnInvitationExpired(d);
                }
            }
            return new InviteResult(result, null);
        }
    }

    public sealed class PartyInviteHandler : InviteMasterHandler
    {
        public PartyInviteHandler(MasterServer server) : base(server, InviteTypes.Party)
        {
        }

        protected override void OnInvitationAccepted(InviteRequest request)
        {
            _server.TeamManager.UpdateParty(request.Key, Shared.Team.PartyOperation.JOIN, request.FromPlayerId, request.ToPlayerId);
        }

        public override void HandleInvitationCreated(CreateInviteRequest request)
        {
            InviteResponseCode responseCode = InviteResponseCode.Success;

            var fromPlayer = _server.CharacterManager.FindPlayerById(request.FromId)!;
            var toPlayer = _server.CharacterManager.FindPlayerByName(request.ToName);
            if (toPlayer == null)
            {
                responseCode = InviteResponseCode.InviteesNotFound;
            }
            else if (toPlayer.Character.Level < 10 && (!YamlConfig.config.server.USE_PARTY_FOR_STARTERS || fromPlayer.Character.Level >= 10))
            {
                responseCode = InviteResponseCode.Team_BeginnerLimit;
            }
            else if (YamlConfig.config.server.USE_PARTY_FOR_STARTERS && toPlayer.Character.Level >= 10 && fromPlayer.Character.Level < 10)
            {
                responseCode = InviteResponseCode.Team_BeginnerLimit;
            }
            else if (toPlayer.Character.Party > 0)
            {
                responseCode = InviteResponseCode.Team_AlreadyInTeam;
            }
            BroadcastResult(responseCode, fromPlayer.Character.Party, fromPlayer, toPlayer, request.ToName);
        }
    }

    public sealed class GuildInviteHandler : InviteMasterHandler
    {
        public GuildInviteHandler(MasterServer server) : base(server, InviteTypes.Guild)
        {
        }

        protected override void OnInvitationAccepted(InviteRequest request)
        {
            _server.GuildManager.PlayerJoinGuild(new JoinGuildRequest { PlayerId = request.ToPlayerId, GuildId = request.Key });
        }

        public override void HandleInvitationCreated(CreateInviteRequest request)
        {
            InviteResponseCode responseCode = InviteResponseCode.Success;
            var fromPlayer = _server.CharacterManager.FindPlayerById(request.FromId)!;
            var toPlayer = _server.CharacterManager.FindPlayerByName(request.ToName);
            if (toPlayer == null)
            {
                responseCode = InviteResponseCode.InviteesNotFound;
            }
            else if (toPlayer.Character.GuildId > 0)
            {
                responseCode = InviteResponseCode.Guild_AlreadInGuild;
            }
            BroadcastResult(responseCode, fromPlayer.Character.GuildId, fromPlayer, toPlayer, request.ToName);
        }
    }

    public sealed class AllianceInviteHandler : InviteMasterHandler
    {
        public AllianceInviteHandler(MasterServer server) : base(server, InviteTypes.Alliance)
        {
        }

        protected override void OnInvitationAccepted(InviteRequest request)
        {
            _server.GuildManager.GuildJoinAlliance(new GuildJoinAllianceRequest { MasterId = request.ToPlayerId, AllianceId = request.Key });
        }

        public override void HandleInvitationCreated(CreateInviteRequest request)
        {
            InviteResponseCode responseCode = InviteResponseCode.Success;
            var fromPlayer = _server.CharacterManager.FindPlayerById(request.FromId)!;
            var toGuild = _server.GuildManager.FindGuildByName(request.ToName);
            if (toGuild == null)
            {
                responseCode = InviteResponseCode.Alliance_GuildNotFound;
            }
            var toPlayer = _server.CharacterManager.FindPlayerById(toGuild.Leader);
            if (toPlayer == null || toPlayer.Channel <= 0)
            {
                responseCode = InviteResponseCode.Alliance_GuildLeaderNotFound;
            }
            if (toGuild.AllianceId > 0)
            {
                responseCode = InviteResponseCode.Alliance_AlreadyInAlliance;
            }

            var fromGuild = _server.GuildManager.GetLocalGuild(fromPlayer.Character.GuildId);
            if (fromGuild == null)
            {
                // 不会触发
            }
            else
            {
                var alliance = _server.GuildManager.GetLocalAlliance(fromGuild.AllianceId);
                if (alliance == null)
                {
                    // 不会触发
                }
                else if (alliance.Capacity == alliance.Guilds.Count)
                {
                    responseCode = InviteResponseCode.Alliance_CapacityFull;
                }
                BroadcastResult(responseCode, fromGuild.AllianceId, fromPlayer, toPlayer, request.ToName);
            }

        }
    }

    public sealed class MessengerInviteHandler : InviteMasterHandler
    {
        public MessengerInviteHandler(MasterServer server) : base(server, InviteTypes.Messenger)
        {
        }

        protected override void OnInvitationAccepted(InviteRequest request)
        {
            _server.ChatRoomManager.JoinChatRoom(new JoinChatRoomRequest { RoomId = request.Key, MasterId = request.ToPlayerId });
        }

        public override void HandleInvitationCreated(CreateInviteRequest request)
        {
            InviteResponseCode responseCode = InviteResponseCode.Success;
            var fromPlayer = _server.CharacterManager.FindPlayerById(request.FromId)!;
            var toPlayer = _server.CharacterManager.FindPlayerByName(request.ToName);
            if (toPlayer == null || toPlayer.Channel <= 0)
            {
                responseCode = InviteResponseCode.InviteesNotFound;
            }

            else if (_server.ChatRoomManager.GetPlayerRoom(toPlayer.Character.Id) != null)
            {
                responseCode = InviteResponseCode.ChatRoom_AlreadInRoom;
            }

            var room = _server.ChatRoomManager.GetPlayerRoom(request.FromId)!;
            if (room.Members.Length == ChatRoomModel.MaxCount)
            {
                responseCode = InviteResponseCode.ChatRoom_CapacityFull;
            }
            BroadcastResult(responseCode, room.Id, fromPlayer, toPlayer, request.ToName);
        }
    }
}

