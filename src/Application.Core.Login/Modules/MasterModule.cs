using Application.Core.Login.Models;
using Application.Resources.Messages;
using Application.Shared.Message;
using Application.Shared.Team;
using GuildProto;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Numerics;

namespace Application.Core.Login.Modules
{
    public sealed class MasterModule : AbstractMasterModule
    {
        public MasterModule(MasterServer server, ILogger<MasterModule> logger) : base(server, logger)
        {
        }

        public override async Task OnPlayerServerChanged(CharacterLiveObject obj, int lastChannel)
        {
            await base.OnPlayerServerChanged(obj, lastChannel);

            await _server.ChatRoomManager.LeaveChatRoom(new Dto.LeaveChatRoomRequst { MasterId = obj.Character.Id });
            await _server.InvitationManager.RemovePlayerInvitation(obj.Character.Id);

            await _server.Transport.BroadcastPlayerFieldChange(ChannelRecvCode.OnPlayerServerChanged, obj, lastChannel);

            await _server.TeamManager.UpdateParty(obj.Character.Party, PartyOperation.LOG_ONOFF, obj.Character.Id, obj.Character.Id);

        }
        /// <summary>
        /// 进入频道
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="isNewComer"></param>
        public override async Task OnPlayerLogin(CharacterLiveObject obj)
        {
            if (_server.AccountManager.TryGetGMLevel(obj.Character.AccountId, out var gmLevel))
                await _server.DropWorldMessage(-2, string.Format(SystemMessage.System_GmLoggedin, gmLevel < 6 ? "GM" : "Admin", obj.Character.Name), true);

            await _server.NoteManager.SendNote(obj);

            await _server.DueyManager.SendDueyNotifyOnLogin(obj.Character.Id);

            var guild = _server.GuildManager.GetLocalGuild(obj.Character.GuildId);
            if (guild != null)
            {
                var res = new GuildMemberServerChangedResponse
                {
                    GuildId = guild.GuildId,
                    MemberChanel = obj.Channel,
                    MemberId = obj.Character.Id,
                    AllianceId = guild.AllianceId,
                };
                var allMembers = _server.GuildManager.GetAllianceMembers(guild);
                res.AllMembers.AddRange(allMembers);
                await _server.Transport.SendMessageN(ChannelRecvCode.OnGuildMemberLoginOff, res, allMembers);
            }
        }

        public override async Task OnPlayerLogoff(CharacterLiveObject obj)
        {
            obj.Character.LastLogoutTime = _server.GetCurrentTimeDateTimeOffset();
            obj.ChannelNode = null;

            var guild = _server.GuildManager.GetLocalGuild(obj.Character.GuildId);
            if (guild != null)
            {
                var res = new GuildMemberServerChangedResponse
                {
                    GuildId = guild.GuildId,
                    MemberChanel = obj.Channel,
                    MemberId = obj.Character.Id,
                    AllianceId = guild.AllianceId,
                };
                var allMembers = _server.GuildManager.GetAllianceMembers(guild);
                res.AllMembers.AddRange(allMembers);
                await _server.Transport.SendMessageN(ChannelRecvCode.OnGuildMemberLoginOff, res, allMembers);
            }
        }

        public override async Task OnPlayerMapChanged(CharacterLiveObject obj)
        {
            // await _server.TeamManager.UpdateParty(obj.Character.Party, PartyOperation.SILENT_UPDATE, obj.Character.Id, obj.Character.Id);
        }

        public override async Task OnPlayerJobChanged(CharacterLiveObject origin)
        {
            await _server.Transport.BroadcastPlayerFieldChange(ChannelRecvCode.OnPlayerJobChanged, origin, origin.Channel);

            await _server.TeamManager.UpdateParty(origin.Character.Party, PartyOperation.SILENT_UPDATE, origin.Character.Id, origin.Character.Id);

            var guild = _server.GuildManager.GetLocalGuild(origin.Character.GuildId);
            if (guild != null)
            {
                var res = new GuildMemberUpdateResponse
                {
                    GuildId = guild.GuildId,
                    MemberJob = origin.Character.JobId,
                    MemberId = origin.Character.Id,
                    MemberName = origin.Character.Name,
                    MemberLevel = origin.Character.Level,
                    AllianceId = guild.AllianceId,
                };
                var allMembers = _server.GuildManager.GetAllianceMembers(guild);
                res.AllMembers.AddRange(allMembers);
                await _server.Transport.SendMessageN(ChannelRecvCode.OnGuildMemberUpdate, res, allMembers);
            }
        }

        public override async Task OnPlayerLevelChanged(CharacterLiveObject origin)
        {
            await _server.Transport.BroadcastPlayerFieldChange(ChannelRecvCode.OnPlayerLevelChanged, origin, origin.Channel);

            await _server.TeamManager.UpdateParty(origin.Character.Party, PartyOperation.SILENT_UPDATE, origin.Character.Id, origin.Character.Id);

            var guild = _server.GuildManager.GetLocalGuild(origin.Character.GuildId);
            if (guild != null)
            {
                var res = new GuildMemberUpdateResponse
                {
                    GuildId = guild.GuildId,
                    MemberJob = origin.Character.JobId,
                    MemberId = origin.Character.Id,
                    MemberName = origin.Character.Name,
                    MemberLevel = origin.Character.Level,
                    AllianceId = guild.AllianceId,
                };
                var allMembers = _server.GuildManager.GetAllianceMembers(guild);
                res.AllMembers.AddRange(allMembers);
                await _server.Transport.SendMessageN(ChannelRecvCode.OnGuildMemberUpdate, res, allMembers);
            }
        }
    }
}
