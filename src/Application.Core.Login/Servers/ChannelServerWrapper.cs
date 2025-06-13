using Application.Core.Login.ServerTransports;
using Application.Shared.Servers;
using Application.Shared.Team;
using Config;
using Dto;

namespace Application.Core.Login.Servers
{
    public abstract class ChannelServerWrapper : IChannelBroadcast
    {
        protected ChannelServerWrapper(string serverName, List<WorldChannelConfig> serverConfigs)
        {
            ServerName = serverName;
            ServerConfigs = serverConfigs;
        }
        public string ServerName { get; protected set; }
        public List<WorldChannelConfig> ServerConfigs { get; }


        public abstract void SendTeamUpdate(int teamId, PartyOperation operation, TeamMemberDto target);

        public abstract void UpdateCouponConfig(CouponConfig config);
        public abstract void SendMultiChat(int type, string nameFrom, int[] value, string chatText);
        public abstract void DropMessage(int[] value, int type, string message);

        public abstract void BroadcastGuildGPUpdate(UpdateGuildGPResponse response);
        public abstract void BroadcastGuildRankTitleUpdate(UpdateGuildRankTitleResponse response);
        public abstract void BroadcastGuildNoticeUpdate(UpdateGuildNoticeResponse response);
        public abstract void BroadcastGuildCapacityUpdate(UpdateGuildCapacityResponse response);
        public abstract void BroadcastGuildEmblemUpdate(UpdateGuildEmblemResponse response);
        public abstract void BroadcastGuildDisband(GuildDisbandResponse response);
        public abstract void BroadcastGuildRankChanged(UpdateGuildMemberRankResponse response);
        public abstract void BroadcastGuildExpelMember(ExpelFromGuildResponse response);

        public abstract void BroadcastPlayerJoinGuild(JoinGuildResponse response);
        public abstract void BroadcastPlayerLeaveGuild(LeaveGuildResponse response);
        public abstract void BroadcastPlayerLevelChanged(PlayerLevelJobChange response);
        public abstract void BroadcastPlayerJobChanged(PlayerLevelJobChange response);
        public abstract void BroadcastPlayerLoginOff(PlayerOnlineChange response);

        public abstract void BroadcastGuildJoinAlliance(GuildJoinAllianceResponse response);
        public abstract void BroadcastGuildLeaveAlliance(GuildLeaveAllianceResponse response);
        public abstract void BroadcastAllianceExpelGuild(AllianceExpelGuildResponse response);
        public abstract void BroadcastAllianceCapacityIncreased(IncreaseAllianceCapacityResponse response);
        public abstract void BroadcastAllianceRankTitleChanged(UpdateAllianceRankTitleResponse response);
        public abstract void BroadcastAllianceNoticeChanged(UpdateAllianceNoticeResponse response);
        public abstract void BroadcastAllianceLeaderChanged(AllianceChangeLeaderResponse response);
        public abstract void BroadcastAllianceMemberRankChanged(ChangePlayerAllianceRankResponse response);
        public abstract void BroadcastAllianceDisband(DisbandAllianceResponse response);
        public abstract void ReturnInvitatioCreated(CreateInviteResponse response);
        public abstract void ReturnInvitationAnswer(AnswerInviteResponse response);
    }


    //public class RemoteWorldChannel : ChannelServerWrapper
    //{
    //    public RemoteWorldChannel(string instanceId, ChannelServerConfig serverNetInfo) : base(instanceId, serverNetInfo)
    //    {
    //    }

    //    public override void SendTeamUpdate(int teamId, PartyOperation operation, TeamMemberDto target)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
}
