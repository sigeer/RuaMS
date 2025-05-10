namespace Application.Core.Game.TheWorld
{
    /// <summary>
    /// 对worldchannel的拆分，不被worldchannel直接调用
    /// </summary>
    public interface IWorldChannelProcessor
    {
        void ProcessExpelFromParty(int partyId, int expelCid);
        void ProcessUpdateTeamChannelData(int partyId, PartyOperation operation, TeamMember targetMember);
        void ProcessBroadcastTeamMessage(int teamId, string from, string message);
    }
}
