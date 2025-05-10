using Application.Core.Game.Relation;
using Application.Core.Game.TheWorld;
using Application.Core.Login.Relations;
using Application.Shared.Relations;
using Application.Shared.Servers;
using constants.id;
using Microsoft.Extensions.Logging;
using net.server;
using Serilog;

namespace Application.Core.Login
{
    public partial class MasterServer
    {
        public Dictionary<int, ITeamGlobal> TeamStorage { get; } = new();
        int runningPartyId = 1000000001;
        object teamLock = new object();
        public int CreateTeam(int playerId)
        {
            lock (teamLock)
            {
                var teamId = runningPartyId++;
                var teamModel = new TeamGlobal(teamId, playerId);
                TeamStorage[teamId] = teamModel;
                Transport.SyncTeam(teamModel);
                return teamId;
            }
        }

        public ITeamGlobal? GetParty(int teamId)
        {
            return TeamStorage.GetValueOrDefault(teamId);
        }

        private bool DisbandParty(int teamId)
        {
            return TeamStorage.Remove(teamId);
        }

        private bool AddPartyMember(int teamId, int playerId)
        {
            if (TeamStorage.TryGetValue(teamId, out var party))
            {
                return party.AddMember(playerId);
            }
            return false;
        }

        private bool RemovePartyMember(int teamId, int playerId)
        {
            if (TeamStorage.TryGetValue(teamId, out var party))
            {
                return party.RemoveMember(playerId);
            }
            return false;
        }

        public void ExpelFromParty(int partyId, int expelCid)
        {
            if (TeamStorage.TryGetValue(partyId, out var party))
            {
                Transport.SendExpelFromParty(partyId, expelCid);
            }
        }

        private bool ChangeLeader(int teamId, int playerId)
        {
            if (TeamStorage.TryGetValue(teamId, out var party))
            {
                return party.ChangeLeader(playerId);
            }
            return false;
        }

        public void UpdateTeamGlobalData(int partyId, PartyOperation operation, int targetId, string targetName)
        {
            bool updateResult = false;
            // 更新主数据
            switch (operation)
            {
                case PartyOperation.JOIN:
                    updateResult = AddPartyMember(partyId, targetId);
                    break;
                case PartyOperation.EXPEL:
                case PartyOperation.LEAVE:
                    updateResult = RemovePartyMember(partyId, targetId);
                    break;
                case PartyOperation.DISBAND:
                    updateResult = DisbandParty(partyId);
                    break;
                case PartyOperation.SILENT_UPDATE:
                case PartyOperation.LOG_ONOFF:
                    updateResult = true;
                    break;
                case PartyOperation.CHANGE_LEADER:
                    updateResult = ChangeLeader(partyId, targetId);
                    break;
                default:
                    Log.Logger.Warning("Unhandled updateParty operation: {PartyOperation}", operation.ToString());
                    break;
            }
            if (updateResult)
            {
                Transport.UpdateTeamChannelData(partyId, operation, GlobalTools.Mapper.Map<TeamMember>(AllPlayerStorage.GetOrAddCharacterById(targetId))!);
            }
            else
            {
                _logger.LogError("[队伍数据]操作失败");
            }
        }
        public void BroadcastTeamMessage(int teamId, string from, string message)
        {
            Transport.SendTeamMessage(teamId, from, message);
        }
    }
}
