using Application.Core.Login.Models;
using Application.Shared.Team;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Application.Core.Login.ServerData
{
    public class TeamManager : IDisposable
    {
        ConcurrentDictionary<int, TeamModel> _dataSource = new();
        private int _currentId = 1000000001;

        readonly MasterServer _server;
        IMapper _mapper;
        ILogger<TeamManager> _logger;

        public TeamManager(MasterServer server, IMapper mapper, ILogger<TeamManager> logger)
        {
            _server = server;
            _mapper = mapper;
            _logger = logger;
        }

        public TeamProto.TeamDto? GetTeamFull(int teamId)
        {
            var data = GetTeamLocal(teamId);
            if (data == null)
                return null;

            var response = new TeamProto.TeamDto();
            response.Id = teamId;
            response.LeaderId = data.LeaderId;
            response.Members.AddRange(_mapper.Map<TeamProto.TeamMemberDto[]>(data.GetMembers().Select(_server.CharacterManager.FindPlayerById)));
            return response;
        }
        public TeamModel? GetTeamLocal(int teamId) => _dataSource.GetValueOrDefault(teamId);

        public TeamProto.TeamDto CreateTeam(int playerId)
        {
            var newTeam = new TeamModel(Interlocked.Increment(ref _currentId), playerId);
            var chrFrom = _server.CharacterManager.FindPlayerById(playerId)!;
            chrFrom.Character.Party = newTeam.Id;

            var response = new TeamProto.TeamDto();
            response.Id = newTeam.Id;
            response.LeaderId = newTeam.LeaderId;
            response.Members.AddRange(_mapper.Map<TeamProto.TeamMemberDto[]>(new CharacterLiveObject[] { chrFrom }));
            _dataSource[newTeam.Id] = newTeam;
            return response;
        }
        public bool RemoveTeam(int leaderId, int teamId)
        {
            if (_dataSource.TryGetValue(teamId, out var d) && leaderId == d.LeaderId)
                return _dataSource.TryRemove(teamId, out _);
            return false;
        }
        public TeamProto.UpdateTeamResponse UpdateParty(int partyid, PartyOperation operation, int fromId, int toId)
        {
            var response = new TeamProto.UpdateTeamResponse();
            UpdateTeamCheckResult errorCode = UpdateTeamCheckResult.Success;

            var party = GetTeamLocal(partyid);
            if (party == null)
                errorCode = UpdateTeamCheckResult.TeamNotExsited;
            else
            {
                var chrFrom = fromId > 0 ? _server.CharacterManager.FindPlayerById(fromId) : null;
                var chrTo = fromId == toId ? chrFrom : _server.CharacterManager.FindPlayerById(toId)!;
                response.UpdatedMember = _mapper.Map<TeamProto.TeamMemberDto>(chrTo);
                switch (operation)
                {
                    case PartyOperation.JOIN:
                        if (chrFrom!.Character.Party > 0)
                            errorCode = UpdateTeamCheckResult.Join_HasTeam;
                        if (party.TryAddMember(toId, out errorCode))
                            chrFrom.Character.Party = partyid;
                        break;
                    case PartyOperation.EXPEL:
                        if (party.TryExpel(fromId, toId, out errorCode))
                            chrTo!.Character.Party = 0;
                        break;
                    case PartyOperation.LEAVE:
                        if (fromId == party.LeaderId)
                        {
                            operation = PartyOperation.DISBAND;
                            goto case PartyOperation.DISBAND;
                        }
                        if (party.TryRemoveMember(fromId, out errorCode))
                            chrFrom!.Character.Party = 0;
                        break;
                    case PartyOperation.DISBAND:
                        if (fromId != party.LeaderId)
                            errorCode = UpdateTeamCheckResult.Disband_NotLeader;
                        else if (!RemoveTeam(fromId, partyid))
                            errorCode = UpdateTeamCheckResult.Leave_InnerError;
                        else
                        {
                            var allMember = party.GetMembers().Select(_server.CharacterManager.FindPlayerById).ToArray();
                            foreach (var member in allMember)
                            {
                                if (member != null)
                                    member.Character.Party = 0;
                            }
                        }
                        break;
                    case PartyOperation.SILENT_UPDATE:
                        break;
                    case PartyOperation.LOG_ONOFF:
                        // fromId = -1 表示下线
                        break;
                    case PartyOperation.CHANGE_LEADER:
                        party.TryChangeLeader(fromId, toId, out errorCode);
                        break;
                    default:
                        _logger.LogWarning("Unhandled updateParty operation: {PartyOperation}", operation.ToString());
                        break;
                }
            }
            response.TeamId = partyid;
            response.Operation = (int)operation;
            response.ErrorCode = (int)errorCode;
            response.OperatorId = fromId;

            if (errorCode == UpdateTeamCheckResult.Success)
                _server.Transport.BroadcastTeamUpdate(response);
            return response;
        }

        public void SendTeamChat(string nameFrom, string chatText)
        {
            var sender = _server.CharacterManager.FindPlayerByName(nameFrom);
            if (sender != null)
            {
                if (_dataSource.TryGetValue(sender.Character.Party, out var team))
                {
                    var teamMember = team.GetMembers().Where(x => x != sender.Character.Id)
                        .Select(x => _server.CharacterManager.FindPlayerById(x)).Where(x => x != null)
                        .Select(x => new PlayerChannelPair(x.Channel, x.Character.Id)).ToArray();
                    _server.Transport.SendMultiChat(1, nameFrom, teamMember, chatText);
                }
            }
        }

        public void Dispose()
        {
            _dataSource.Clear();
        }
    }
}
