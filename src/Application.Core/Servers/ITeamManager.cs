using Application.Core.Game.Relation;
using Application.Shared.Relations;

namespace Application.Core.Servers
{
    public interface ITeamManager
    {
        Dictionary<int, ITeamGlobal> TeamStorage { get; }
        int CreateTeam(int playerId);
        ITeamGlobal? GetParty(int teamId);
        void UpdateTeamGlobalData(int partyId, PartyOperation operation, int targetId, string targetName);
    }
}
