namespace Application.Core.Game.Players
{
    public interface ITeamPlayer
    {
        bool JoinParty(int partyId, bool silentCheck);
        bool LeaveParty(bool disbandTeam = true);
        bool CreateParty(bool silentCheck);
        void ExpelFromParty(int expelCid);
        void TeamChat(string message);
    }
}
