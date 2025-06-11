using Application.Core.Login.Servers;
using Application.Shared.Team;
using Config;
using Dto;

namespace Application.Core.Channel.Local
{
    public class InternalWorldChannel : ChannelServerWrapper
    {
        public InternalWorldChannel(WorldChannelServer worldChannel, List<WorldChannel> channels) : base(worldChannel.ServerName, channels.Select(x => x.ChannelConfig).ToList())
        {
            ChannelServer = worldChannel;
        }

        public WorldChannelServer ChannelServer { get; }

        public override void SendTeamUpdate(int teamId, PartyOperation operation, TeamMemberDto target)
        {
            ChannelServer.TeamManager.ProcessUpdateResponse(teamId, operation, target);
        }

        public override void UpdateCouponConfig(CouponConfig config)
        {
            ChannelServer.UpdateCouponConfig(config);

            foreach (var ch in ChannelServer.Servers.Values)
            {
                foreach (var chr in ch.getPlayerStorage().getAllCharacters())
                {
                    if (!chr.isLoggedin())
                    {
                        continue;
                    }

                    chr.updateCouponRates();
                }
            }
        }

        public override void SendMultiChat(int type, string nameFrom, int[] value, string chatText)
        {
            ChannelServer.SendMultiChat(type, nameFrom, value, chatText);
        }

        public override void DropMessage(int[] value, int type, string message)
        {
            ChannelServer.DropMessage(value, type, message);
        }

        public override void BroadcastGuildGPUpdate(UpdateGuildGPResponse response)
        {
            ChannelServer.BroadcastGuildGPUpdate(response);
        }

        public override void BroadcastGuildRankTitleUpdate(UpdateGuildRankTitleResponse response)
        {
            ChannelServer.GuildManager.OnGuildRankTitleUpdate(response);
        }

        public override void BroadcastGuildNoticeUpdate(UpdateGuildNoticeResponse response)
        {
            ChannelServer.GuildManager.OnGuildNoticeUpdate(response);
        }

        public override void BroadcastGuildCapacityUpdate(UpdateGuildCapacityResponse response)
        {
            ChannelServer.GuildManager.OnGuildCapacityIncreased(response);
        }

        public override void BroadcastGuildEmblemUpdate(UpdateGuildEmblemResponse response)
        {
            ChannelServer.GuildManager.OnGuildEmblemUpdate(response);
        }

        public override void BroadcastGuildDisband(GuildDisbandResponse response)
        {
            ChannelServer.GuildManager.OnGuildDisband(response);
        }

        public override void BroadcastGuildRankChanged(UpdateGuildMemberRankResponse response)
        {
            ChannelServer.GuildManager.OnChangePlayerGuildRank(response);
        }

        public override void BroadcastGuildExpelMember(ExpelFromGuildResponse response)
        {
            ChannelServer.GuildManager.OnGuildExpelMember(response);
        }

        public override void BroadcastPlayerJoinGuild(JoinGuildResponse response)
        {
            ChannelServer.GuildManager.OnPlayerJoinGuild(response);
        }

        public override void BroadcastPlayerLeaveGuild(LeaveGuildResponse response)
        {
            ChannelServer.GuildManager.OnPlayerLeaveGuild(response);
        }

        public override void BroadcastPlayerLevelChanged(PlayerLevelJobChange response)
        {
            ChannelServer.OnPlayerLevelChanged(response);
        }

        public override void BroadcastPlayerJobChanged(PlayerLevelJobChange response)
        {
            ChannelServer.OnPlayerJobChanged(response);
        }

        public override void BroadcastPlayerLoginOff(PlayerOnlineChange response)
        {
            ChannelServer.OnPlayerLoginOff(response);
        }

        #region Alliance
        public override void BroadcastGuildJoinAlliance(GuildJoinAllianceResponse response)
        {
            ChannelServer.GuildManager.OnGuildJoinAlliance(response);
        }

        public override void BroadcastAllianceCapacityIncreased(IncreaseAllianceCapacityResponse response)
        {
            ChannelServer.GuildManager.OnAllianceCapacityIncreased(response);
        }

        public override void BroadcastGuildLeaveAlliance(GuildLeaveAllianceResponse response)
        {
            ChannelServer.GuildManager.OnGuildLeaveAlliance(response);
        }

        public override void BroadcastAllianceExpelGuild(AllianceExpelGuildResponse response)
        {
            ChannelServer.GuildManager.OnAllianceExpelGuild(response);
        }

        public override void BroadcastAllianceRankTitleChanged(UpdateAllianceRankTitleResponse response)
        {
            ChannelServer.GuildManager.OnAllianceRankTitleChanged(response);
        }

        public override void BroadcastAllianceNoticeChanged(UpdateAllianceNoticeResponse response)
        {
            ChannelServer.GuildManager.OnAllianceNoticeChanged(response);
        }

        public override void BroadcastAllianceLeaderChanged(AllianceChangeLeaderResponse response)
        {
            ChannelServer.GuildManager.OnAllianceLeaderChanged(response);
        }

        public override void BroadcastAllianceMemberRankChanged(ChangePlayerAllianceRankResponse response)
        {
            ChannelServer.GuildManager.OnPlayerAllianceRankChanged(response);
        }

        public override void BroadcastAllianceDisband(DisbandAllianceResponse response)
        {
            ChannelServer.GuildManager.OnAllianceDisband(response);
        }

        #endregion
    }
}
