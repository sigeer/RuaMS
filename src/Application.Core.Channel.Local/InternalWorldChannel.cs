using Application.Core.Login.Servers;
using Application.Shared.Constants.Job;
using Application.Shared.Team;
using Dto;
using net.server.guild;
using static Mysqlx.Notice.Warning.Types;
using System.Xml.Linq;

namespace Application.Core.Channel.Local
{
    public class InternalWorldChannel : ChannelServerWrapper
    {
        public InternalWorldChannel(WorldChannel worldChannel) : base(worldChannel.InstanceId, worldChannel.ChannelConfig)
        {
            WorldChannel = worldChannel;
        }

        public WorldChannel WorldChannel { get; }

        public override void SendTeamUpdate(int teamId, PartyOperation operation, TeamMemberDto target)
        {
            WorldChannel.TeamManager.ProcessUpdateResponse(WorldChannel, teamId, operation, target);
        }

        public override void BroadcastJobChanged(int type, int[] players, string name, int jobId)
        {
            WorldChannel.Service.ProcessBroadcastJobChanged(type, players, name, jobId);
        }

        public override void BroadcastLevelChanged(int type, int[] value, string name, int level)
        {
            WorldChannel.Service.ProcessBroadcastLevelChanged(type, value, name, level);
        }

        public override void SendGuildUpdate(UpdateGuildResponse response)
        {
            WorldChannel.GuildManager.ProcessUpdateGuild(WorldChannel, response);
        }

        public override void SendAllianceUpdate(UpdateAllianceResponse response)
        {
            WorldChannel.GuildManager.ProcessAllianceUpdate(WorldChannel, response);
        }
    }
}
