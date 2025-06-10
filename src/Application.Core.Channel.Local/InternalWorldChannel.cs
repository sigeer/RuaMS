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

        public override void BroadcastJobChanged(int type, int[] players, string name, int jobId)
        {
            ChannelServer.ProcessBroadcastJobChanged(type, players, name, jobId);
        }

        public override void BroadcastLevelChanged(int type, int[] value, string name, int level)
        {
            ChannelServer.ProcessBroadcastLevelChanged(type, value, name, level);
        }

        public override void SendGuildUpdate(UpdateGuildResponse response)
        {
            ChannelServer.GuildManager.ProcessUpdateGuild(response);
        }

        public override void SendAllianceUpdate(UpdateAllianceResponse response)
        {
            ChannelServer.GuildManager.ProcessAllianceUpdate(response);
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
    }
}
