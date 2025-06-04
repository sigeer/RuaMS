using Application.Shared.Team;
using Dto;

namespace Application.Core.Login.ServerTransports
{
    public interface IChannelBroadcast
    {
        void SendTeamUpdate(int teamId, PartyOperation operation, TeamMemberDto target);
    }
}
