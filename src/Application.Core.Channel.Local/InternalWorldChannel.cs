using Application.Core.Login.Servers;
using Application.Shared.Team;
using Dto;

namespace Application.Core.Channel.Local
{
    public class InternalWorldChannel : ChannelServerWrapper
    {
        public InternalWorldChannel(WorldChannel worldChannel) : base(worldChannel.InstanceId, worldChannel.ServerConfig)
        {
            WorldChannel = worldChannel;
        }

        public WorldChannel WorldChannel { get; }

        public override void SendTeamUpdate(int teamId, PartyOperation operation, TeamMemberDto target)
        {
            WorldChannel.TeamManager.ProcessUpdateResponse(teamId, operation, target);
        }
    }
}
