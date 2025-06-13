using Application.Core.Game.Controllers;
using Application.Core.Game.Players;
using Application.Core.Login.Models;
using Application.Core.Login.Models.Invitations;
using Application.Shared.Invitations;
using Application.Utility;
using Application.Utility.Configs;
using Application.Utility.Exceptions;
using Dto;
using Microsoft.Extensions.Logging;
using net.server.guild;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Collections.Concurrent;
using tools;

namespace Application.Core.Login.ServerData
{
    /// <summary>
    /// 邀请过期检查
    /// </summary>
    public class InvitationManager : TimelyControllerBase
    {
        readonly MasterServer _server;
        readonly ILogger<InvitationManager> _logger;
        public ConcurrentDictionary<InviteTypeEnum, InviteType> AllInviteTypes;

        public InvitationManager(MasterServer server, ILogger<InvitationManager> logger)
            : base("InvitationExpireCheckTask", TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30))
        {
            _server = server;
            _logger = logger;
            AllInviteTypes = new(EnumClassUtils.GetValues<InviteType>().ToDictionary(x => x.Value));
        }

        public void AddInvitation(CreateInviteRequest request)
        {
            InviteResponseCode responseCode = InviteResponseCode.Success;

            var enumType = (InviteTypeEnum)request.Type;
            if (AllInviteTypes.TryGetValue(enumType, out var type))
            {
                var fromPlayer = _server.CharacterManager.FindPlayerById(request.FromId)!;

                CharacterLiveObject? toPlayer = null;
                int key = 0;

                switch (enumType)
                {
                    case InviteTypeEnum.FAMILY:
                        break;
                    case InviteTypeEnum.FAMILY_SUMMON:
                        break;
                    case InviteTypeEnum.MESSENGER:
                        break;
                    case InviteTypeEnum.TRADE:
                        break;
                    case InviteTypeEnum.PARTY:
                        {
                            toPlayer = _server.CharacterManager.FindPlayerByName(request.ToName);
                            if (toPlayer == null)
                            {
                                responseCode = InviteResponseCode.InviteesNotFound;
                                break;
                            }
                            if (toPlayer.Character.Level < 10 && (!YamlConfig.config.server.USE_PARTY_FOR_STARTERS || fromPlayer.Character.Level >= 10))
                            {
                                responseCode = InviteResponseCode.Team_BeginnerLimit;
                                break;
                            }
                            if (YamlConfig.config.server.USE_PARTY_FOR_STARTERS && toPlayer.Character.Level >= 10 && fromPlayer.Character.Level < 10)
                            {
                                responseCode = InviteResponseCode.Team_BeginnerLimit;
                                break;
                            }
                            if (toPlayer.Character.Party > 0)
                            {
                                responseCode = InviteResponseCode.Team_AlreadyInTeam;
                                break;
                            }

                            key = fromPlayer.Character.Party;
                        }
                        break;
                    case InviteTypeEnum.GUILD:
                        {
                            toPlayer = _server.CharacterManager.FindPlayerByName(request.ToName);
                            if (toPlayer == null)
                            {
                                responseCode = InviteResponseCode.InviteesNotFound;
                                break;
                            }
                            if (toPlayer.Character.GuildId > 0)
                            {
                                responseCode = InviteResponseCode.Guild_AlreadInGuild;
                                break;
                            }
                            key = fromPlayer.Character.GuildId;
                        }
                        break;
                    case InviteTypeEnum.ALLIANCE:
                        {
                            var toGuild = _server.GuildManager.FindGuildByName(request.ToName);
                            if (toGuild == null)
                            {
                                responseCode = InviteResponseCode.Alliance_GuildNotFound;
                                break;
                            }
                            toPlayer = _server.CharacterManager.FindPlayerById(toGuild.Leader);
                            if (toPlayer == null || toPlayer.Channel <= 0)
                            {
                                responseCode = InviteResponseCode.Alliance_GuildLeaderNotFound;
                                break;
                            }
                            if (toGuild.AllianceId > 0)
                            {
                                responseCode = InviteResponseCode.Alliance_AlreadyInAlliance;
                                break;
                            }

                            var fromGuild = _server.GuildManager.GetLocalGuild(fromPlayer.Character.GuildId);
                            if (fromGuild == null)
                            {
                                // 不会触发
                                break;
                            }
                            var alliance = _server.GuildManager.GetLocalAlliance(fromGuild.AllianceId);
                            if (alliance == null)
                            {
                                // 不会触发
                                break;
                            }
                            if (alliance.Capacity == alliance.Guilds.Count)
                            {
                                responseCode = InviteResponseCode.Alliance_CapacityFull;
                                break;
                            }
                            key = fromGuild.AllianceId;
                        }
                        break;
                    default:
                        break;
                }
                if (responseCode == InviteResponseCode.Success)
                {
                    var inviteRequest = new InviteRequest(
                        enumType,
                        _server.getCurrentTime(),
                        request.FromId, fromPlayer.Character.Name,
                        toPlayer!.Character.Id, toPlayer.Character.Name,
                        key,
                        request.ToName);
                    var createResult = type.CreateInvite(inviteRequest);
                    if (!createResult)
                    {
                        responseCode = InviteResponseCode.MANAGING_INVITE;
                    }
                }
                var response = new CreateInviteResponse
                {
                    Code = (int)responseCode,
                    SenderPlayerId = fromPlayer.Character.Id,
                    SenderPlayerName = fromPlayer.Character.Name,
                    Type = request.Type
                };
                if (responseCode == InviteResponseCode.Success)
                {
                    response.Key = key;
                    response.ReceivePlayerId = toPlayer!.Character.Id;
                    response.ReceivePlayerName = toPlayer.Character.Name;
                }
                _server.Transport.ReturnInvitatioCreated(response);
            }
            else
            {
                throw new BusinessFatalException($"不存在的邀请类型：{enumType}");
            }

        }

        public void AnswerInvitation(AnswerInviteRequest request)
        {
            var enumType = (InviteTypeEnum)request.Type;
            if (AllInviteTypes.TryGetValue(enumType, out var type))
            {
                var result = type.AnswerInvite(request.MasterId, request.Ok);
                AnswerInviteResponse response = new AnswerInviteResponse
                {
                    Result = (int)result.Result,
                    Type = request.Type,
                    SenderPlayerId = request.MasterId,
                };

                if (result.Result != InviteResultType.NOT_FOUND && result.Request != null)
                {
                    if (result.Result == InviteResultType.ACCEPTED)
                    {
                        if (result.Request.Type == InviteTypeEnum.PARTY)
                        {
                            _server.TeamManager.UpdateParty(result.Request.Key, Shared.Team.PartyOperation.JOIN, result.Request.FromPlayerId, result.Request.ToPlayerId);
                        }
                        if (result.Request.Type == InviteTypeEnum.GUILD)
                        {
                            _server.GuildManager.PlayerJoinGuild(new JoinGuildRequest { PlayerId = result.Request.ToPlayerId, GuildId = result.Request.Key });
                        }
                        if (result.Request.Type == InviteTypeEnum.ALLIANCE)
                        {
                            _server.GuildManager.GuildJoinAlliance(new GuildJoinAllianceRequest { MasterId = result.Request.ToPlayerId, AllianceId = result.Request.Key });
                        }
                    }

                    response.Code = (int)result.Result;
                    response.Result = (int)result.Result;
                    response.Key = result.Request.Key;
                    response.ReceivePlayerId = result.Request.ToPlayerId;
                    response.ReceivePlayerName = result.Request.ToPlayerName;
                    response.Type = (int)result.Request.Type;
                    response.SenderPlayerId = result.Request.FromPlayerId;
                    response.SenderPlayerName = result.Request.FromPlayerName;
                    response.TargetName = result.Request.TargetName;
                }
                _server.Transport.ReturnInvitationAnswer(response);
                return;
            }
            throw new BusinessFatalException($"不存在的邀请类型：{enumType}");
        }

        public void RemovePlayerIncomingInvites(int cid)
        {
            foreach (InviteType it in AllInviteTypes.Values)
            {
                it.RemoveRequest(cid);
            }
        }

        protected override void HandleRun()
        {
            var now = _server.getCurrentTime();
            foreach (InviteType it in AllInviteTypes.Values)
            {
                it.CheckExpired(now);
            }
        }

        public override async ValueTask DisposeAsync()
        {
            await base.DisposeAsync();
            foreach (var item in AllInviteTypes.Values)
            {
                item.Dispose();
            }
        }


    }

}

