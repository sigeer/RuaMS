using Application.Core.Game.Relation;
using Application.Core.model;
using Application.Shared.Configs;
using Application.Shared.Relations;
using Application.Shared.Servers;

namespace Application.Core.ServerTransports
{
    public interface IMasterServerTransport : IServerTransport
    {
        void SendServerMessage(IEnumerable<int> playerIdList);
        CoupleIdPair? GetAllWeddingCoupleForGuest(int guestId, bool cathedral);
        int GetAllWeddingReservationStatus(IEnumerable<int> pw, bool cathedral);
        void SendWorldConfig(WorldConfigPatch patch);

        #region Team
        /// <summary>
        /// 分发给频道服务器同步队伍数据
        /// </summary>
        /// <param name="teamGlobal"></param>
        void SyncTeam(ITeamGlobal teamGlobal);
        void SendExpelFromParty(int partyId, int expelCid);
        void UpdateTeamChannelData(int partyId, PartyOperation operation, TeamMember targetMember);
        void SendTeamMessage(int teamId, string from, string message);
        #endregion
    }
}
